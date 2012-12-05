using System;

namespace Wintellect.Sterling.Test.Helpers
{
    public class TestChangingTypeFirstVersionClass
    {
        public string Key { get; set; }

        public string Name { get; set; }

        public static TestChangingTypeFirstVersionClass MakeChangingTypeFirstVersionClass()
        {
            return new TestChangingTypeFirstVersionClass
                       {
                           Key = Guid.NewGuid().ToString(),
                           Name = "Name"
                       };
        }
    }
}