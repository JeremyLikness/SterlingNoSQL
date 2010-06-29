using System;
using System.Collections.Generic;
using Wintellect.Sterling.Indexes;
using Wintellect.Sterling.Keys;

namespace Wintellect.Sterling.Database
{
    /// <summary>
    ///     Table definnition
    /// </summary>
    public interface ITableDefinition
    {
        /// <summary>
        ///     Key list
        /// </summary>
        IKeyCollection Keys { get; }

        /// <summary>
        ///     Indexes
        /// </summary>
        Dictionary<string, IIndexCollection> Indexes { get; }

        /// <summary>
        ///     Table type
        /// </summary>
        Type TableType { get; }

        /// <summary>
        ///     Key type
        /// </summary>
        Type KeyType { get; }

        /// <summary>
        ///     Refresh key list
        /// </summary>
        void Refresh();

        /// <summary>
        ///     Fetch the key for the instance
        /// </summary>
        /// <param name="instance">The instance</param>
        /// <returns>The key</returns>
        object FetchKeyFromInstance(object instance);
    }
}