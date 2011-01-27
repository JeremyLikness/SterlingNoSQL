using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using Wintellect.Sterling.Database;
using Wintellect.Sterling.Exceptions;
using System.Collections.Generic;

namespace Wintellect.Sterling.Serialization
{
    /// <summary>
    ///     This class assists with the serialization and de-serialization of objects
    /// </summary>
    /// <remarks>
    ///     This is where the heavy lifting is done, and likely where most of the tweaks make sense
    /// </remarks>
    internal class SerializationHelper
    {
        // a few constants to serialize null values to the stream
        private const ushort NULL = 0;
        private const ushort NOTNULL = 1;
        const string LIST_TYPE_ITEM = "Item";                    

        /// <summary>
        ///     "Remember" how lists map to avoid reflecting each time
        /// </summary>
        private static readonly Dictionary<Type, Type> _listTypes = new Dictionary<Type, Type>();
        private static readonly Dictionary<Type, Tuple<Type, Type>> _dictTypes = new Dictionary<Type, Tuple<Type, Type>>();

        /// <summary>
        ///     The import cache, stores what properties are available and how to access them
        /// </summary>
        private static readonly
            Dictionary<Type, List<SerializationCache>>
            _propertyCache =
                new Dictionary
                    <Type, List<SerializationCache>>();

        private readonly ISterlingDatabaseInstance _database;
        private readonly ISterlingSerializer _serializer;
        private readonly LogManager _logManager;
        private readonly Func<string, int> _typeResolver = s => 1;
        private readonly Func<int, string> _typeIndexer = i => string.Empty;

        /// <summary>
        ///     Checks to see if it is a generic list, and, if it is, returns the type of the list
        /// </summary>
        /// <param name="type">The type</param>
        /// <returns>NULL if not a generic list</returns>
        private static Type _IsGenericList(Type type)
        {
            if (type == null)
            {
                return null;
            }

            if (_listTypes.ContainsKey(type))
            {
                return _listTypes[type];
            }

            Type listType = (from @interface in type.GetInterfaces()
                             where @interface.IsGenericType
                                   && (@interface.GetGenericTypeDefinition() == typeof(ICollection<>)
                                       || @interface.GetGenericTypeDefinition() == typeof(IList<>)
                                       || @interface.GetGenericTypeDefinition() == typeof(IList))
                             select @interface.GetGenericArguments()[0]).FirstOrDefault();

            if (listType == null)
            {
                return listType;
            }

            if (!_listTypes.ContainsKey(type))
            {
                lock (((ICollection) _listTypes).SyncRoot)
                {
                    if (!_listTypes.ContainsKey(type))
                    {
                        _listTypes.Add(type, listType);
                    }
                }
            }

            return listType;
        }

        private static bool _IsArray(Type type)
        {
            return type.IsArray;
        }

        private static bool _IsGenericDictionary(Type type, out Type keyType, out Type valueType)
        {
            keyType = null;
            valueType = null;

            if (type == null)
            {
                return false;
            }

            if (_dictTypes.ContainsKey(type))
            {
                keyType = _dictTypes[type].Item1;
                valueType = _dictTypes[type].Item2;

                return true;
            }

            var gTypes = (from @interface in type.GetInterfaces()
                          where @interface.IsGenericType &&
                          @interface.GetGenericTypeDefinition() == typeof(IDictionary<,>)
                          select @interface.GetGenericArguments()).FirstOrDefault();

            if (gTypes == null || gTypes.Length != 2)
            {
                return false;
            }

            keyType = gTypes[0];
            valueType = gTypes[1];

            if (!_dictTypes.ContainsKey(type))
            {
                lock (((ICollection) _dictTypes).SyncRoot)
                {
                    if (!_dictTypes.ContainsKey(type))
                    {
                        _dictTypes.Add(type, new Tuple<Type, Type>(gTypes[0], gTypes[1]));
                    }
                }
            }

            return true;
        }

