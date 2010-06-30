using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace System
{
    internal interface ITuple
    {
        // Methods
        int GetHashCode(IEqualityComparer comparer);
        string ToString(StringBuilder sb);

        // Properties
        int Size { get; }
    }

    public static class Tuple
    {        
        // Methods
        internal static int CombineHashCodes(int h1, int h2)
        {
            return (((h1 << 5) + h1) ^ h2);
        }

        internal static int CombineHashCodes(int h1, int h2, int h3)
        {
            return CombineHashCodes(CombineHashCodes(h1, h2), h3);
        }
        

        public static Tuple<T1> Create<T1>(T1 item1)
        {
            return new Tuple<T1>(item1);
        }

        public static Tuple<T1, T2> Create<T1, T2>(T1 item1, T2 item2)
        {
            return new Tuple<T1, T2>(item1, item2);
        }

        public static Tuple<T1, T2, T3> Create<T1, T2, T3>(T1 item1, T2 item2, T3 item3)
        {
            return new Tuple<T1, T2, T3>(item1, item2, item3);
        }
        
    }

    public class Tuple<T1> : IStructuralEquatable, IStructuralComparable, IComparable, ITuple
    {
        // Fields
        private readonly T1 _mItem1;

        // Methods
        public Tuple(T1 item1)
        {
            _mItem1 = item1;
        }

        public override bool Equals(object obj)
        {
            return ((IStructuralEquatable)this).Equals(obj, EqualityComparer<object>.Default);
        }

        public override int GetHashCode()
        {
            return ((IStructuralEquatable)this).GetHashCode(EqualityComparer<object>.Default);
        }

        int IStructuralComparable.CompareTo(object other, IComparer comparer)
        {
            if (other == null)
            {
                return 1;
            }
            var tuple = other as Tuple<T1>;
            if (tuple == null)
            {
                throw new ArgumentException("ArgumentException_TupleIncorrectType", "other");
            }
            return comparer.Compare(_mItem1, tuple._mItem1);
        }

        bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
        {
            if (other == null)
            {
                return false;
            }
            var tuple = other as Tuple<T1>;
            return tuple != null && comparer.Equals(_mItem1, tuple._mItem1);
        }

        int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
        {
            return comparer.GetHashCode(_mItem1);
        }

        int IComparable.CompareTo(object obj)
        {
            return ((IStructuralComparable)this).CompareTo(obj, Comparer<object>.Default);
        }

        int ITuple.GetHashCode(IEqualityComparer comparer)
        {
            return ((IStructuralEquatable)this).GetHashCode(comparer);
        }

        string ITuple.ToString(StringBuilder sb)
        {
            sb.Append(_mItem1);
            sb.Append(")");
            return sb.ToString();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("(");
            return ((ITuple)this).ToString(sb);
        }

        // Properties
        public T1 Item1
        {
            get
            {
                return _mItem1;
            }
        }

        int ITuple.Size
        {
            get
            {
                return 1;
            }
        }
    }

    public class Tuple<T1, T2> : IStructuralEquatable, IStructuralComparable, IComparable, ITuple
    {
        // Fields
        private readonly T1 _mItem1;
        private readonly T2 _mItem2;

        // Methods
        public Tuple(T1 item1, T2 item2)
        {
            _mItem1 = item1;
            _mItem2 = item2;
        }

        public override bool Equals(object obj)
        {
            return ((IStructuralEquatable)this).Equals(obj, EqualityComparer<object>.Default);
        }

        public override int GetHashCode()
        {
            return ((IStructuralEquatable)this).GetHashCode(EqualityComparer<object>.Default);
        }

        int IStructuralComparable.CompareTo(object other, IComparer comparer)
        {
            if (other == null)
            {
                return 1;
            }
            var tuple = other as Tuple<T1, T2>;
            if (tuple == null)
            {
                throw new ArgumentException("ArgumentException_TupleIncorrectType", "other");
            }
            int num = comparer.Compare(_mItem1, tuple._mItem1);
            return num != 0 ? num : comparer.Compare(_mItem2, tuple._mItem2);
        }

        bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
        {
            if (other == null)
            {
                return false;
            }
            var tuple = other as Tuple<T1, T2>;
            if (tuple == null)
            {
                return false;
            }
            return (comparer.Equals(_mItem1, tuple._mItem1) && comparer.Equals(_mItem2, tuple._mItem2));
        }

        int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
        {
            return Tuple.CombineHashCodes(comparer.GetHashCode(_mItem1), comparer.GetHashCode(_mItem2));
        }

        int IComparable.CompareTo(object obj)
        {
            return ((IStructuralComparable)this).CompareTo(obj, Comparer<object>.Default);
        }

        int ITuple.GetHashCode(IEqualityComparer comparer)
        {
            return ((IStructuralEquatable)this).GetHashCode(comparer);
        }

        string ITuple.ToString(StringBuilder sb)
        {
            sb.Append(_mItem1);
            sb.Append(", ");
            sb.Append(_mItem2);
            sb.Append(")");
            return sb.ToString();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("(");
            return ((ITuple)this).ToString(sb);
        }

        // Properties
        public T1 Item1
        {
            get
            {
                return _mItem1;
            }
        }

        public T2 Item2
        {
            get
            {
                return _mItem2;
            }
        }

        int ITuple.Size
        {
            get
            {
                return 2;
            }
        }
    }

    public class Tuple<T1, T2, T3> : IStructuralEquatable, IStructuralComparable, IComparable, ITuple
    {
        // Fields
        private readonly T1 _mItem1;
        private readonly T2 _mItem2;
        private readonly T3 _mItem3;

        // Methods
        public Tuple(T1 item1, T2 item2, T3 item3)
        {
            _mItem1 = item1;
            _mItem2 = item2;
            _mItem3 = item3;
        }

        public override bool Equals(object obj)
        {
            return ((IStructuralEquatable)this).Equals(obj, EqualityComparer<object>.Default);
        }

        public override int GetHashCode()
        {
            return ((IStructuralEquatable)this).GetHashCode(EqualityComparer<object>.Default);
        }

        int IStructuralComparable.CompareTo(object other, IComparer comparer)
        {
            if (other == null)
            {
                return 1;
            }
            var tuple = other as Tuple<T1, T2, T3>;
            if (tuple == null)
            {
                throw new ArgumentException("ArgumentException_TupleIncorrectType", "other");
            }
            int num = comparer.Compare(_mItem1, tuple._mItem1);
            if (num != 0)
            {
                return num;
            }
            num = comparer.Compare(_mItem2, tuple._mItem2);
            return num != 0 ? num : comparer.Compare(_mItem3, tuple._mItem3);
        }

        bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
        {
            if (other == null)
            {
                return false;
            }
            var tuple = other as Tuple<T1, T2, T3>;
            if (tuple == null)
            {
                return false;
            }
            return ((comparer.Equals(_mItem1, tuple._mItem1) && comparer.Equals(_mItem2, tuple._mItem2)) && comparer.Equals(_mItem3, tuple._mItem3));
        }

        int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
        {
            return Tuple.CombineHashCodes(comparer.GetHashCode(_mItem1), comparer.GetHashCode(_mItem2), comparer.GetHashCode(_mItem3));
        }

        int IComparable.CompareTo(object obj)
        {
            return ((IStructuralComparable)this).CompareTo(obj, Comparer<object>.Default);
        }

        int ITuple.GetHashCode(IEqualityComparer comparer)
        {
            return ((IStructuralEquatable)this).GetHashCode(comparer);
        }

        string ITuple.ToString(StringBuilder sb)
        {
            sb.Append(_mItem1);
            sb.Append(", ");
            sb.Append(_mItem2);
            sb.Append(", ");
            sb.Append(_mItem3);
            sb.Append(")");
            return sb.ToString();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("(");
            return ((ITuple)this).ToString(sb);
        }

        // Properties
        public T1 Item1
        {
            get
            {
                return _mItem1;
            }
        }

        public T2 Item2
        {
            get
            {
                return _mItem2;
            }
        }

        public T3 Item3
        {
            get
            {
                return _mItem3;
            }
        }

        int ITuple.Size
        {
            get
            {
                return 3;
            }
        }
    }



}
