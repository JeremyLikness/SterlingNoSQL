using System;

namespace Wintellect.Sterling.Test.Helpers
{
    /// <summary>
    ///     A test model for testing serialization
    /// </summary>
    public class TestModel
    {
        private static int _idx = 0;

        /// <summary>
        ///     The key
        /// </summary>
        public int Key { get; set; }

        /// <summary>
        ///     Data
        /// </summary>
        public string Data { get; set; }

        /// <summary>
        ///     The date
        /// </summary>
        public DateTime Date { get; set; }

        public static TestModel MakeTestModel()
        {
            return new TestModel {Data = Guid.NewGuid().ToString(), Date = DateTime.Now, Key = _idx++};
        }
    }
}
