// SwitchExpression.cs in EternityChronicles/IronDragon
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
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using IronDragon.Runtime;

namespace IronDragon.Expressions
{
    /// <summary>
    ///     Represents a switch statement for Dragon.
    /// </summary>
    public class SwitchExpression : DragonExpression
    {
        internal SwitchExpression(Expression test, Expression @default, List<SwitchCase> cases)
        {
            Test = test;
            DefaultBody = @default;
            Cases = cases;
        }

        public Expression Test { get; }

        public Expression DefaultBody { get; }

        public List<SwitchCase> Cases { get; }


        public override Type Type => Cases.First().Body.Type;

        public override Expression Reduce()
        {
            return Operation.Switch(Type, Constant(Test), Constant(Cases), Constant(DefaultBody), Constant(Scope));
        }

        public override void SetChildrenScopes(DragonScope scope)
        {
            Test.SetScope(scope);
            if (DefaultBody != null) DefaultBody.SetScope(scope);
            foreach (var @case in Cases)
            {
                foreach (var test in @case.TestValues) test.SetScope(scope);
                @case.Body.SetScope(scope);
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("switch({0}) ", Test);
            sb.Append("{");
            sb.AppendLine();
            foreach (var @case in Cases)
            {
                sb.AppendFormat("  case {0}:", @case.TestValues[0]);
                sb.AppendLine();
                sb.AppendFormat("{0}", @case.Body);
                sb.AppendLine();
                sb.AppendLine("    break;");
            }

            if (DefaultBody != null)
            {
                sb.AppendLine("  default:");
                sb.AppendFormat("{0}", DefaultBody);
                sb.AppendLine("    break;");
            }

            sb.AppendLine("};");
            return sb.ToString();
        }
    }
}