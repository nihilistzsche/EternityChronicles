// RuntimeOperations.cs in EternityChronicles/IronDragon
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
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using IronDragon.Builtins;
using IronDragon.Expressions;

namespace IronDragon.Runtime
{
    using E = ExpressionType;

    public static class StringExtensions
    {
        public static string Capitalize(this string @this)
        {
            return string.Format("{0}{1}", @this.ToUpper().Substring(0, 1), @this.Substring(1));
        }
    }

    /// <summary>
    ///     This class provides the operations Dragon needs to operate.  It houses the methods that makes up the IS
    ///     runtime, which works in conjunction with the DLR runtime.
    /// </summary>
    public static partial class RuntimeOperations
    {
        private static MethodInfo OpToMethod(string op)
        {
            return typeof(RuntimeOperations).GetMethod(op,
                                                       BindingFlags.NonPublic |
                                                       BindingFlags.Static);
        }

        internal static Expression Op(string op, Type expectedType, params Expression[] args)
        {
            if (op == "Convert") return Expression.Convert(Expression.Call(null, OpToMethod(op), args), expectedType);
            return DragonExpression.Convert(Expression.Call(null, OpToMethod(op), args), expectedType);
        }


        private static dynamic Binary(object left, object right, E type, object _scope)
        {
            var args = new List<Expression>();
            args.Add(Expression.Constant(left, typeof(object)));
            args.Add(Expression.Constant(right, typeof(object)));

            return Dynamic(typeof(object), new InteropBinder.Binary((DragonScope)_scope, type),
                           args);
        }

        private static FunctionArgument Arg(object val)
        {
            return val as FunctionArgument ?? new FunctionArgument(null, Expression.Constant(val));
        }

        private static DynamicMetaObject DMO(dynamic val)
        {
            return DynamicMetaObject.Create(val, Expression.Constant(val));
        }

        private static List<FunctionArgument> L(params FunctionArgument[] args)
        {
            return new List<FunctionArgument>(args);
        }

        private static DynamicMetaObject[] _DMO(params DynamicMetaObject[] args)
        {
            return args;
        }

        private static dynamic Compare(object left, object right, object rawScope)
        {
            var scope = (DragonScope)rawScope;

            var DragonName = "<=>";
            var clrName = InteropBinder.ToClrOperatorName(DragonName);

            if (left is DragonInstance)
            {
                var lo = (DragonInstance)left;
                var dmo = lo.GetMetaObject(Expression.Constant(left));
                if (InteropBinder.InvokeMember.SearchForFunction(lo.Class, DragonName, lo, L(Arg(right)), true) != null)
                    return dmo.BindInvokeMember(new InteropBinder.InvokeMember(DragonName, new CallInfo(1), scope),
                                                _DMO(DMO(scope), DMO(Arg(right))));
                if (InteropBinder.InvokeMember.SearchForFunction(lo.Class, clrName, lo, L(Arg(right)), true) != null)
                    return dmo.BindInvokeMember(new InteropBinder.InvokeMember(clrName, new CallInfo(1), scope),
                                                _DMO(DMO(scope), DMO(Arg(right))));
            }

            var Value = Dragon.Box(left);
            if (Value.Class != null)
            {
                var _dmo = Value.GetMetaObject(Expression.Constant(Value));
                if (InteropBinder.InvokeMember.SearchForFunction(Value.Class, DragonName, Value, L(Arg(right)), true) !=
                    null)
                    return _dmo.BindInvokeMember(new InteropBinder.InvokeMember(DragonName, new CallInfo(1), scope),
                                                 _DMO(DMO(scope), DMO(Arg(right))));
                if (InteropBinder.InvokeMember.SearchForFunction(Value.Class, clrName, Value, L(Arg(right)), true) !=
                    null)
                    return _dmo.BindInvokeMember(new InteropBinder.InvokeMember(clrName, new CallInfo(1), scope),
                                                 _DMO(DMO(scope), DMO(Arg(right))));
            }

            dynamic _left = left;
            dynamic _right = right;
            if (_left < _right) return -1;

            if (_left > _right) return 1;

            return 0;
        }

        private static dynamic Match(object rawLeft, object rawRight, DragonExpressionType dragonBinaryNodeType,
                                     object rawScope)
        {
            var scope = (DragonScope)rawScope;

            var left = CompilerServices.CompileExpression((Expression)rawLeft, scope);
            var right = CompilerServices.CompileExpression((Expression)rawRight, scope);

            var DragonName = "=~";
            var clrName = InteropBinder.ToClrOperatorName(DragonName);

