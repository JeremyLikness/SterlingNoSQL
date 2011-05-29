using System;
using System.Collections;
using System.IO;
using System.Linq;
using Wintellect.Sterling.Database;
using Wintellect.Sterling.Exceptions;
using System.Collections.Generic;

namespace Wintellect.Sterling.Serialization
{
    /// <summary>
    ///     Wraps nodes for passing directly into the Save pass of the Serialization Helper
    ///     Basically just hosts another object so that the helper can recursively navigate properties
    ///     Useful in external serializers that want to re-enter the stream using the helper methods
    /// </summary>
    public class SerializationNode
    {
        public object Value { get; set; }

        public static SerializationNode WrapForSerialization(object obj)
        {
            return new SerializationNode {Value = obj};
        }

        public T UnwrapForDeserialization<T>()
        {
            return (T) Value;
        }
    }

    /// <summary>
    ///     This class assists with the serialization and de-serialization of objects
    /// </summary>
    /// <remarks>
    ///     This is where the heavy lifting is done, and likely where most of the tweaks make sense
    /// </remarks>
    public class SerializationHelper
    {
        // a few constants to serialize null values to the stream
        private const ushort NULL = 0;
        private const ushort NOTNULL = 1;
        private const string NULL_DISPLAY = "[NULL]";
        private const string NOTNULL_DISPLAY = "[NOT NULL]";
        
        /// <summary>
        ///     The import cache, stores what properties are available and how to access them
        /// </summary>
        private readonly
            Dictionary<Type, List<SerializationCache>>
            _propertyCache =
                new Dictionary
                    <Type, List<SerializationCache>>();

        private readonly Dictionary<string,Type> _typeRef = new Dictionary<string, Type>();

        private readonly ISterlingDatabaseInstance _database;
        private readonly ISterlingSerializer _serializer;
        private readonly LogManager _logManager;
        private readonly Func<string, int> _typeResolver = s => 1;
        private readonly Func<int, string> _typeIndexer = i => string.Empty;

