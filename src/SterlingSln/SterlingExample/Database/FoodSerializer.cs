using System;
using System.IO;
using SterlingExample.Model;
using Wintellect.Sterling.Serialization;

namespace SterlingExample.Database
{
    /// <summary>
    ///     Help serializing the nutrient values
    /// </summary>
    public class FoodSerializer : BaseSerializer  
    {
        /// <summary>
        ///     Return true if this serializer can handle the object
        /// </summary>
        /// <param name="targetType">The target</param>
        /// <returns>True if it can be serialized</returns>
        public override bool CanSerialize(Type targetType)
        {
            return targetType.Equals(typeof (NutrientDataElement));                
        }

        /// <summary>
        ///     Serialize the object
        /// </summary>
        /// <param name="target">The target</param>
        /// <param name="writer">The writer</param>
        public override void Serialize(object target, BinaryWriter writer)
        {
            var data = (NutrientDataElement)target;
            writer.Write(data.NutrientDefinitionId);
            writer.Write(data.AmountPerHundredGrams);
        }

        /// <summary>
        ///     Deserialize the object
        /// </summary>
        /// <param name="type">The type of the object</param>
        /// <param name="reader">A reader to deserialize from</param>
        /// <returns>The deserialized object</returns>
        public override object Deserialize(Type type, BinaryReader reader)
        {
            return new NutrientDataElement
                       {
                           NutrientDefinitionId = reader.ReadInt32(),
                           AmountPerHundredGrams = reader.ReadDouble()
                       };
        }
    }
}
