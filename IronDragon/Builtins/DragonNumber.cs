// DragonNumber.cs in EternityChronicles/IronDragon
// 
// Copyright (C) 2022 Michael Tindal (nihilistzsche AT gmail DOT com)
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using IronDragon.Runtime;

namespace IronDragon.Builtins
{
    /// <summary>
    ///     TODO: Update summary.
    /// </summary>
    [DragonExport("Number")]
    public class DragonNumber
    {
        private static readonly List<Type> NumberTypes = new()
                                                          {
                                                              typeof(bool),
                                                              typeof(byte),
                                                              typeof(sbyte),
                                                              typeof(short),
                                                              typeof(ushort),
                                                              typeof(int),
                                                              typeof(uint),
                                                              typeof(long),
                                                              typeof(ulong),
                                                              typeof(float),
                                                              typeof(double),
                                                              typeof(decimal)
                                                          };

        public DragonNumber()
        {
            Internal = default(int);
            MyType = typeof(int);
        }

        public DragonNumber(object val)
        {
            Internal = val;
            MyType = val.GetType();
        }

        private object Internal { get; }

        private Type MyType { get; }

        public override int GetHashCode()
        {
            return Internal.GetHashCode();
        }

        public override string ToString()
        {
            return Internal.ToString();
        }

        public override bool Equals(object obj)
        {
            if (obj is DragonNumber)
                return RuntimeOperations.Convert(((DragonNumber)obj).Internal, MyType) ==
                       RuntimeOperations.Convert(Internal, MyType);
            if (NumberTypes.Contains(obj.GetType()))
                return RuntimeOperations.Convert(obj, MyType) == RuntimeOperations.Convert(Internal, MyType);
            return false;
        }

        public static bool IsConvertable(object o)
        {
            return o != null && NumberTypes.Contains(o.GetType());
        }

        public static dynamic Convert(DragonNumber number)
        {
            return RuntimeOperations.Convert(number.Internal, number.MyType);
        }

        public static implicit operator bool(DragonNumber n)
        {
            return (bool)n.Internal;
        }

        public static implicit operator DragonNumber(bool n)
        {
            return new DragonNumber(n);
        }

        public static implicit operator byte(DragonNumber n)
        {
            return (byte)n.Internal;
        }

        public static implicit operator DragonNumber(byte n)
        {
            return new DragonNumber(n);
        }

        public static implicit operator sbyte(DragonNumber n)
        {
            return (sbyte)n.Internal;
        }

        public static implicit operator DragonNumber(sbyte n)
        {
            return new DragonNumber(n);
        }

        public static implicit operator short(DragonNumber n)
        {
            return (short)n.Internal;
        }

        public static implicit operator DragonNumber(short n)
        {
            return new DragonNumber(n);
        }

        public static implicit operator ushort(DragonNumber n)
        {
            return (ushort)n.Internal;
        }

        public static implicit operator DragonNumber(ushort n)
        {
            return new DragonNumber(n);
        }

        public static implicit operator int(DragonNumber n)
        {
            return (int)n.Internal;
        }

        public static implicit operator DragonNumber(int n)
        {
            return new DragonNumber(n);
        }

        public static implicit operator uint(DragonNumber n)
        {
            return (uint)n.Internal;
        }

        public static implicit operator DragonNumber(uint n)
        {
            return new DragonNumber(n);
        }

        public static implicit operator long(DragonNumber n)
        {
            return (long)n.Internal;
        }

        public static implicit operator DragonNumber(long n)
        {
            return new DragonNumber(n);
        }

        public static implicit operator ulong(DragonNumber n)
        {
            return (ulong)n.Internal;
        }

        public static implicit operator DragonNumber(ulong n)
        {
            return new DragonNumber(n);
        }

        public static implicit operator float(DragonNumber n)
        {
            return (float)n.Internal;
        }

        public static implicit operator DragonNumber(float n)
        {
            return new DragonNumber(n);
        }

        public static implicit operator double(DragonNumber n)
        {
            return (double)n.Internal;
        }

        public static implicit operator DragonNumber(double n)
        {
            return new DragonNumber(n);
        }

        public static implicit operator decimal(DragonNumber n)
        {
            return (decimal)n.Internal;
        }

        public static implicit operator DragonNumber(decimal n)
        {
            return new DragonNumber(n);
        }
    }
}