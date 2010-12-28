using System;

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
    ///     A test model for testing serialization
    /// </summary>
    public class TestModel
    {
        public TestModel()
        {
            SubClass = new TestSubclass();
        }

        private static int _idx = 0;

        /// <summary>
        ///     The key
        /// </summary>
        public int Key { get; set; }

        /// <summary>
        ///     Data
        /// </summary>
        public string Data { get; set; }

        public TestSubclass SubClass { get; set; }

        /// <summary>
        ///     The date
        /// </summary>
        public DateTime Date { get; set; }

        public static TestModel MakeTestModel()
        {
            return new TestModel {Data = Guid.NewGuid().ToString(), Date = DateTime.Now, Key = _idx++, SubClass = new TestSubclass { NestedText = Guid.NewGuid().ToString()}};
        }
    }
}
