using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wintellect.Sterling.Database;
using Wintellect.Sterling.Serialization;

namespace Wintellect.Sterling.Test.Serializer
{
    public class NotSupportedList : IEnumerable<string>
    {
        private readonly List<string> _list = new List<string>();

        public void Add(IEnumerable<string> newItems)
        {
            _list.AddRange(newItems);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<string> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class NotSupportedClass
    {
        public NotSupportedClass()
        {
            InnerList = new NotSupportedList();
        }

        public int Id { get; set; }

        public NotSupportedList InnerList { get; set; }
    }

    public class CustomSerializerDatabase : BaseDatabaseInstance
    {        
        /// <summary>
        ///     Method called from the constructor to register tables
        /// </summary>
        /// <returns>The list of tables for the database</returns>
        protected override List<ITableDefinition> RegisterTables()
        {
            return new List<ITableDefinition>
                           {
                               CreateTableDefinition<NotSupportedClass, int>(t=>t.Id)
                           };
        }
    }

    public class SupportSerializer : BaseSerializer
    {
        /// <summary>
        ///     Return true if this serializer can handle the object
        /// </summary>
        /// <param name="targetType">The target</param>
        /// <returns>True if it can be serialized</returns>
        public override bool CanSerialize(Type targetType)
        {
            return targetType.Equals(typeof (NotSupportedList));
        }

        /// <summary>
        ///     Serialize the object
        /// </summary>
        /// <param name="target">The target</param>
        /// <param name="writer">The writer</param>
        public override void Serialize(object target, BinaryWriter writer)
        {
            // turn it into a list and save it 
            var list = new List<string>((NotSupportedList) target);
            TestCustomSerializer.DatabaseInstance.Helper.Save(typeof(SerializationNode), SerializationNode.WrapForSerialization(list), writer, new CycleCache());
        }

        /// <summary>
        ///     Deserialize the object
        /// </summary>
        /// <param name="type">The type of the object</param>
        /// <param name="reader">A reader to deserialize from</param>
        /// <returns>The deserialized object</returns>
        public override object Deserialize(Type type, BinaryReader reader)
        {
            // grab it as a list 
            var list =
                ((SerializationNode)
                 TestCustomSerializer.DatabaseInstance.Helper.Load(typeof (SerializationNode), null, reader,
                                                                   new CycleCache())).UnwrapForDeserialization
                    <List<string>>();
            return new NotSupportedList {list};
        }
    }

#if SILVERLIGHT
    [Tag("Custom")]
    [Tag("Serializer")]
#endif
    [TestClass]
    public class TestCustomSerializer
    {
        private SterlingEngine _engine;
        public static ISterlingDatabaseInstance DatabaseInstance;        

        [TestInitialize]
        public void TestInit()
        {
            _engine = new SterlingEngine();
            _engine.SterlingDatabase.RegisterSerializer<SupportSerializer>();            
            _engine.Activate();
            DatabaseInstance = _engine.SterlingDatabase.RegisterDatabase<CustomSerializerDatabase>();
            DatabaseInstance.Purge();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            DatabaseInstance.Purge();
            _engine.Dispose();
            DatabaseInstance = null;
        }

        [TestMethod]
        public void TestCustomSaveAndLoad()
        {
            var expectedList = new[] {"one", "two", "three"};
            var expected = new NotSupportedClass {Id = 1};
            expected.InnerList.Add(expectedList);

            var key = DatabaseInstance.Save(expected);

            var actual = DatabaseInstance.Load<NotSupportedClass>(key);
            Assert.IsNotNull(actual, "Load failed: instance is null.");
            Assert.AreEqual(expected.Id, actual.Id, "Load failed: key mismatch.");

            // cast to list
            var actualList = new List<string>(actual.InnerList);

            CollectionAssert.AreEquivalent(expectedList, actualList, "Load failed: lists do not match.");

        }        
    }
}