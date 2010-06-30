using System.Collections;

namespace System
{
    public interface IStructuralEquatable
    {
        // Methods
        bool Equals(object other, IEqualityComparer comparer);
        int GetHashCode(IEqualityComparer comparer);
    }
}