        /// <summary>
        ///     Cache the properties for a type so we don't reflect every time
        /// </summary>
        /// <param name="type">The type to manage</param>
        /// <param name="instance"></param>
        private void _CacheProperties(Type type, object instance)
        {
            Type valueType = null;

            lock (((ICollection)_propertyCache).SyncRoot)
            {
                // fast "out" if already exits
                if (_propertyCache.ContainsKey(type)) return;

                _propertyCache.Add(type,
                                   new List<SerializationCache>());

                // first fields
                var fields = from f in type.GetFields()
                             where !f.IsIgnored() && !f.FieldType.IsIgnored()
                             select new PropertyOrField(f);               

                var properties = from p in type.GetProperties()
                                 where p.GetGetMethod() != null && p.GetSetMethod() != null
                                       && !p.IsIgnored() && !p.PropertyType.IsIgnored()
                                 select new PropertyOrField(p);                                 

                foreach (var p in properties.Concat(fields))
                {                    
                    var propType = p.PfType;                    

                    if (propType.IsGenericType && propType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
                    {
                        propType = Nullable.GetUnderlyingType(propType);
                    }
                    else if (propType.IsEnum)
                    {
                        propType = Enum.GetUnderlyingType(propType);
                    }

                    if (p.Name.Equals(LIST_TYPE_ITEM) && p.DeclaringType.GetInterfaces().Contains(typeof(IList)))
                    {
                        _propertyCache[type].Add(
                            new SerializationCache(
                                p.DeclaringType,
                                propType, PropertyType.List,
                                (parent, property) => new object(),
                                parent => parent));
                        continue;
                    }

                    var value = p.GetValue(instance);

                    // Try to get the value's type. PropertyType could be abstract

                    if (value != null)
                    {
                        valueType = value.GetType();
                    }

                    // ignored type?
                    if (valueType != null && valueType.IsIgnored())
                    {
                        continue;
                    }                                        

                    // this is registered "keyed" type, so we serialize the foreign key
                    if (_database.IsRegistered(propType) || (value != null && _database.IsRegistered(valueType)))
                    {
                        var p1 = p;
                        var v = _database.IsRegistered(propType) ? propType : valueType;
                        _propertyCache[type].Add(
                            new SerializationCache(
                            //p.PropertyType,
                                v,
                                null,
                                PropertyType.Class,
                                p1.Setter,                                
                                p1.Getter));
                        continue;
                    }
                    
                    // this is a property
                    if (_serializer.CanSerialize(propType))
                    {
                        var p1 = p;

                        var getter = p1.Getter;

                        // cast to underlying type
                        if (p.PfType.IsEnum)
                        {
                            getter = parent => Convert.ChangeType(p1.GetValue(instance),
                                                                  propType, null);
                        }

                        _propertyCache[type].Add(
                            new SerializationCache(
                                propType,
                                null,
                                PropertyType.Property,
                                p1.Setter,
                                getter));
                        continue;
                    }
                    
                    if (_IsArray(p.PfType))
                    {
                        var p1 = p;
                        _propertyCache[type].Add(
                            new SerializationCache(
                                p.PfType,
                                p.PfType.GetElementType(),
                                PropertyType.Array,
                                p1.Setter,
                                p1.Getter));
                        continue;
                    }
                    
                    Type dictKeyType;
                    Type dictValueType;
                    if (_IsGenericDictionary(p.PfType, out dictKeyType, out dictValueType))
                    {
                        var p1 = p;
                        _propertyCache[type].Add(
                            new SerializationCache(
                                p.PfType,
                                dictKeyType,
                                dictValueType,
                                p1.Setter,
                                p1.Getter));
                        continue;
                    }
                    
                    Type listType;
                    if ((listType = _IsGenericList(propType)) != null)
                    {
                        // check if we can handle this as a list

                        // try to get the type of each object of the list
                        var v = p.GetValue(instance);
                        
                        var ie = v as IEnumerable;

                        if (ie != null)
                        {
                            var cansave = ie.Cast<object>().All(o => _database.IsRegistered(o.GetType()));

                            if (cansave || _database.IsRegistered(listType) || _serializer.CanSerialize(listType))
                            {
                                var p1 = p;
                                _propertyCache[type].Add(
                                    new SerializationCache(
                                        p.PfType,
                                        listType, PropertyType.List,
                                        p1.Setter,
                                        p1.Getter));
                            }
                            continue;
                        }
                    }

                    // nothing else, so we'll call it complex and recurse
                    var pLocal = p;
                    _propertyCache[type].Add(
                                    new SerializationCache(
                                        p.PfType,
                                        listType, PropertyType.ComplexType,
                                        pLocal.Setter,
                                        pLocal.Getter));
                }
            }
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="database">Database this is a helper for</param>
        /// <param name="serializer">The serializer</param>
        /// <param name="logManager">The logger</param>
        /// <param name="typeResolver"></param>
        /// <param name="typeIndexer"></param>
        public SerializationHelper(ISterlingDatabaseInstance database, ISterlingSerializer serializer,
                                   LogManager logManager, Func<string,int> typeResolver, Func<int,string> typeIndexer)
        {
            _database = database;
            _serializer = serializer;
            _logManager = logManager;
            _typeResolver = typeResolver;
            _typeIndexer = typeIndexer;
        }

        /// <summary>
        ///     Recursive save operation
        /// </summary>
        /// <param name="type">The type to save (passed to support NULL)</param>
        /// <param name="instance">The instance to type</param>
        /// <param name="bw">The writer to save it to</param>
        /// <param name="cache">Cycle cache</param>
        public void Save(Type type, object instance, BinaryWriter bw, CycleCache cache)
        {
            _logManager.Log(SterlingLogLevel.Verbose, string.Format("Sterling is serializing type {0}", type.FullName),
                            null);

            // need to indicate to the stream whether or not this is null
            var nullFlag = instance == null ? NULL : NOTNULL;

            bw.Write(nullFlag);

            if (instance == null) return;

            // build the cache for reflection
            if (!_propertyCache.ContainsKey(type))
            {
                //_CacheProperties(type);
                _CacheProperties(type, instance);
            }

            // now iterate the serializable properties
            foreach (var p in _propertyCache[type])
            {
                if (p.SerializationType.Equals(PropertyType.Class))
                {
                    // foreign table - write if it is null or not, and if not null, write the key
                    // then serialize it separately
                    _SerializeClass(type, p.GetMethod(instance), bw, cache);
                    continue;
                }
                
                if (p.SerializationType.Equals(PropertyType.Property))
                {
                    _SerializeProperty(type, p.GetMethod(instance), bw, p.PropType);
                    continue;
                }
                
                if (p.SerializationType.Equals(PropertyType.List) || p.SerializationType.Equals(PropertyType.Array))
                {
                    _SerializeList(p.ListType, (IList)p.GetMethod(instance), bw, cache);
                    continue;
                }
                
                if (p.SerializationType.Equals(PropertyType.Dictionary))
                {
                    _SerializeDictionary(p.DictionaryKeyType, p.DictionaryValueType, (IDictionary)p.GetMethod(instance), bw, cache);
                    continue;
                }

                if (!p.SerializationType.Equals(PropertyType.ComplexType)) continue;

                // complex type only
                var propertyValue = p.GetMethod(instance);
                var propertyType = propertyValue == null ? p.PropType : propertyValue.GetType();
                bw.Write(_typeResolver(propertyType.AssemblyQualifiedName));
                Save(propertyType, propertyValue, bw, cache);
            }
        }

        /// <summary>
        ///     Handles serialization of a list
        /// </summary>
        /// <param name="listType">The type of elements in the list</param>
        /// <param name="instance">The list to serialize</param>
        /// <param name="bw">The stream to serialize to</param>
        /// <param name="cache">Cache</param>
        private void _SerializeList(Type listType, IList instance, BinaryWriter bw, CycleCache cache)
        {
            var count = instance == null ? 0 : instance.Count;

            // always pass the count (if it's null we'll just re-serialize an empty list)
            bw.Write(count);

            if (instance == null || count <= 0) return;

            Action<object> serialize;

            //serialize to database (as class) or as property?
            if (_database.IsRegistered(listType) || !_serializer.CanSerialize(listType))
            {
                serialize = obj => _SerializeClass(listType, obj, bw, cache);
            }
            else
            {
                serialize = obj => _SerializeProperty(listType, obj, bw, listType);
            }

            // now iterate
            foreach (var item in instance)
            {
                serialize(item);
            }
        }

        /// <summary>
        ///     Handles serialization of a dictionary
        /// </summary>
        /// <param name="keyType">The type of dictionary's Key </param>
        /// <param name="valueType">The type of dictionary's Value</param>
        /// <param name="instance">The dictionary to serialize</param>
        /// <param name="bw">The stream to serialize to</param>
        /// <param name="cache">Cycle cache</param>
        private void _SerializeDictionary(Type keyType, Type valueType, IDictionary instance, BinaryWriter bw, CycleCache cache)
        {
            Type keyListType, valueListType;
            var count = instance == null ? 0 : instance.Count;

            // always pass the count (if it's null we'll just re-serialize an empty list)
            bw.Write(count);

            if (instance == null || count <= 0)
                return;

            Action<object> serializeKey, serializeValue;

            if ((keyListType = _IsGenericList(keyType)) == null)
            {
                //serialize to database (as class) or as property?
                if (_database.IsRegistered(keyType) || !_serializer.CanSerialize(keyType))
                {
                    serializeKey = obj => _SerializeClass(keyType, obj, bw, cache);
                }
                else
                {
                    serializeKey = obj => _SerializeProperty(keyType, obj, bw, keyType);
                }
            }
            else
            {
                serializeKey = obj => _SerializeList(keyListType, (IList)obj, bw, cache);
            }

            if ((valueListType = _IsGenericList(valueType)) == null)
            {
                //serialize to database (as class) or as property?
                if (_database.IsRegistered(valueType) || !_serializer.CanSerialize(valueType))
                {
                    serializeValue = obj => _SerializeClass(keyType, obj, bw, cache);
                }
                else
                {
                    serializeValue = obj => _SerializeProperty(valueType, obj, bw, valueType);
                }
            }
            else
            {
                serializeValue = obj => _SerializeList(valueListType, (IList)obj, bw, cache);
            }

            foreach (var key in instance.Keys)
            {
                serializeKey(key);
                serializeValue(instance[key]);
            }
        }

        /// <summary>
        ///     Serializes a property
        /// </summary>
        /// <param name="type">The parent type</param>
        /// <param name="propertyValue">The property value</param>
        /// <param name="bw">The writer</param>
        /// <param name="p">The type of the property</param>
        private void _SerializeProperty(Type type, object propertyValue, BinaryWriter bw, Type p)
        {
            if (propertyValue == null)
            {
                bw.Write(NULL);
                _logManager.Log(SterlingLogLevel.Verbose,
                            string.Format("Sterling is saving property of type {0} with value NULL for parent {1}",
                                          p.FullName, type.FullName), null);
                return;
            }

            bw.Write(NOTNULL);

            _logManager.Log(SterlingLogLevel.Verbose,
                            string.Format("Sterling is saving property of type {0} with value {1} for parent {2}",
                                          p.FullName, propertyValue, type.FullName), null);
            _serializer.Serialize(propertyValue, bw);
        }

        /// <summary>
        ///     Serialize a class
        /// </summary>
        /// <param name="type">The type</param>
        /// <param name="foreignTable">The referenced type</param>
        /// <param name="bw">The writer</param>
        /// <param name="cache">Cycle cache</param>
        private void _SerializeClass(Type type, object foreignTable, BinaryWriter bw, CycleCache cache)
        {
            // serialize to the stream if the foreign key is nulled
            bw.Write(foreignTable == null ? NULL : NOTNULL);

            if (foreignTable == null) return;

            // aggiungo il tipo dell'oggetto da serializzare            
            bw.Write(_typeResolver(foreignTable.GetType().AssemblyQualifiedName));

            // if not null, serialize the key value to look up when we load back
            var foreignKey = _database.GetKey(foreignTable);

            // need to be able to serialize the key 
            if (!_serializer.CanSerialize(foreignKey.GetType()))
            {
                var exception = new SterlingSerializerException(_serializer, foreignKey.GetType());
                _logManager.Log(SterlingLogLevel.Error, exception.Message, exception);
                throw exception;
            }

            _logManager.Log(SterlingLogLevel.Verbose,
                            string.Format(
                                "Sterling is saving foreign key of type {0} with value {1} for parent {2}",
                                foreignKey.GetType().FullName, foreignKey, type.FullName), null);

            _serializer.Serialize(foreignKey, bw);
            _database.Save(foreignTable.GetType(), foreignTable, cache);
        }

        /// <summary>
        ///     Recursive load operation
        /// </summary>
        /// <param name="type">The type to save (passed to support NULL)</param>
        /// <param name="key">The associated key (for cycle detection)</param>
        /// <param name="br">The reader</param>
        /// <param name="cache">Cycle cache</param>
        public object Load(Type type, object key, BinaryReader br, CycleCache cache)
        {
            _logManager.Log(SterlingLogLevel.Verbose,
                            string.Format("Sterling is de-serializing type {0}", type.FullName), null);

            // are we de-serializing something that is null?
            var nullFlag = br.ReadUInt16();

            // that's it
            if (nullFlag == NULL)
            {
                _logManager.Log(SterlingLogLevel.Verbose, "De-serialized NULL value.", null);
                return null;
            }

            // make a template
            var instance = Activator.CreateInstance(type);

            // push to the stack
            cache.Add(type, instance, key);            

            // build the reflection cache);
            if (!_propertyCache.ContainsKey(type))
            {
                //_CacheProperties(type);
                _CacheProperties(type, instance);
            }

            // now iterate
            foreach (var p in _propertyCache[type])
            {
                // recursive save? 
                if (p.SerializationType.Equals(PropertyType.Class))
                {
                    p.SetMethod(instance, _DeserializeClass(type, p.PropType, br, cache));
                    continue;
                }
                
                if (p.SerializationType.Equals(PropertyType.Property))
                {
                    p.SetMethod(instance, _DeserializeProperty(type, br, p.PropType));
                    continue;
                }
                
                if (p.SerializationType.Equals(PropertyType.List))
                {
                    // We've to support interfaces (ie: IList<MyClass>)
                    // build the base list (this will be List<T> for example because we know the exact type)
                    var list = (IList)Activator.CreateInstance(p.PropType);
                    if (instance.GetType().GetInterfaces().Contains(typeof(IList)))
                    {
                        _DeserializeList(type, p.ListType, (IList)instance, br, p.PropType, cache);
                    }
                    else
                    {
                        p.SetMethod(instance, list);
                        _DeserializeList(type, p.ListType, list, br, p.PropType, cache);
                    }
                    continue;
                }
                
                if (p.SerializationType.Equals(PropertyType.Array))
                {
                    Array array;
                    _DeserializeArray(type, p.ListType, out array, br, p.PropType, cache);
                    p.SetMethod(instance, array);
                    continue;
                }
                
                if (p.SerializationType.Equals(PropertyType.Dictionary))
                {
                    var dict = (IDictionary)Activator.CreateInstance(p.PropType);
                    p.SetMethod(instance, dict);
                    _DeserializeDictionary(type, p.DictionaryKeyType, p.DictionaryValueType, dict, br, p.PropType, cache);
                    continue;
                }

                if (!p.SerializationType.Equals(PropertyType.ComplexType)) continue;

                // case for complex type
                var propertyTypeIndex = br.ReadInt32();
                var propertyType = Type.GetType(_typeIndexer(propertyTypeIndex));
                p.SetMethod(instance, Load(propertyType, Guid.NewGuid(), br, cache)); // dummy key for the cache
            }

            return instance;
        }

        /// <summary>
        ///     De-serialize an array
        /// </summary>
        /// <param name="parentType">The parent type ("owns the array")</param>
        /// <param name="arrayType">The type of array elements</param>
        /// <param name="instance">The array to build</param>
        /// <param name="br">The reader</param>
        /// <param name="p">The full type of the array (the array itself, not the elements)</param>
        /// <param name="cache">Cycle cache</param>
        private void _DeserializeArray(Type parentType, Type arrayType, out Array instance, BinaryReader br, Type p, CycleCache cache)
        {
            var idx = br.ReadInt32();
            instance = Array.CreateInstance(arrayType, idx);

            for (var i = 0; i < idx; i++)
            {
                var obj = _database.IsRegistered(arrayType) || !_serializer.CanSerialize(arrayType)
                                 ? _DeserializeClass(parentType, arrayType, br, cache)
                                 : _DeserializeProperty(p, br, arrayType);

                instance.SetValue(obj, i);
            }
        }

        /// <summary>
        ///     De-serialize a dictionary
        /// </summary>
        /// <param name="parentType">The parent type ("owns the dictionary")</param>
        /// <param name="keyType">The type of dictionary's Key </param>
        /// <param name="valueType">The type of dictionary's Value</param>
        /// <param name="instance">The dictionary to build</param>
        /// <param name="br">The reader</param>
        /// <param name="p">The full type of the dictionary (the dictionary itself, not the elements)</param>
        /// <param name="cache">Cycle cache</param>
        private void _DeserializeDictionary(Type parentType, Type keyType, Type valueType,
            IDictionary instance, BinaryReader br, Type p, CycleCache cache)
        {
            var idx = br.ReadInt32();

            for (var i = 0; i < idx; i++)
            {
                Type listKeyType;
                object key;
                if ((listKeyType = _IsGenericList(keyType)) == null)
                {
                    key = _database.IsRegistered(keyType) || !_serializer.CanSerialize(keyType)
                                     ? _DeserializeClass(parentType, keyType, br, cache)
                                     : _DeserializeProperty(p, br, keyType);
                }
                else
                {
                    key = Activator.CreateInstance(keyType);
                    _DeserializeList(parentType, listKeyType, (IList)key, br, p, cache);
                }

                Type listValueType;
                object value;
                if ((listValueType = _IsGenericList(valueType)) == null)
                {
                    value = _database.IsRegistered(valueType) || !_serializer.CanSerialize(valueType)
                                 ? _DeserializeClass(parentType, valueType, br, cache)
                                 : _DeserializeProperty(p, br, valueType);
                }
                else
                {
                    value = Activator.CreateInstance(valueType);
                    _DeserializeList(parentType, listValueType, (IList)value, br, p, cache);
                }

                instance.Add(key, value);
            }
        }

        /// <summary>
        ///     De-serialize a list
        /// </summary>
        /// <param name="parentType">The parent type ("owns the list")</param>
        /// <param name="listType">The type of elements in the list</param>
        /// <param name="instance">The list to build</param>
        /// <param name="br">The reader</param>
        /// <param name="p">The full type of the list (the list itself, not the elements)</param>
        /// <param name="cache">Cycle cache</param>
        private void _DeserializeList(Type parentType, Type listType, IList instance, BinaryReader br, Type p, CycleCache cache)
        {
            var idx = br.ReadInt32();

            for (var i = 0; i < idx; i++)
            {
                var obj = _database.IsRegistered(listType) || !_serializer.CanSerialize(listType)
                                 ? _DeserializeClass(parentType, listType, br, cache)
                                 : _DeserializeProperty(p, br, listType);
                instance.Add(obj);
            }
        }

        /// <summary>
        ///     Deserialize a property
        /// </summary>
        /// <param name="type">The parent type</param>
        /// <param name="br">The reader</param>
        /// <param name="p">The property type</param>
        /// <returns>The de-serialized property</returns>
        private object _DeserializeProperty(Type type, BinaryReader br, Type p)
        {
            var isNull = _serializer.Deserialize(NULL.GetType(), br);

            if (isNull.Equals(NULL))
            {
                _logManager.Log(SterlingLogLevel.Verbose,
                            string.Format(
                                "Sterling de-serialized property of type {0} with value NULL for parent {1}",
                                p.FullName, type.FullName), null);

                return null;
            }

            var propertyValue = _serializer.Deserialize(p, br);
            _logManager.Log(SterlingLogLevel.Verbose,
                            string.Format(
                                "Sterling de-serialized property of type {0} with value {1} for parent {2}",
                                p.FullName, propertyValue, type.FullName), null);
            return propertyValue;
        }

        /// <summary>
        ///     De-serialize a class ("load a foreign table")
        /// </summary>
        /// <param name="type">The parent type</param>
        /// <param name="targetType">The foreign type</param>
        /// <param name="br">The reader</param>
        /// <param name="cache">Cycle cache</param>
        /// <returns>The de-serialized class</returns>
        private object _DeserializeClass(Type type, Type targetType, BinaryReader br, CycleCache cache)
        {
            _logManager.Log(SterlingLogLevel.Verbose,
                            string.Format("Sterling is de-serializing foreign key of type {0} for parent {1}",
                                          targetType.FullName, type.FullName), null);

            var foreignKeyIsNull = br.ReadInt16();

            if (foreignKeyIsNull == NULL)
            {
                // set to null
                return null;
            }

            var typeIndex = br.ReadInt32();
            targetType = Type.GetType(_typeIndexer(typeIndex));

            var keyType = _database.GetKeyType(targetType);
            return _database.Load(targetType, _serializer.Deserialize(keyType, br), cache);
        }
    }
}