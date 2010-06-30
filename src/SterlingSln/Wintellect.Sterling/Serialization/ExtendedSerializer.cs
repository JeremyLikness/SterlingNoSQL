using System;
using System.Collections.Generic;
using System.IO;
using Wintellect.Sterling.Exceptions;

namespace Wintellect.Sterling.Serialization
{
    /// <summary>
    ///     Serializes some extended objects
    /// </summary>
    internal class ExtendedSerializer : BaseSerializer
    {
        /// <summary>
        ///     Dictionary of serializers
        /// </summary>
        private readonly Dictionary<Type, Tuple<Action<BinaryWriter,object>,Func<BinaryReader,object>>> _serializers
            = new Dictionary<Type, Tuple<Action<BinaryWriter, object>, Func<BinaryReader, object>>>();

        /// <summary>
        ///     Default constructor
        /// </summary>
        public ExtendedSerializer()
        {
            // wire up the serialization pairs 
            _serializers.Add(typeof (DateTime), new Tuple<Action<BinaryWriter, object>, Func<BinaryReader, object>>(
                                                    (bw, obj) => bw.Write(((DateTime) obj).Ticks),
                                                    br => new DateTime(br.ReadInt64())));

#if WINPHONE7
#else
            _serializers.Add(typeof(Guid), new Tuple<Action<BinaryWriter, object>, Func<BinaryReader, object>>(
                (bw, obj) => bw.Write(obj.ToString()),
                br => Guid.Parse(br.ReadString())));
#endif

            _serializers.Add(typeof(Uri), new Tuple<Action<BinaryWriter, object>, Func<BinaryReader, object>>(
                (bw, obj) => bw.Write(((Uri)obj).AbsoluteUri),
                br => new Uri(br.ReadString())));          
        }

        /// <summary>
        ///     Return true if this serializer can handle the object
        /// </summary>
        /// <param name="type">The target type</param>
        /// <returns>True if it can be serialized</returns>
        public override bool CanSerialize(Type type)
        {
            return _serializers.ContainsKey(type);            
        }

        /// <summary>
        ///     Serialize the object
        /// </summary>
        /// <param name="target">The target</param>
        /// <param name="writer">The writer</param>
        public override void Serialize(object target, BinaryWriter writer)
        {
            if (!CanSerialize(target.GetType()))
            {
                throw new SterlingSerializerException(this, target.GetType());
            }
            _serializers[target.GetType()].Item1(writer, target);
        }

        /// <summary>
        ///     Deserialize the object
        /// </summary>
        /// <param name="type">The type of the object</param>
        /// <param name="reader">A reader to deserialize from</param>
        /// <returns>The deserialized object</returns>
        public override object Deserialize(Type type, BinaryReader reader)
        {
            if (!CanSerialize(type))
            {
                throw new SterlingSerializerException(this, type);
            }
            return _serializers[type].Item2(reader);
        }   
    }
}
