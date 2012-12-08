using System;

namespace Wintellect.Sterling.Test.Helpers
{
    public class TestChangingTypeSecondVersionClass
    {
        public string Key { get; set; }

        public string Name { get; set; }

        public string PropertyTwo { get; set; }

        public string PropertyOne { get; set; }

        public static TestChangingTypeSecondVersionClass MakeChangingTypeSecondVersionClass()
        {
            return new TestChangingTypeSecondVersionClass
                       {
                           Key = Guid.NewGuid().ToString(),
                           Name = "Name",
                           PropertyOne = "One",
                           PropertyTwo = "Two"
                       };
        }
    }
}