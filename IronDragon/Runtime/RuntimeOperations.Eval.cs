// RuntimeOperations.Eval.cs in EternityChronicles/IronDragon
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
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using IronDragon.Builtins;
using IronDragon.Expressions;
using IronDragon.Parser;

namespace IronDragon.Runtime
{
    using E = ExpressionType;

    /// <summary>
    ///     This class provides the operations Dragon needs to operate.  It houses the methods that makes up the IS
    ///     runtime, which works in conjunction with the DLR runtime.
    /// </summary>
    public static partial class RuntimeOperations
    {
        internal static dynamic String(object rawEval, object rawScope)
        {
            StringBuilder @new;
            var eval = rawEval as string;

            var components = eval.Split(new[] { "#{" }, StringSplitOptions.None);
            if (components.Count() == 1) return new DragonString(eval);
            @new = new StringBuilder(components[0]);
            for (var i = 1; i < components.Count(); i++)
            {
                var parts = components[i].Split(new[] { "}" }, StringSplitOptions.None);
                var expression = parts[0];
                var escapeString = false;
                if (expression != null && expression[0] == ':')
                {
                    escapeString = true;
                    expression = expression[1..];
                }

                if (expression != null)
                {
                    var scope = (DragonScope)rawScope;
                    var xexpression = string.Format("{0};", expression);

                    var res = DragonParser.Parse(xexpression);
                    Expression block;
                    if (res != null)
                        block = DragonExpression.DragonBlock(res);
                    else
                        return null;
                    var myScope = new DragonScope();
                    var visitor = new VariableNameVisitor();
                    visitor.Visit(block);
                    visitor.VariableNames.ForEach(name => myScope[name] = scope[name]);
                    var val = CompilerServices.CompileExpression(block, myScope);
                    if (val != null)
                    {
                        string stringVal = val.ToString();
                        if (escapeString && val is string) stringVal = string.Format("\"{0}\"", stringVal);
                        @new.Append(stringVal ?? "");
                    }
                    else
                    {
                        @new.Append(expression);
                    }
                }

                if (parts.Count() > 1)
                {
                    @new.Append(parts[1]);
                    var j = 2;
                    while (j < parts.Count())
                    {
                        @new.Append("}");
                        @new.Append(parts[j++]);
                    }
                }
            }

            return new DragonString(@new.ToString());
        }

        internal static dynamic Regex(object rawValue)
        {
            var value = (string)rawValue;

            return new Regex(value);
        }

        internal static dynamic Number(object rawValue)
        {
            return new DragonNumber(rawValue);
        }
    }
}