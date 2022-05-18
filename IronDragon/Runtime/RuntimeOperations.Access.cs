// RuntimeOperations.Access.cs in EternityChronicles/IronDragon
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

using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using IronDragon.Builtins;
using IronDragon.Expressions;

namespace IronDragon.Runtime
{
    using E = ExpressionType;

    /// <summary>
    ///     This class provides the operations Dragon needs to operate.  It houses the methods that makes up the IS
    ///     runtime, which works in conjunction with the DLR runtime.
    /// </summary>
    public static partial class RuntimeOperations
    {
        internal static dynamic Access(object container, List<FunctionArgument> args, object rawScope)
        {
            var names = new List<string>();
            for (var i = 0; i < args.Count; i++) names.Add(string.Format("index{0}", i));
            var scope = rawScope as DragonScope;
            var realArgs = new List<object>();
            args.ForEach(arg => realArgs.Add(CompilerServices.CompileExpression(arg.Value, scope)));

            var eArgs = new List<Expression>();
            eArgs.Add(Expression.Constant(container, typeof(object)));
            realArgs.ForEach(arg => eArgs.Add(Expression.Constant(arg)));
            return Dynamic(typeof(object),
                           new InteropBinder.GetIndex(scope, new CallInfo(args.Count, names)), eArgs);
        }

        internal static dynamic AccessSet(object container, List<FunctionArgument> args, object value, E type,
                                          object rawScope)
        {
            var map = new Dictionary<ExpressionType, ExpressionType>();
            map[E.AddAssign] = E.Add;
            map[E.AndAssign] = E.And;
            map[E.DivideAssign] = E.Divide;
            map[E.ExclusiveOrAssign] = E.ExclusiveOr;
            map[E.LeftShiftAssign] = E.LeftShift;
            map[E.ModuloAssign] = E.Modulo;
            map[E.MultiplyAssign] = E.Multiply;
            map[E.OrAssign] = E.Or;
            map[E.PowerAssign] = E.Power;
            map[E.RightShiftAssign] = E.RightShift;
            map[E.SubtractAssign] = E.Subtract;

            var incDecMap = new List<ExpressionType>
                            {
                                E.PreIncrementAssign,
                                E.PreDecrementAssign,
                                E.PostIncrementAssign,
                                E.PostDecrementAssign
                            };

            var names = new List<string>();
            for (var i = 0; i < args.Count; i++) names.Add(string.Format("index{0}", i));

            var scope = rawScope as DragonScope;
            var realArgs = new List<object>();
            args.ForEach(arg => realArgs.Add(CompilerServices.CompileExpression(arg.Value, scope)));


            if (map.ContainsKey(type))
            {
                var nvalue =
                    CompilerServices.CreateLambdaForExpression(
                                                               DragonExpression
                                                                   .Binary(Expression.Constant(Access(container, args, scope)),
                                                                           Expression.Constant(value),
                                                                           map[type]))();
                value = nvalue;
            }

            if (incDecMap.Contains(type))
            {
                if (type is E.PostIncrementAssign or E.PostDecrementAssign)
                {
                    var val = Access(container, args, scope);
                    AccessSet(container, args, 1, type == E.PostIncrementAssign ? E.AddAssign : E.SubtractAssign,
                              rawScope);
                    return val;
                }

                AccessSet(container, args, 1, type == E.PreIncrementAssign ? E.AddAssign : E.SubtractAssign, rawScope);
                return Access(container, args, scope);
            }

            var eArgs = new List<Expression>();
            eArgs.Add(Expression.Constant(container, typeof(object)));
            realArgs.ForEach(arg => eArgs.Add(Expression.Constant(arg)));
            eArgs.Add(Expression.Constant(value, typeof(object)));
            return Dynamic(typeof(object),
                           new InteropBinder.SetIndex(scope, new CallInfo(args.Count, names)), eArgs);
        }

        internal static dynamic ConditionalAccessSet(object container, List<FunctionArgument> args, object value,
                                                     DragonExpressionType conditionalAssignmentType, object rawScope)
        {
            var scope = rawScope as DragonScope;
            var v = Access(container, args, scope);
            if (Boolean(v))
            {
                if (conditionalAssignmentType == DragonExpressionType.IfNotNullAssign)
                    return AccessSet(container, args, value, E.Assign, scope);
            }
            else
            {
                if (conditionalAssignmentType == DragonExpressionType.IfNullAssign)
                    return AccessSet(container, args, value, E.Assign, scope);
            }

            return v;
        }

        internal static dynamic CreateArray(List<Expression> values)
        {
            var list = new List<object>();
            values.ForEach(value => list.Add(CompilerServices.CreateLambdaForExpression(value)()));
            return new DragonArray(list);
        }

        internal static dynamic CreateDictionary(IEnumerable<Expression> values)
        {
            var dict = new Dictionary<object, object>();
            foreach (var val in values.Select(val => CompilerServices.CreateLambdaForExpression(val)()))
                dict[((KeyValuePair<object, object>)val).Key] = ((KeyValuePair<object, object>)val).Value;
            return new DragonDictionary(dict);
        }

        internal static dynamic KeyValuePair(object key, object value)
        {
            return new KeyValuePair<object, object>(key, value);
        }
    }
}