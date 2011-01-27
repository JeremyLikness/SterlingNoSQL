using System;
using Wintellect.Sterling.Serialization;

namespace Wintellect.Sterling.Test.Helpers
{
    /// <summary>
    ///     A sub class Sterling isn't explicitly aware of
    /// </summary>
    public class TestSubclass
    {
        public string NestedText { get; set; }
    }

    /// <summary>
    ///     A sub class Sterling isn't explicitly aware of
    ///     That is suppressed
    /// </summary>
    [SterlingIgnore]
    public class TestSubclass2
    {
        public string NestedText { get; set; }
    }

    /// <summary>
    ///     A test model for testing serialization
    /// </summary>
    public class TestModel
    {
        public TestModel()
        {
            SubClass = new TestSubclass();
        }        

        private static int _idx;

        /// <summary>
        ///     The key
        /// </summary>
        public int Key { get; set; }

        /// <summary>
        ///     Data
        /// </summary>
        public string Data { get; set; }
        
        [SterlingIgnore]
        public string Data2 { get; set; }

        public TestSubclass SubClass { get; set; }

        public TestSubclass2 SubClass2 { get; set; }

        public TestModelAsListModel Parent { get; set; }

        /// <summary>
        ///     The date
        /// </summary>
        public DateTime Date { get; set; }

        public static TestModel MakeTestModel()
        {
            return new TestModel { Data = Guid.NewGuid().ToString(), Data2 = Guid.NewGuid().ToString(), Date = DateTime.Now, Key = _idx++, SubClass = new TestSubclass { NestedText = Guid.NewGuid().ToString() },
                                   SubClass2 = new TestSubclass2 { NestedText = Guid.NewGuid().ToString() }
            };
        }

        internal static TestModel MakeTestModel(TestModelAsListModel parentModel)
        {
            return new TestModel { Data = Guid.NewGuid().ToString(), Data2 = Guid.NewGuid().ToString(), Date = DateTime.Now, Key = _idx++, SubClass = new TestSubclass { NestedText = Guid.NewGuid().ToString() }, 
                SubClass2 = new TestSubclass2 { NestedText = Guid.NewGuid().ToString() }, Parent = parentModel };
        }
    }
}
