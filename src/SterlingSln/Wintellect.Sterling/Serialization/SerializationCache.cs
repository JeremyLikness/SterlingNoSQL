using System;

namespace Wintellect.Sterling.Serialization
{
    /// <summary>
    ///     For easily unrolling cached properties
    /// </summary>
    internal enum PropertyType
    {
        Class,
        Property,
        Array,
        List,
        Dictionary,
        ComplexType
    }

    /// <summary>
    ///     Cache for serialization of properties
    /// </summary>
    internal class SerializationCache
    {
        public SerializationCache(Type propertyType, Type listType, PropertyType serializationType,
            Action<object, object> setter, Func<object, object> getter)
        {
            PropType = propertyType;
            ListType = listType;
            SerializationType = serializationType;
            SetMethod = setter;
            GetMethod = getter;
        }

        public SerializationCache(Type propertyType, Type keyType, Type valueType,
            Action<object, object> setter, Func<object, object> getter)
        {
            PropType = propertyType;
            DictionaryKeyType = keyType;
            DictionaryValueType = valueType;
            SetMethod = setter;
            GetMethod = getter;
            SerializationType = PropertyType.Dictionary;
        }

        /// <summary>
        ///     Property type
        /// </summary>
        public Type PropType { get; private set; }

        /// <summary>
        ///     For IList, the type of the individual elements
        /// </summary>
        public Type ListType { get; private set; }

        /// <summary>
        ///     Property type
        /// </summary>
        public Type DictionaryKeyType { get; private set; }

        /// <summary>
        ///     Property type
        /// </summary>
        public Type DictionaryValueType { get; private set; }

        /// <summary>
        ///     The method for serialization
        /// </summary>
        public PropertyType SerializationType { get; private set; }

        /// <summary>
        ///     The setter for the type
        /// </summary>
        public Action<object, object> SetMethod { get; private set; }

        /// <summary>
        ///     The getter for the type
        /// </summary>
        public Func<object, object> GetMethod { get; private set; }
    }
}
