using System;

namespace Wintellect.Sterling
{
    /// <summary>
    ///     Interface for a sterling trigger
    /// </summary>
    internal interface ISterlingTrigger
    {
        bool BeforeSave(Type type, object instance);
        void AfterSave(Type type, object instance);
        bool BeforeDelete(Type type, object key);
    }

    /// <summary>
    ///     Trigger for sterling
    /// </summary>
    /// <typeparam name="T">The type it supports</typeparam>
    /// <typeparam name="TKey">The key</typeparam>
    internal interface ISterlingTrigger<in T, in TKey> : ISterlingTrigger where T: class, new() 
    {
        bool BeforeSave(T instance);
        void AfterSave(T instance);
        bool BeforeDelete(TKey key);
    }
}