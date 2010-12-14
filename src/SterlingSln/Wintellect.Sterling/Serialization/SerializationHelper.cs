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

        /// <summary>
        ///     "Remember" how lists map to avoid reflecting each time
        /// </summary>
        private static readonly Dictionary<Type, Type> _listTypes = new Dictionary<Type, Type>();

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

            Type listType = null;

            // if we can cast to a list and find the interface with the type, we'll use it
            if (typeof (IList).IsAssignableFrom(type))
            {
                listType = (from @interface in type.GetInterfaces()
                            where @interface.IsGenericType
                            where @interface.GetGenericTypeDefinition() == typeof (IList<>)
                            select @interface.GetGenericArguments()[0]).FirstOrDefault();
            }

            lock (((ICollection) _listTypes).SyncRoot)
            {
                if (!_listTypes.ContainsKey(type))
                {
                    _listTypes.Add(type, listType);
                }
            }

            return listType;
        }

        /// <summary>
        ///     Cache the properties for a type so we don't reflect every time
        /// </summary>
        /// <param name="type">The type to manage</param>
        private void _CacheProperties(Type type)
        {
            lock (((ICollection) _propertyCache).SyncRoot)
            {
                if (_propertyCache.ContainsKey(type)) return;

                _propertyCache.Add(type,
                                   new List
                                       <
                                       SerializationCache
                                       >());

                var properties = from p in type.GetProperties()
                                 where p.GetGetMethod() != null && p.GetSetMethod() != null
                                 select p;

                foreach (var p in properties)
                {
                    var propType = p.PropertyType;

                    if (propType.IsGenericType && propType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
                    {
                        propType = Nullable.GetUnderlyingType(propType);
                    }
                    else if (propType.IsEnum)
                    {
                        propType = Enum.GetUnderlyingType(propType);
                    }

                    // this is registered "keyed" type, so we serialize the foreign key
                    if (_database.IsRegistered(propType))
                    {
                        var p1 = p;
                        _propertyCache[type].Add(
                            new SerializationCache(
                                p.PropertyType,
                                null,
                                PropertyType.Class,
                                (parent, property) => p1.GetSetMethod().Invoke(parent, new[] {property}),
                                parent => p1.GetGetMethod().Invoke(parent, new object[] {})));
                    }
                        // this is a property
                    else if (_serializer.CanSerialize(propType))
                    {
                        var p1 = p;

                        Func<object, object> getter = parent => p1.GetGetMethod().Invoke(parent, new object[] {});

                        // cast to underlying type
                        if (p.PropertyType.IsEnum)
                        {                            
                            getter = parent => Convert.ChangeType(p1.GetGetMethod().Invoke(parent, new object[] {}),
                                                                  propType, null);
                        }
                        
                        _propertyCache[type].Add(
                            new SerializationCache(
                                propType,
                                null,
                                PropertyType.Property,
                                (parent, property) => p1.GetSetMethod().Invoke(parent, new[] {property}),
                                getter));
                    }
                    else
                    {
                        // check if we can handle this as a list
                        var listType = _IsGenericList(propType);
                        if (listType != null &&
                            (_database.IsRegistered(listType) || _serializer.CanSerialize(listType)))
                        {
                            var p1 = p;
                            _propertyCache[type].Add(
                                new SerializationCache(
                                    propType,
                                    listType, PropertyType.List,
                                    (parent, property) => p1.GetSetMethod().Invoke(parent, new[] {property}),
                                    parent => p1.GetGetMethod().Invoke(parent, new object[] {})));
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="database">Database this is a helper for</param>
        /// <param name="serializer">The serializer</param>
        /// <param name="logManager">The logger</param>
        public SerializationHelper(ISterlingDatabaseInstance database, ISterlingSerializer serializer,
                                   LogManager logManager)
        {
            _database = database;
            _serializer = serializer;
            _logManager = logManager;
        }

        /// <summary>
        ///     Recursive save operation
        /// </summary>
        /// <param name="type">The type to save (passed to support NULL)</param>
        /// <param name="instance">The instance to type</param>
        /// <param name="bw">The writer to save it to</param>
        public void Save(Type type, object instance, BinaryWriter bw)
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
                _CacheProperties(type);
            }

            // now iterate the serializable properties
            foreach (var p in _propertyCache[type])
            {
                if (p.SerializationType.Equals(PropertyType.Class))
                {
                    // foreign table - write if it is null or not, and if not null, write the key
                    // then serialize it separately
                    _SerializeClass(type, p.GetMethod(instance), bw);
                }
                else if (p.SerializationType.Equals(PropertyType.Property))
                {
                    _SerializeProperty(type, p.GetMethod(instance), bw, p.PropType);
                }
                else
                {
                    _SerializeList(p.ListType, (IList) p.GetMethod(instance), bw);
                }
            }
        }

        /// <summary>
        ///     Handles serialization of a list
        /// </summary>
        /// <param name="listType">The type of elements in the list</param>
        /// <param name="instance">The list to serialize</param>
        /// <param name="bw">The stream to serialize to</param>
        private void _SerializeList(Type listType, IList instance, BinaryWriter bw)
        {
            var count = instance == null ? 0 : instance.Count;

            // always pass the count (if it's null we'll just re-serialize an empty list)
            bw.Write(count);

            if (instance == null || count <= 0) return;

            Action<object> serialize;

            // serialize to database (as class) or as property?
            if (_database.IsRegistered(listType))
            {
                serialize = obj => _SerializeClass(listType, obj, bw);
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
        private void _SerializeClass(Type type, object foreignTable, BinaryWriter bw)
        {
            // serialize to the stream if the foreign key is nulled
            bw.Write(foreignTable == null ? NULL : NOTNULL);

            if (foreignTable == null) return;

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
            _database.Save(foreignTable.GetType(), foreignTable);
        }

        /// <summary>
        ///     Recursive load operation
        /// </summary>
        /// <param name="type">The type to save (passed to support NULL)</param>
        /// <param name="br">The reader</param>
        public object Load(Type type, BinaryReader br)
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

            // build the reflection cache
            if (!_propertyCache.ContainsKey(type))
            {
                _CacheProperties(type);
            }

            // now iterate
            foreach (var p in _propertyCache[type])
            {
                // recursive save? 
                if (p.SerializationType.Equals(PropertyType.Class))
                {
                    p.SetMethod(instance, _DeserializeClass(type, p.PropType, br));
                }
                else if (p.SerializationType.Equals(PropertyType.Property))
                {
                    p.SetMethod(instance, _DeserializeProperty(type, br, p.PropType));
                }
                else
                {
                    // build the base list (this will be List<T> for example because we know the exact type)
                    var list = (IList) Activator.CreateInstance(p.PropType);
                    p.SetMethod(instance, list);
                    _DeserializeList(type, p.ListType, list, br, p.PropType);
                }
            }

            return instance;
        }

        /// <summary>
        ///     De-serialize a list
        /// </summary>
        /// <param name="parentType">The parent type ("owns the list")</param>
        /// <param name="listType">The type of elements in the list</param>
        /// <param name="instance">The list to build</param>
        /// <param name="br">The reader</param>
        /// <param name="p">The full type of the list (the list itself, not the elements)</param>
        private void _DeserializeList(Type parentType, Type listType, IList instance, BinaryReader br, Type p)
        {
            var idx = br.ReadInt32();

            for (var i = 0; i < idx; i++)
            {
                var obj = _database.IsRegistered(listType)
                                 ? _DeserializeClass(parentType, listType, br)
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
        /// <returns>The de-serialized class</returns>
        private object _DeserializeClass(Type type, Type targetType, BinaryReader br)
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

            var keyType = _database.GetKeyType(targetType);
            return _database.Load(targetType, _serializer.Deserialize(keyType, br));
        }
    }
}