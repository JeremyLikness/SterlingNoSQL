using System;
using Wintellect.Sterling.Exceptions;
using Wintellect.Sterling.IsolatedStorage;
using Wintellect.Sterling.Serialization;

namespace Wintellect.Sterling.Indexes
{
    /// <summary>
    ///     Collection of keys for a given entity
    /// </summary>
    internal class IndexCollection<T, TIndex1, TIndex2, TKey> : IndexCollection<T, Tuple<TIndex1, TIndex2>, TKey>
        where T : class, new()
    {
        /// <summary>
        ///     Initialize the key collection
        /// </summary>
        /// <param name="name">Index name</param>
        /// <param name="pathProvider">Path provider</param>
        /// <param name="databaseName">Name of the database</param>
        /// <param name="serializer">The serializer it can use to write/restore keys</param>
        /// <param name="indexer">How to resolve the index</param>
        /// <param name="resolver">The resolver for loading the object</param>
        public IndexCollection(string name, PathProvider pathProvider, string databaseName, ISterlingSerializer serializer,
                               Func<T, Tuple<TIndex1, TIndex2>> indexer, Func<TKey, T> resolver)
            : base(name, pathProvider, databaseName, serializer, indexer, resolver,
                   (tuple, bw, localSerializer) =>
                       {
                           localSerializer.Serialize(tuple.Item1, bw);
                           localSerializer.Serialize(tuple.Item2, bw);
                       },
                   (localSerializer, br) => Tuple.Create(localSerializer.Deserialize<TIndex1>(br),
                                                         localSerializer.Deserialize<TIndex2>(br)))

        {
            if (!serializer.CanSerialize<TIndex1>())
            {
                throw new SterlingSerializerException(serializer, typeof (TIndex1));
            }
            if (!serializer.CanSerialize<TIndex2>())
            {
                throw new SterlingSerializerException(serializer, typeof (TIndex2));
            }
        }
        

        /// <summary>
        ///     Add an index to the list
        /// </summary>
        /// <param name="index2">The second index</param>
        /// <param name="key">The related key</param>
        /// <param name="index1">The first index</param>
        public void AddIndex(object index1, object index2, object key)
        {
            var newIndex = new TableIndex<T, TIndex1, TIndex2, TKey>((TIndex1) index1, (TIndex2) index2, (TKey) key,
                                                                     Resolver);
            AddIndex(newIndex, key);
        }
    }
}