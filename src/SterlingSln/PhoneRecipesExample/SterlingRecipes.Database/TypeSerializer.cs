using System;
using System.IO;
using Wintellect.Sterling.Exceptions;
using Wintellect.Sterling.Serialization;

namespace SterlingRecipes.Database
{
    /// <summary>
    ///     Serialier that handles types and their derivatives
    /// </summary>
    public class TypeSerializer : BaseSerializer 
    {
        /// <summary>
        ///     Return true if this serializer can handle the object, i.e. if it
        ///     can be cast to type
        /// </summary>
        /// <param name="targetType">The target</param>
        /// <returns>True if it can be serialized</returns>
        public override bool CanSerialize(Type targetType)
        {
            return typeof (Type).IsAssignableFrom(targetType);
        }

        /// <summary>
        ///     Serialize the object
        /// </summary>
        /// <param name="target">The target</param>
        /// <param name="writer">The writer</param>
        public override void Serialize(object target, BinaryWriter writer)
        {
            var type = target as Type;
            
            // this is important
            if (type == null)
            {
                throw new SterlingSerializerException(this, target.GetType());
            }
            
            // we need this 
            if (type.AssemblyQualifiedName == null)
            {
                throw new SterlingSerializerException(this, target.GetType());
            }

            writer.Write(type.AssemblyQualifiedName);
        }

        /// <summary>
        ///     Deserialize the object
        /// </summary>
        /// <param name="type">The type of the object</param>
        /// <param name="reader">A reader to deserialize from</param>
        /// <returns>The deserialized object</returns>
        public override object Deserialize(Type type, BinaryReader reader)
        {
            return Type.GetType(reader.ReadString());
        }
    }
}