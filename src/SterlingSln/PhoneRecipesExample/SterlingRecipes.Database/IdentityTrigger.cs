using System.Linq;
using SterlingRecipes.Models;
using Wintellect.Sterling;
using Wintellect.Sterling.Database;

namespace SterlingRecipes.Database
{
    /// <summary>
    ///     Generic definition for an identity trigger based on integers
    /// </summary>
    /// <typeparam name="T">The type</typeparam>
    public class IdentityTrigger<T> : BaseSterlingTrigger<T,int> where T: class, IBaseModel, new()
    {
        private static int _idx = 1;

        public IdentityTrigger(ISterlingDatabaseInstance database)
        {
            // if a record exists, set it to the highest value plus 1
            if (database.Query<T,int>().Any())
            {
                _idx = database.Query<T, int>().Max(key => key.Key) + 1;
            }
        }

        /// <summary>
        ///  See if we need to provide an id
        /// </summary>
        /// <param name="instance">Instance to check</param>
        /// <returns>True if we're good to save</returns>
        public override bool BeforeSave(T instance)
        {
            if (instance.Id < 1)
            {
                instance.Id = _idx++;
            }

            return true;
        }

        public override void AfterSave(T instance)
        {
            return;
        }

        public override bool BeforeDelete(int key)
        {
            return true;
        }
    }
}