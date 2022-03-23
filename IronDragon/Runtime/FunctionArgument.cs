// FunctionArgument.cs in EternityChronicles/IronDragon
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

using System.Linq.Expressions;

namespace IronDragon.Runtime
{
    /// <summary>
    ///     The class to represent function arguments to the Dragon runtime.  The same class is used by both calls and
    ///     definitions.
    /// </summary>
    public class FunctionArgument
    {
        public FunctionArgument(string name) : this(name, null)
        {
        }

        public FunctionArgument(string name, int index) : this(name, null)
        {
            Index = index;
        }

        public FunctionArgument(string name, Expression value)
        {
            Name = name;
            Value = value;
        }

        public int Index { get; set; }

        public string Name { get; set; }

        public bool HasDefault { get; set; }

        public Expression DefaultValue { get; set; }

        public bool IsVarArg { get; set; }

        public bool IsFunction { get; set; }

        public bool IsLiteral { get; set; }

        public Expression Value { get; set; }

        public override string ToString()
        {
            return
                string.Format(
                              "[FunctionArgument: Index={0}, Name={1}, HasDefault={2}, DefaultValue={3}, IsVarArg={4}, IsFunction={5}, IsLiteral={6}, Value={7}]",
                              Index, Name, HasDefault, DefaultValue, IsVarArg, IsFunction, IsLiteral, Value);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode() + base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            if (!(obj is FunctionArgument)) return false;

            if (this == obj) return true;

            var other = (FunctionArgument)obj;

            if (!Name.Equals(other.Name)) return false;

            if (Value != null)
                if (!Value.Equals(other.Value))
                    return false;

            if (HasDefault)
                if (!DefaultValue.Equals(other.DefaultValue))
                    return false;

            return true;
        }
    }
}