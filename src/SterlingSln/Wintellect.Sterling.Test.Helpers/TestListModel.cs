using System.Collections.Generic;

namespace Wintellect.Sterling.Test.Helpers
{
    public class TestListModel
    {
        private static int _nextId = 0;

        public int ID { get; set; }
        
        public List<TestModel> Children { get; set; }

        public static TestListModel MakeTestListModel(bool includeNullInList)
        {
            return new TestListModel
                       {
                           ID = _nextId++,
                           Children =
                               includeNullInList ?
                               new List<TestModel> {TestModel.MakeTestModel(), TestModel.MakeTestModel(), TestModel.MakeTestModel(), null} : 
                               new List<TestModel> {TestModel.MakeTestModel(), TestModel.MakeTestModel(), TestModel.MakeTestModel()}
                       };
        }
    }
}
