using System;
using System.Collections.Generic;

namespace SterlingRecipes.Models
{
    /// <summary>
    ///     Help with tombstoning view models
    /// </summary>
    public class TombstoneModel
    {
        /// <summary>
        ///     The type of the view model - can be interface, etc.
        /// </summary>
        public Type SyncType { get; set; }

        /// <summary>
        ///     Set up the state dictionary on construction
        /// </summary>
        public TombstoneModel()
        {
            State = new Dictionary<string, object>();
        }

        /// <summary>
        ///     A dictionary of states - set the key and the object 
        ///     and it's stored
        /// </summary>
        public Dictionary<string, object> State { get; set; }

        /// <summary>
        ///     Helper method to try to get a value out 
        ///     Supply a default value if the value doesn't exist
        /// </summary>
        /// <typeparam name="T">The type of the stored value</typeparam>
        /// <param name="key">The key</param>
        /// <param name="defaultValue">A default if it doesn't exist</param>
        /// <returns>The value or the default</returns>
        public T TryGet<T>(string key, T defaultValue)
        {
            if (State.ContainsKey(key))
            {
                return (T)State[key];
            }
            return defaultValue;
        }
    }
}