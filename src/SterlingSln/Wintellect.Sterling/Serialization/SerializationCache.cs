using System;

namespace Wintellect.Sterling.Serialization
{    
    /// <summary>
    ///     Cache for serialization of properties
    /// </summary>
    internal class SerializationCache
    {
        public SerializationCache(Type propertyType, 
            Action<object, object> setter, Func<object, object> getter)
        {
            PropType = propertyType;          
            SetMethod = setter;
            GetMethod = getter;
        }

        /// <summary>
        ///     Property type
        /// </summary>
        public Type PropType { get; private set; }

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
