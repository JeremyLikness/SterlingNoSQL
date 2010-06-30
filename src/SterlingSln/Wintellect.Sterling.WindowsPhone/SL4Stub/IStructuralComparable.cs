using System.Collections;

namespace System
{
    public interface IStructuralComparable
    {
        // Methods
        int CompareTo(object other, IComparer comparer);
    } 
}
