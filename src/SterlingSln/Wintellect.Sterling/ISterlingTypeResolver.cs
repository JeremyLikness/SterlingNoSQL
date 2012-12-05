using System;

namespace Wintellect.Sterling
{
    /// <summary>
    /// A class responsible for converting objects serialized in a previous schema to the current schema.
    /// </summary>
    public interface ISterlingTypeResolver
    {
        Type ResolveTableType(string fullTypeName);
    }
}