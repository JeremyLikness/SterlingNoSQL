using System;

namespace Wintellect.Sterling.Test.Helpers
{
    public class TestClassWithArray
    {
        private static int _id;

        public int ID { get; set; }
        public int[] ValueTypeArray { get; set; }
        public TestBaseClassModel[] BaseClassArray { get; set; }
        public TestModel[] ClassArray { get; set; }

        public static TestClassWithArray MakeTestClassWithArray(bool includeNullInArrays)
        {
            return new TestClassWithArray()
            {
                ID = _id++,
                ValueTypeArray = new[] { 1, 2, 3 },
                BaseClassArray = includeNullInArrays ? 
                    new TestBaseClassModel[] { new TestDerivedClassAModel(), new TestDerivedClassBModel(), null } :
                    new TestBaseClassModel[] { new TestDerivedClassAModel(), new TestDerivedClassBModel() },
                ClassArray = includeNullInArrays ? new[] { TestModel.MakeTestModel(), null } : new[] { TestModel.MakeTestModel() }
            };
        }
    }
}
