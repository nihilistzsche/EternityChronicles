using System;
using System.Collections.Generic;
using IronDragon.Runtime;

namespace IronDragon.Builtins
{
    /// <summary>
    ///     TODO: Update summary.
    /// </summary>
    [DragonExport("Number")]
    public class DragonNumber {
        public DragonNumber()
        {
            _internal = default(int);
            _myType = typeof (int);
        }

        public DragonNumber(object val)
        {
            _internal = val;
            _myType = val.GetType();
        }

        private object _internal { get; }

        private Type _myType { get; }

        public override int GetHashCode()
        {
            return _internal.GetHashCode();
        }

        public override string ToString()
        {
            return _internal.ToString();
        }

        private static List<Type> _numberTypes = new()
        {
            typeof (bool),
            typeof (byte),
            typeof (sbyte),
            typeof (short),
            typeof (ushort),
            typeof (int),
            typeof (uint),
            typeof (long),
            typeof (ulong),
            typeof (float),
            typeof (double),
            typeof (decimal)
        };

        public override bool Equals(object obj)
        {
            if (obj is DragonNumber)
            {
                return RuntimeOperations.Convert(((DragonNumber) obj)._internal, _myType) ==
                       RuntimeOperations.Convert(_internal, _myType);
            }
            if (_numberTypes.Contains(obj.GetType()))
            {
                return RuntimeOperations.Convert(obj, _myType) == RuntimeOperations.Convert(_internal, _myType);
            }
            return false;
        }

        public static bool IsConvertable(object o)
        {
            return o != null && _numberTypes.Contains(o.GetType());
        }

        public static dynamic Convert(DragonNumber number)
        {
            return RuntimeOperations.Convert(number._internal, number._myType);
        }

        public static implicit operator bool(DragonNumber n)
        {
            return (bool)n._internal;
        }

        public static implicit operator DragonNumber(bool n)
        {
            return new(n);
        }

        public static implicit operator byte(DragonNumber n)
        {
            return (byte)n._internal;
        }

        public static implicit operator DragonNumber(byte n)
        {
            return new(n);
        }

        public static implicit operator sbyte(DragonNumber n)
        {
            return (sbyte)n._internal;
        }

        public static implicit operator DragonNumber(sbyte n)
        {
            return new(n);
        }

        public static implicit operator short(DragonNumber n)
        {
            return (short)n._internal;
        }

        public static implicit operator DragonNumber(short n)
        {
            return new(n);
        }

        public static implicit operator ushort(DragonNumber n)
        {
            return (ushort)n._internal;
        }

        public static implicit operator DragonNumber(ushort n)
        {
            return new(n);
        }

        public static implicit operator int(DragonNumber n)
        {
            return (int)n._internal;
        }

        public static implicit operator DragonNumber(int n)
        {
            return new(n);
        }

        public static implicit operator uint(DragonNumber n)
        {
            return (uint)n._internal;
        }

        public static implicit operator DragonNumber(uint n)
        {
            return new(n);
        }

        public static implicit operator long(DragonNumber n)
        {
            return (long)n._internal;
        }

        public static implicit operator DragonNumber(long n)
        {
            return new(n);
        }

        public static implicit operator ulong(DragonNumber n)
        {
            return (ulong)n._internal;
        }

        public static implicit operator DragonNumber(ulong n)
        {
            return new(n);
        }

        public static implicit operator float(DragonNumber n)
        {
            return (float)n._internal;
        }

        public static implicit operator DragonNumber(float n)
        {
            return new(n);
        }

        public static implicit operator double(DragonNumber n)
        {
            return (double)n._internal;
        }

        public static implicit operator DragonNumber(double n)
        {
            return new(n);
        }

        public static implicit operator decimal(DragonNumber n)
        {
            return (decimal)n._internal;
        }

        public static implicit operator DragonNumber(decimal n)
        {
            return new(n);
        }

    }
}