        /// <summary>
        ///     Cache the properties for a type so we don't reflect every time
        /// </summary>
        /// <param name="type">The type to manage</param>
        private void _CacheProperties(Type type)
        {
            lock (((ICollection)_propertyCache).SyncRoot)
            {
                // fast "out" if already exists
                if (_propertyCache.ContainsKey(type)) return;

                _propertyCache.Add(type,
                                   new List<SerializationCache>());

                var isList = typeof (IList).IsAssignableFrom(type);
                var isDictionary = typeof (IDictionary).IsAssignableFrom(type);
                var isArray = typeof (Array).IsAssignableFrom(type);

                var noDerived = isList || isDictionary || isArray; 

                // first fields
                var fields = from f in type.GetFields()
                             where                              
                             !f.IsStatic &&
                             !f.IsLiteral &&
                             !f.IsIgnored(_database.IgnoreAttribute) && !f.FieldType.IsIgnored(_database.IgnoreAttribute)
                             select new PropertyOrField(f);               
                
                var properties = from p in type.GetProperties()
                                 where          
                                 ((noDerived && p.DeclaringType.Equals(type) || !noDerived)) &&
                                 p.CanRead && p.CanWrite &&
                                 p.GetGetMethod() != null && p.GetSetMethod() != null
                                       && !p.IsIgnored(_database.IgnoreAttribute) && !p.PropertyType.IsIgnored(_database.IgnoreAttribute)
                                 select new PropertyOrField(p);                                 

                foreach (var p in properties.Concat(fields))
                {                    
                    var propType = p.PfType;   
                 
                    // eagerly add to the type master
                    _typeResolver(propType.AssemblyQualifiedName);

                    var p1 = p;

                    _propertyCache[type].Add(new SerializationCache(propType, (parent, property) => p1.Setter(parent,property), p1.GetValue));                    
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
        ///     External entry point for save, used by serializers
        ///     or other methods that simply want to intercept the
        ///     serialization stream. Wraps the object in a node and
        ///     then parses recursively
        /// </summary>
        /// <remarks>
        ///     See the custom serializer test for an example
        /// </remarks>
        /// <param name="obj">The instance to save</param>
        /// <param name="bw">The writer to inject to</param>
        public void Save(object obj, BinaryWriter bw)
        {
            var node = SerializationNode.WrapForSerialization(obj);
            Save(typeof(SerializationNode), node, bw, new CycleCache(),true);
        }

        /// <summary>
        ///     Recursive save operation
        /// </summary>
        /// <param name="type">The type to save (passed to support NULL)</param>
        /// <param name="instance">The instance to type</param>
        /// <param name="bw">The writer to save it to</param>
        /// <param name="cache">Cycle cache</param>
        /// <param name="saveTypeExplicit">False if the calling method has already stored the object type, otherwise true</param>
        public void Save(Type type, object instance, BinaryWriter bw, CycleCache cache, bool saveTypeExplicit)
        {
            _logManager.Log(SterlingLogLevel.Verbose, string.Format("Sterling is serializing type {0}", type.FullName),
                            null);
            
            // need to indicate to the stream whether or not this is null
            var nullFlag = instance == null;

            _SerializeNull(bw, nullFlag);

            if (nullFlag) return;

            // build the cache for reflection
            if (!_propertyCache.ContainsKey(type))
            {
                //_CacheProperties(type);
                _CacheProperties(type);
            }

            if (typeof(Array).IsAssignableFrom(type))
            {
                _SaveArray(bw, cache, instance as Array);
            }
            else if (typeof(IList).IsAssignableFrom(type))
            {
                _SaveList(instance as IList, bw, cache);
            }
            else if (typeof(IDictionary).IsAssignableFrom(type))
            {
                _SaveDictionary(instance as IDictionary, bw, cache);              
            }
            else if (saveTypeExplicit)
            {
                bw.Write(_typeResolver(type.AssemblyQualifiedName));
            }

            // now iterate the serializable properties - create a copy to avoid multi-threaded conflicts
            foreach (var p in new List<SerializationCache>(_propertyCache[type]))
            {
                var value = p.GetMethod(instance);
                _InnerSave(value == null ? p.PropType : value.GetType(), value, bw, cache);
            }
        }

        private void _SaveList(IList list, BinaryWriter bw, CycleCache cache)
        {
            _SerializeNull(bw, list == null);

            if (list == null)
            {
                return;
            }

            bw.Write(list.Count);
            foreach(var item in list)
            {
                _InnerSave(item == null ? typeof(string) : item.GetType(), item, bw, cache);
            }
        }

        private void _SaveDictionary(IDictionary dictionary, BinaryWriter bw, CycleCache cache)
        {
            _SerializeNull(bw, dictionary == null);

            if (dictionary == null)
            {
                return;
            }

            bw.Write(dictionary.Count);
            foreach (var item in dictionary.Keys)
            {
                _InnerSave(item.GetType(), item, bw, cache);
                _InnerSave(dictionary[item] == null ? typeof(string) : dictionary[item].GetType(), dictionary[item], bw, cache);
            }
        }

        private void _SaveArray(BinaryWriter bw, CycleCache cache, Array array)
        {
            _SerializeNull(bw, array == null);

            if (array == null)
            {
                return;
            }

            bw.Write(array.Length);
            foreach (var item in array)
            {
                _InnerSave(item == null ? typeof(string) : item.GetType(), item, bw, cache);
            }
        }

        private void _InnerSave(Type type, object instance, BinaryWriter bw,  CycleCache cache)
        {                                    
            if (_database.IsRegistered(type))
            {
                // foreign table - write if it is null or not, and if not null, write the key
                // then serialize it separately
                _SerializeClass(type, instance, bw, cache);
                return;
            }
            
            if (_serializer.CanSerialize(type))
            {
                _SerializeProperty(type, instance, bw);
                return;
            }

            if (instance is Array)
            {
                bw.Write(_typeResolver(type.AssemblyQualifiedName));
                _SaveArray(bw, cache, instance as Array);
                return;
            }

            if (typeof(IList).IsAssignableFrom(type))
            {
                bw.Write(_typeResolver(type.AssemblyQualifiedName));
                _SaveList(instance as IList, bw, cache);                                
                return;
            }

            if (typeof(IDictionary).IsAssignableFrom(type))
            {
                bw.Write(_typeResolver(type.AssemblyQualifiedName));
                _SaveDictionary(instance as IDictionary, bw, cache);
                return;
            }           
                       
            bw.Write(_typeResolver(type.AssemblyQualifiedName));
            Save(type, instance, bw, cache,false);
        }

        /// <summary>
        ///     Serializes a property
        /// </summary>
        /// <param name="type">The parent type</param>
        /// <param name="propertyValue">The property value</param>
        /// <param name="bw">The writer</param>
        private void _SerializeProperty(Type type, object propertyValue, BinaryWriter bw)
        {
            bw.Write(_typeResolver(type.AssemblyQualifiedName));

            var isNull = propertyValue == null;
            _SerializeNull(bw, isNull);

            if (isNull)
            {
                return;
            }

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
            bw.Write(_typeResolver(type.AssemblyQualifiedName));

            // serialize to the stream if the foreign key is nulled
            _SerializeNull(bw, foreignTable == null);

            if (foreignTable == null) return;

            var foreignKey = _database.Save(foreignTable.GetType(), foreignTable.GetType(),foreignTable, cache);
            
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
        }

        /// <summary>
        ///     Helper load for serializers - this is not part of the internal recursion
        ///     Basically allows a node to be saved in a wrapper, and this is the entry
        ///     to unwrap it
        /// </summary>
        /// <typeparam name="T">Type of the object to laod</typeparam>
        /// <param name="br">The reader stream being accessed</param>
        /// <returns>The unwrapped object instance</returns>
        public T Load<T>(BinaryReader br)
        {
            var node = Load(typeof (SerializationNode), null, br, new CycleCache()) as SerializationNode;
            if (node != null)
            {
                return node.UnwrapForDeserialization<T>();
            }
            return default(T);
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

            if (_DeserializeNull(br))
            {
                return null;
            }
            
            // make a template
            var instance = Activator.CreateInstance(type);

            // build the reflection cache);
            if (!_propertyCache.ContainsKey(type))
            {
                //_CacheProperties(type);
                _CacheProperties(type);
            }

            if (instance is Array)
            {
                // push to the stack
                cache.Add(type, instance, key);
                var isNull = _DeserializeNull(br);

                if (!isNull)
                {
                    var count = br.ReadInt32();
                    for (var x = 0; x < count; x++)
                    {
                        ((Array) instance).SetValue(_Deserialize(br, cache), x);
                    }
                }
            }
            else if (instance is IList)
            {
                // push to the stack
                cache.Add(type, instance, key);
                var isNull = _DeserializeNull(br);
                if (!isNull)
                {
                    _LoadList(br, cache, instance as IList);
                }
            }

            else if (instance is IDictionary)
            {
                // push to the stack
                cache.Add(type, instance, key);
                var isNull = _DeserializeNull(br);
                if (!isNull)
                {
                    _LoadDictionary(br, cache, instance as IDictionary);
                }
            }
            else
            {
                type = Type.GetType(_typeIndexer(br.ReadInt32()));
                if (instance.GetType() != type)
                {
                    instance = Activator.CreateInstance(type);
                }

                // push to the stack
                cache.Add(type, instance, key);

                // build the reflection cache);
                if (!_propertyCache.ContainsKey(type))
                {
                    //_CacheProperties(type);
                    _CacheProperties(type);
                }
            }

            // now iterate
            foreach (var p in new List<SerializationCache>(_propertyCache[type]))
            {
                p.SetMethod(instance, _Deserialize(br, cache));
            }

            return instance;
        }

        private object _Deserialize(BinaryReader br, CycleCache cache)
        {
            var typeName = _typeIndexer(br.ReadInt32());

            if (_DeserializeNull(br))
            {
                return null;
            }

            Type typeResolved = null;

            if (!_typeRef.TryGetValue(typeName, out typeResolved))
            {
                typeResolved = Type.GetType(typeName);

                lock(((ICollection)_typeRef).SyncRoot)
                {
                    if (!_typeRef.ContainsKey(typeName))
                    {
                        _typeRef.Add(typeName, typeResolved);
                    }
                }
            }            

            if (_database.IsRegistered(typeResolved))
            {
                var keyType = _database.GetKeyType(typeResolved);
                var key = _serializer.Deserialize(keyType, br);

                var cached = cache.CheckKey(keyType, key);
                if (cached != null)
                {
                    return cached;
                }

                cached = _database.Load(typeResolved, key, cache);
                cache.Add(typeResolved, cached, key);
                return cached;
            }

            if (_serializer.CanSerialize(typeResolved))
            {
                return _serializer.Deserialize(typeResolved, br);
            }

            
            if (typeof(Array).IsAssignableFrom(typeResolved))
            {                
                var count = br.ReadInt32();
                var array = Array.CreateInstance(typeResolved.GetElementType(), count);
                for (var x = 0; x < count; x++)
                {
                    array.SetValue(_Deserialize(br, cache), x);
                }

                return array;
            }

            if (typeof (IList).IsAssignableFrom(typeResolved))
            {
                var list = Activator.CreateInstance(typeResolved) as IList;
                return _LoadList(br, cache, list);               
            }

            if (typeof (IDictionary).IsAssignableFrom(typeResolved))
            {
                var dictionary = Activator.CreateInstance(typeResolved) as IDictionary;
                return _LoadDictionary(br, cache, dictionary);
            }            

            var instance = Activator.CreateInstance(typeResolved);

            // build the reflection cache);
            if (!_propertyCache.ContainsKey(typeResolved))
            {
                //_CacheProperties(type);
                _CacheProperties(typeResolved);
            }

            // now iterate
            foreach (var p in _propertyCache[typeResolved])
            {
                p.SetMethod(instance, _Deserialize(br, cache));
            }

            return instance;
        }
        
        private IDictionary _LoadDictionary(BinaryReader br, CycleCache cache, IDictionary dictionary)
        {            
            var count = br.ReadInt32();
            for (var x = 0; x < count; x++)
            {
                dictionary.Add(_Deserialize(br, cache), _Deserialize(br, cache));
            }
            return dictionary;
        }

        private IList _LoadList(BinaryReader br, CycleCache cache, IList list)
        {
            var count = br.ReadInt32();
            for (var x = 0; x < count; x++)
            {
                list.Add(_Deserialize(br, cache));
            }
            return list;
        }

        private void _SerializeNull(BinaryWriter bw, bool isNull)
        {
            bw.Write(isNull ? NULL : NOTNULL);
            _logManager.Log(SterlingLogLevel.Verbose, string.Format("{0}", isNull ? NULL_DISPLAY : NOTNULL_DISPLAY), null);
        }    

        private bool _DeserializeNull(BinaryReader br)
        {
            var nullFlag = br.ReadUInt16();
            var isNull = nullFlag == NULL;
            _logManager.Log(SterlingLogLevel.Verbose, string.Format("{0}", isNull ? NULL_DISPLAY : NOTNULL_DISPLAY),null);
            return isNull;
        }
    }
}