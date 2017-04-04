Sterling expects a unique key to identify each class. That key, however, can be a composite key. There are two strategies to create composite keys from classes. 

Consider the following class that has four key fields:

{code:c#}
namespace Wintellect.Sterling.Test.Helpers
{
    public class TestCompositeClass
    {
        public int Key1 { get; set; }
        public string Key2 { get; set; }
        public Guid Key3 { get; set; }
        public DateTime Key4 { get; set; }

        public string Data { get; set; }
    }
}
{code:c#}

To store as a composite key, you could cast the key values to a string that contains all of the values. This can be done in an order that makes sorts work out as well. You can then use indexes to access the individual properties. The definition of the key will look like this:

{code:c#}
CreateTableDefinition<TestCompositeClass, string>(k=>string.Format("{0}-{1}-{2}-{3}",
                               k.Key1, k.Key2, k.Key3, k.Key4)
{code:c#}

The follow unit tests shows defining and accessing the class:

{code:c#}
private static string GetKey(TestCompositeClass testClass)
{
    return string.Format("{0}-{1}-{2}-{3}", testClass.Key1, testClass.Key2, testClass.Key3,
                            testClass.Key4);
}

[TestMethod](TestMethod)
public void TestSave()
{
    var random = new Random();
    // test saving and reloading
    var list = new List<TestCompositeClass>();
    for (var x = 0; x < 100; x++)
    {
        var testClass = new TestCompositeClass
                            {
                                Key1 = random.Next(),
                                Key2 = random.Next().ToString(),
                                Key3 = Guid.NewGuid(),
                                Key4 = DateTime.Now.AddMinutes(-1*random.Next(100)),
                                Data = Guid.NewGuid().ToString()
                            };
        list.Add(testClass);
        _databaseInstance.Save(testClass);
    }

    for (var x = 0; x < 100; x++)
    {
        var actual = _databaseInstance.Load<TestCompositeClass>(GetKey(list[x](x)));
        Assert.IsNotNull(actual, "Load failed.");
        Assert.AreEqual(GetKey(list[x](x)), GetKey(actual), "Load failed: key mismatch.");
        Assert.AreEqual(list[x](x).Data, actual.Data, "Load failed: data mismatch.");
    }
}
{code:c#}

The "GetKey" could be defined as an extension method to clarify reading this in code.

A more streamlined approach would be to type the composite key. Because the key is a top level Sterling object, Sterling must know how to serialize it and therefore you must provide a custom serializer. In the above example, consider the following composite key. Notice it has a default constructor, another constructor that makes it easy to capture the key information, and overrides equality and hash code so it can be sorted appropriately: 

{code:c#}
namespace Wintellect.Sterling.Test.Helpers
{
    public class TestCompositeKeyClass
    {
        public TestCompositeKeyClass()
        {
            
        }

        public TestCompositeKeyClass(int key1, string key2, Guid key3, DateTime key4)
        {
            Key1 = key1;
            Key2 = key2;
            Key3 = key3;
            Key4 = key4;
        }

        public int Key1 { get; set; }
        public string Key2 { get; set; }
        public Guid Key3 { get; set; }
        public DateTime Key4 { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as TestCompositeKeyClass;
            if (other == null)
            {
                return false;
            }

            return other.Key1.Equals(Key1) && other.Key2.Equals(Key2)
                   && other.Key3.Equals(Key3) && other.Key4.Equals(Key4);
        }

        public override int GetHashCode()
        {
            return string.Format("{0}{1}{2}{3}", Key1, Key2, Key3, Key4).GetHashCode();
        }

    }
}
{code:c#}

The custom serializer simply writes out and de-serializes the key as needed:

{code:c#}
namespace Wintellect.Sterling.Test.Helpers
{    
    public class TestCompositeSerializer : BaseSerializer
    {
        /// <summary>
        ///     Return true if this serializer can handle the object
        /// </summary>
        /// <param name="targetType">The target</param>
        /// <returns>True if it can be serialized</returns>
        public override bool CanSerialize(Type targetType)
        {
            return targetType.Equals(typeof(TestCompositeKeyClass));
        }

        /// <summary>
        ///     Serialize the object
        /// </summary>
        /// <param name="target">The target</param>
        /// <param name="writer">The writer</param>
        public override void Serialize(object target, BinaryWriter writer)
        {
            var instance = (TestCompositeKeyClass)target;
            writer.Write(instance.Key1);
            writer.Write(instance.Key2);
            writer.Write(instance.Key3.ToByteArray());
            writer.Write(instance.Key4.ToFileTimeUtc());
        }

        /// <summary>
        ///     Deserialize the object
        /// </summary>
        /// <param name="type">The type of the object</param>
        /// <param name="reader">A reader to deserialize from</param>
        /// <returns>The deserialized object</returns>
        public override object Deserialize(Type type, BinaryReader reader)
        {
            return new TestCompositeKeyClass(
                reader.ReadInt32(),
                reader.ReadString(),
                new Guid(reader.ReadBytes(16)),
                DateTime.FromFileTimeUtc(reader.ReadInt64()).ToLocalTime());            
        }
    }
}
{code:c#}

Finally, the following test demonstrates the use of this key. Be sure to register the serializer.

{code:c#}
_engine.SterlingDatabase.RegisterSerializer<TestCompositeSerializer>();
...
[TestMethod](TestMethod)
public void TestSave()
{
    var random = new Random();
    // test saving and reloading
    var list = new List<TestCompositeClass>();
    for (var x = 0; x < 100; x++)
    {
        var testClass = new TestCompositeClass
        {
            Key1 = random.Next(),
            Key2 = random.Next().ToString(),
            Key3 = Guid.NewGuid(),
            Key4 = DateTime.Now.AddMinutes(-1 * random.Next(100)),
            Data = Guid.NewGuid().ToString()
        };
        list.Add(testClass);
        _databaseInstance.Save(testClass);
    }

    for (var x = 0; x < 100; x++)
    {
        var actual = _databaseInstance.Load<TestCompositeClass>(new TestCompositeKeyClass(list[x](x).Key1,
            list[x](x)(x)(x).Key2,list[x](x)(x)(x).Key3,list[x](x)(x)(x).Key4));
        Assert.IsNotNull(actual, "Load failed.");
        Assert.AreEqual(list[x](x).Data, actual.Data, "Load failed: data mismatch.");
    }
}
{code:c#}

These tests are include in the latest Sterling source.