            if (left is DragonInstance)
            {
                var lo = (DragonInstance)left;
                DynamicMetaObject dmo = lo.GetMetaObject(Expression.Constant(left));
                if (InteropBinder.InvokeMember.SearchForFunction(lo.Class, DragonName, lo, L(Arg(right)), true) != null)
                {
                    if (dragonBinaryNodeType == DragonExpressionType.NotMatch)
                        return !dmo.BindInvokeMember(new InteropBinder.InvokeMember(DragonName, new CallInfo(1), scope),
                                                     _DMO(DMO(scope), DMO(Arg(right))));
                    return dmo.BindInvokeMember(new InteropBinder.InvokeMember(DragonName, new CallInfo(1), scope),
                                                _DMO(DMO(scope), DMO(Arg(right))));
                }

                if (InteropBinder.InvokeMember.SearchForFunction(lo.Class, clrName, lo, L(Arg(right)), true) != null)
                {
                    if (dragonBinaryNodeType == DragonExpressionType.NotMatch)
                        return !dmo.BindInvokeMember(new InteropBinder.InvokeMember(clrName, new CallInfo(1), scope),
                                                     _DMO(DMO(scope), DMO(Arg(right))));
                    return dmo.BindInvokeMember(new InteropBinder.InvokeMember(clrName, new CallInfo(1), scope),
                                                _DMO(DMO(scope), DMO(Arg(right))));
                }
            }

            var Value = Dragon.Box(left);
            if (Value.Class != null)
            {
                var _dmo = Value.GetMetaObject(Expression.Constant(Value));
                if (InteropBinder.InvokeMember.SearchForFunction(Value.Class, DragonName, Value, L(Arg(right)), true) !=
                    null)
                {
                    if (dragonBinaryNodeType == DragonExpressionType.NotMatch)
                        return !_dmo.BindInvokeMember(
                                                      new InteropBinder.InvokeMember(DragonName, new CallInfo(1),
                                                          scope),
                                                      _DMO(DMO(scope), DMO(Arg(right))));
                    return _dmo.BindInvokeMember(new InteropBinder.InvokeMember(DragonName, new CallInfo(1), scope),
                                                 _DMO(DMO(scope), DMO(Arg(right))));
                }

                if (InteropBinder.InvokeMember.SearchForFunction(Value.Class, clrName, Value, L(Arg(right)), true) !=
                    null)
                {
                    if (dragonBinaryNodeType == DragonExpressionType.NotMatch)
                        return !_dmo.BindInvokeMember(new InteropBinder.InvokeMember(clrName, new CallInfo(1), scope),
                                                      _DMO(DMO(scope), DMO(Arg(right))));
                    return _dmo.BindInvokeMember(new InteropBinder.InvokeMember(clrName, new CallInfo(1), scope),
                                                 _DMO(DMO(scope), DMO(Arg(right))));
                }
            }

            if (!(left is Regex || right is Regex)) return null;

            var left1 = left as Regex;
            var regex = left1 ?? (Regex)right;

            var str = left is Regex ? (string)right : (string)left;

            if (!regex.Match(str).Success) return dragonBinaryNodeType == DragonExpressionType.NotMatch;
            var groups = regex.Match(str).Groups;

            foreach (var groupName in regex.GetGroupNames()) scope[groupName] = groups[groupName].Value;


            return dragonBinaryNodeType == DragonExpressionType.Match;
        }

        private static dynamic Unary(object expr, E type, object _scope)
        {
            var args = new List<Expression>();
            args.Add(Expression.Constant(expr, typeof(object)));

            return Dynamic(typeof(object), new InteropBinder.Unary((DragonScope)_scope, type), args);
        }

        private static dynamic Dynamic(Type returnType, CallSiteBinder binder, IEnumerable<Expression> args)
        {
            return CompilerServices.CreateLambdaForExpression(Expression.Dynamic(binder, returnType, args.ToArray()))();
        }

        private static dynamic Boolean(object value)
        {
            var numberTypes = new[]
                              {
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
            if (value == null) return false;
            if (value is DragonNumber) value = DragonNumber.Convert((DragonNumber)value);
            if (value is bool) return (bool)value;
            if (numberTypes.Contains(value.GetType()))
            {
                dynamic _value = value;
                return _value != 0;
            }

            return value != null;
        }

        private static dynamic TryConvert<T>(object value)
        {
            try
            {
                return (T)value;
            }
            catch
            {
                // Workarounds for known conversion problems.

                // double->int
                if (value is double && typeof(T) == typeof(int)) return (int)(double)value;
                if (value is int && typeof(T) == typeof(double)) // int->double
                    return (double)(int)value;
                return null;
            }
        }

        internal static dynamic Convert(object value, Type type)
        {
            var tryConvert =
                typeof(RuntimeOperations).GetMethod("TryConvert", BindingFlags.NonPublic | BindingFlags.Static)
                                         .MakeGenericMethod(type);

            return tryConvert.Invoke(null, new[] { value });
        }

        internal static dynamic ConvertIfNumber(object value)
        {
            var number = value as DragonNumber;
            return number != null ? DragonNumber.Convert(number) : value;
        }
    }
}