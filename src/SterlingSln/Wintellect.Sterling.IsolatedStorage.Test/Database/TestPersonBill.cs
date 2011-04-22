using System;
using System.Collections.Generic;
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wintellect.Sterling.Database;

namespace Wintellect.Sterling.IsolatedStorage.Test.Database
{
    /// <summary>
    ///     User-submitted test for a bug fix related to type indexing
    /// </summary>
    [Tag("PersonBill")]
    [TestClass]
    public class TestPersonBill
    {
        public class Bill
        {
            public Bill()
            {
                BilledPeople = new List<BilledPerson>();
            }

            public Guid Id { get; set; }
            public string Name { get; set; }
            public int Total { get; set; }
            public IList<BilledPerson> BilledPeople { get; set; }
        }

        public class Person
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
        }

        public class BilledPerson
        {
            public Person ThePerson { get; set; }
            public int Paid { get; set; }

        }

        public class BillPersonDatabase : BaseDatabaseInstance
        {
            public override string Name
            {
                get { return typeof(BillPersonDatabase).Name; }
            }

            protected override List<ITableDefinition> RegisterTables()
            {
                return new List<ITableDefinition>
                           {
                               CreateTableDefinition<Bill, Guid>(b => b.Id),
                               CreateTableDefinition<Person, Guid>(p => p.Id)
                           };
            }
        }

        [TestMethod]
        public void TestService()
        {
            var engine = new SterlingEngine();
            engine.Activate();

            try
            {
                var database = engine.SterlingDatabase.RegisterDatabase<BillPersonDatabase>(new IsolatedStorageDriver());
                database.Purge();
                database.Flush();

                var bill = new Bill { Name = "Test", Total = 45, Id = Guid.NewGuid() };
                var person = new Person { Name = "Martin Plante", Id = Guid.NewGuid() };

                database.Save(bill);
                database.Flush();

                database.Save(person);
                database.Flush();

                var bills = database.Query<Bill, Guid>();
                Assert.IsTrue(bills.Count == 1, "The database does not contain a single Bill");

                var people = database.Query<Person, Guid>();
                Assert.IsTrue(people.Count == 1, "The database does not contain a single Person");

                Assert.IsTrue(bills[0].LazyValue.Value.BilledPeople.Count == 0, "The Bill should not contain a single BilledPerson");

                var billedPerson = new BilledPerson { ThePerson = person, Paid = 20 };
                bill.BilledPeople.Add(billedPerson);

                database.Save(bill);
                database.Flush();

                bills = database.Query<Bill, Guid>();
                Assert.IsTrue(bills.Count == 1, "The database does not contain a single Bill");

                people = database.Query<Person, Guid>();
                Assert.IsTrue(people.Count == 1, "The database does not contain a single Person");                
            }
            finally
            {
                engine.Dispose();
            }

            engine = new SterlingEngine();
            engine.Activate();

            try
            {
                var database = engine.SterlingDatabase.RegisterDatabase<BillPersonDatabase>(new IsolatedStorageDriver());

                var bills = database.Query<Bill, Guid>();
                Assert.IsTrue(bills.Count == 1, "The database does not contain a single Bill");

                var people = database.Query<Person, Guid>();
                Assert.IsTrue(people.Count == 1, "The database does not contain a single Person");

                Assert.IsTrue(bills[0].LazyValue.Value.BilledPeople.Count == 1, "The Bill does not contain a single BilledPerson");
            }
            finally
            {
                engine.Dispose();
            }
        }
    }
}
