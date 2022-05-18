// InteropBinder.cs in EternityChronicles/IronDragon
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
using IronDragon.Builtins;
using IronDragon.Expressions;
using BlockExpression = IronDragon.Expressions.BlockExpression;

namespace IronDragon.Runtime
{
    internal interface IInteropBinder
    {
        DragonScope Scope { get; }
    }

    internal static class InteropBinder
    {
        #region Binary

        internal sealed class Binary : BinaryOperationBinder, IInteropBinder
        {
            private static readonly ExpressionType[] BoolTypes = { ExpressionType.AndAlso, ExpressionType.OrElse };

            private static readonly ExpressionType[] IntTypes =
            {
                ExpressionType.And, ExpressionType.Or, ExpressionType.ExclusiveOr, ExpressionType.LeftShift,
                ExpressionType.RightShift, ExpressionType.Modulo
            };

            private static readonly ExpressionType[] DoubleTypes = { ExpressionType.Power };

            internal Binary(DragonScope scope, ExpressionType operation) : base(operation)
            {
                Scope = scope;
            }

            public Type ResultType => typeof(object);

            public DragonScope Scope { get; }

            public static DynamicMetaObject Bind(BinaryOperationBinder binder, DragonMetaObject target,
                                                 DynamicMetaObject arg)
            {
                return binder.FallbackBinaryOperation(target, arg);
            }

            private static FunctionArgument Arg(object val)
            {
                return val as FunctionArgument ?? new FunctionArgument(null, Expression.Constant(val));
            }

            private static DynamicMetaObject Dmo(dynamic val)
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

            public override DynamicMetaObject FallbackBinaryOperation(DynamicMetaObject target, DynamicMetaObject arg,
                                                                      DynamicMetaObject errorSuggestion)
            {
                object left = Check(target.Value);
                object right = Check(arg.Value);

                DragonInstance value = Dragon.Box(left);
                var dragonName = MapExpressionType(Operation);
                if (dragonName != null)
                {
                    var dmo = value.GetMetaObject(Expression.Constant(value));
                    if (InvokeMember.SearchForFunction(value.Class, dragonName, value, L(Arg(arg.Value)), true) != null)
                        return dmo.BindInvokeMember(new InvokeMember(dragonName, new CallInfo(1), Scope),
                                                    _DMO(Dmo(Scope), Dmo(Arg(arg.Value))));
                    var clrName = ToClrOperatorName(dragonName);
                    if (InvokeMember.SearchForFunction(value.Class, clrName, value, L(Arg(arg.Value)), true) != null)
                        return dmo.BindInvokeMember(new InvokeMember(clrName, new CallInfo(1), Scope),
                                                    _DMO(Dmo(Scope), Dmo(Arg(arg.Value))));
                }

                Expression eleft = Expression.Constant(left);
                Expression eright = Expression.Constant(right);
                var rl = eleft;
                var rr = eright;

                if (BoolTypes.Contains(Operation))
                {
                    if (rl.Type != typeof(bool)) rl = DragonExpression.Convert(rl, typeof(bool));
                    if (rr.Type != typeof(bool)) rr = DragonExpression.Convert(rr, typeof(bool));
                }
                else if (IntTypes.Contains(Operation))
                {
                    if (rl.Type != typeof(int)) rl = DragonExpression.Convert(rl, typeof(int));
                    if (rr.Type != typeof(int)) rr = DragonExpression.Convert(rr, typeof(int));
                }
                else if (DoubleTypes.Contains(Operation))
                {
                    if (rl.Type != typeof(double)) rl = DragonExpression.Convert(rl, typeof(double));
                    if (rr.Type != typeof(double)) rr = DragonExpression.Convert(rr, typeof(double));
                }
                else
                {
                    if (rl.Type == typeof(object) &&
                        Operation is not ExpressionType.Equal and not ExpressionType.NotEqual)
                    {
                        if (rr.Type != typeof(object))
                            rl = DragonExpression.Convert(rl, rr.Type);
                        else
                            rl = DragonExpression.Convert(rl, typeof(double));
                    }

                    if (rr.Type != rl.Type) rr = DragonExpression.Convert(rr, rl.Type);
                }

                Expression expr = Expression.MakeBinary(Operation, rl, rr);

                expr = DragonExpression.Convert(expr, typeof(object));

                var l = CompilerServices.CreateLambdaForExpression(expr);
                var val = l();

                return new DynamicMetaObject(Expression.Constant(val, typeof(object)), GetRes());
            }
        }

        #endregion

        #region Unary

        internal sealed class Unary : UnaryOperationBinder, IInteropBinder
        {
            internal Unary(DragonScope scope, ExpressionType operation) : base(operation)
            {
                Scope = scope;
            }

            public Type ResultType => typeof(object);

            public DragonScope Scope { get; }

            public static DynamicMetaObject Bind(UnaryOperationBinder binder, DragonMetaObject target)
            {
                return binder.FallbackUnaryOperation(target);
            }

            public override DynamicMetaObject FallbackUnaryOperation(DynamicMetaObject target,
                                                                     DynamicMetaObject errorSuggestion)
            {
                var xre = Check(target.Value);

                Expression re = Expression.Constant(xre);
                if (Operation == ExpressionType.OnesComplement)
                    re = DragonExpression.Convert(re, typeof(int));
                else if (Operation == ExpressionType.Not) re = DragonExpression.Convert(re, typeof(bool));

                Expression expr = Expression.MakeUnary(Operation, re, re.Type);

                expr = DragonExpression.Convert(expr, typeof(object));

                var l = CompilerServices.CreateLambdaForExpression(expr);
                var val = l();
                return new DynamicMetaObject(Expression.Constant(val, typeof(object)), GetRes());
            }
        }

        #endregion

        #region GetIndex

        internal sealed class GetIndex : GetIndexBinder, IInteropBinder
        {
            private MetaObjectBuilder _metaBuilder;

            internal GetIndex(DragonScope scope, CallInfo info) : base(info)
            {
                Scope = scope;
            }

            public Type ResultType => typeof(object);

            public DragonScope Scope { get; }

            public static DynamicMetaObject Bind(GetIndexBinder binder, DragonMetaObject target,
                                                 DynamicMetaObject[] indexes)
            {
                ((GetIndex)binder)._metaBuilder = new MetaObjectBuilder(target, indexes);
                return binder.FallbackGetIndex(target, indexes);
            }

            public override DynamicMetaObject FallbackGetIndex(DynamicMetaObject target, DynamicMetaObject[] indexes,
                                                               DynamicMetaObject errorSuggestion)
            {
                dynamic container = target.Value;
                var args = new List<dynamic>();
                indexes.ToList().ForEach(arg => args.Add(Check(arg.Value)));

                var firstIsRange = args[0].GetType() == typeof(DragonRange);
                MethodInfo getItem = GetIndexerProperty(container, true);
                Expression expr = null;
                if (getItem != null)
                {
                    if (firstIsRange)
                    {
                        var vl = new List<dynamic>();
                        var r = args[0] as DragonRange;
                        for (var i = r.Start; r.Inclusive ? i <= r.End : i < r.End; i++)
                        {
                            var val = getItem.Invoke(container, new object[] { i });
                            vl.Add(val);
                        }

                        expr = Expression.Constant(new DragonArray(vl));
                    }
                    else
                    {
                        var val = getItem.Invoke(container, args.ToArray());
                        expr = Expression.Constant(val);
                    }
                }
                else
                {
                    Array arr = GetArray(container);
                    if (arr != null)
                    {
                        if (firstIsRange)
                        {
                            var vl = new List<dynamic>();
                            var r = args[0] as DragonRange;
                            for (var i = r.Start; r.Inclusive ? i <= r.End : i < r.End; i++)
                            {
                                var val = arr.GetValue(i);
                                vl.Add(val);
                            }

                            expr = Expression.Constant(new DragonArray(vl));
                        }
                        else
                        {
                            dynamic val = arr.GetValue((int)args[0]);
                            expr = Expression.Constant(val);
                        }
                    }
                }

                if (expr == null) return DynamicMetaObject.Create(null, Expression.Constant(null));

                expr = DragonExpression.Convert(expr, typeof(object));

                var l = CompilerServices.CreateLambdaForExpression(expr);
                var xval = l();
                if (_metaBuilder != null)
                {
                    _metaBuilder.SetMetaResult(Expression.Constant(xval, typeof(object)));
                    _metaBuilder.AddRestriction(GetRes());
                    return _metaBuilder.CreateMetaObject(this);
                }

                return new DynamicMetaObject(Expression.Constant(xval, typeof(object)), GetRes());
            }
        }

        #endregion

        #region SetIndex

        internal sealed class SetIndex : SetIndexBinder, IInteropBinder
        {
            private MetaObjectBuilder _metaBuilder;

            internal SetIndex(DragonScope scope, CallInfo info) : base(info)
            {
                Scope = scope;
            }

            public Type ResultType => typeof(object);

            public DragonScope Scope { get; }

            public static DynamicMetaObject Bind(SetIndexBinder binder, DragonMetaObject target,
                                                 DynamicMetaObject[] indexes, DynamicMetaObject value)
            {
                ((SetIndex)binder)._metaBuilder = new MetaObjectBuilder(target, indexes);
                return binder.FallbackSetIndex(target, indexes, value);
            }

            public override DynamicMetaObject FallbackSetIndex(DynamicMetaObject target, DynamicMetaObject[] indexes,
                                                               DynamicMetaObject value,
                                                               DynamicMetaObject errorSuggestion)
            {
                dynamic container = target.Value;
                var args = new List<dynamic>();
                indexes.ToList().ForEach(arg => args.Add(Check(arg.Value)));

                dynamic val = value.Value;

                MethodInfo setItem = GetIndexerProperty(container, false);
                Expression expr = null;
                if (setItem != null)
                {
                    var realArgs = new List<dynamic>(args) { val };
                    setItem.Invoke(container, realArgs.ToArray());
                    expr = Expression.Constant(val);
                }
                else
                {
                    Array arr = GetArray(container);
                    if (arr != null)
                    {
                        arr.SetValue(val, (int)args[0]);
                        expr = Expression.Constant(val);
                    }
                }

                if (expr == null) return DynamicMetaObject.Create(null, Expression.Constant(null));

                expr = DragonExpression.Convert(expr, typeof(object));

                var l = CompilerServices.CreateLambdaForExpression(expr);
                var xval = l();

                if (_metaBuilder != null)
                {
                    _metaBuilder.SetMetaResult(Expression.Constant(xval, typeof(object)));
                    _metaBuilder.AddRestriction(GetRes());
                    return _metaBuilder.CreateMetaObject(this);
                }

                return new DynamicMetaObject(Expression.Constant(xval, typeof(object)),
                                             GetRes());
            }
        }

        #endregion

        #region Invoke

        internal sealed class Invoke : InvokeBinder, IInteropBinder
        {
            internal Invoke(DragonScope /*!*/scope, CallInfo /*!*/callInfo)
                : base(callInfo)
            {
                Scope = scope;
            }

            public Type /*!*/ ResultType => typeof(object);

            private static bool DragonScopeFound { get; set; }

            public DragonScope Scope { get; }

            private static object Arg(object val)
            {
                if (val is DragonScope)
                {
                    DragonScopeFound = true;
                    return val;
                }

                return val as FunctionArgument ?? new FunctionArgument(null, Expression.Constant(val));
            }

            private static DynamicMetaObject Dmo(dynamic val)
            {
                return DynamicMetaObject.Create(val, Expression.Constant(val));
            }

            public static DynamicMetaObject /*!*/ Bind(InvokeBinder binder,
                                                       DragonMetaObject target, DynamicMetaObject[] args)
            {
                var realArgs = new List<DynamicMetaObject>();
                args.ToList().ForEach(arg => realArgs.Add(Dmo(Arg(arg.Value))));

                if (!DragonScopeFound)
                    realArgs.Insert(0, Dmo(((Invoke)binder).Scope ?? new DragonScope()));
                else
                    DragonScopeFound = false;

                return binder.FallbackInvoke(target, realArgs.ToArray(), null);
            }

            public override string /*!*/ ToString()
            {
                return string.Format("Interop.Invoke({0})",
                                     CallInfo.ArgumentCount
                                    );
            }

            #region implemented abstract members of InvokeBinder

            public override DynamicMetaObject FallbackInvoke(DynamicMetaObject target, DynamicMetaObject[] args,
                                                             DynamicMetaObject errorSuggestion)
            {
                dynamic rawFunction = target.Value;
                var myArgs = new List<object>();
                args.ToList().ForEach(arg => myArgs.Add(arg.Value));
                var varArgs = new List<dynamic>();
                var scope = (DragonScope)myArgs[0];
                myArgs.Remove(scope);
                DragonFunction function;
                if (rawFunction is DragonPartialFunction)
                {
                    function = (rawFunction as DragonPartialFunction).WrappedFunction;
                    var pipeType = DragonExpressionType.Empty;
                    if (myArgs.Last() is DragonExpressionType)
                    {
                        pipeType = (DragonExpressionType)myArgs.Last();
                        myArgs.Remove(myArgs.Last());
                    }

                    var newArgs = new List<object>(myArgs);
                    (rawFunction as DragonPartialFunction).PartialArguments.ForEach(arg =>
                    {
                        if (pipeType == DragonExpressionType.BackwardPipe)
                            newArgs.Insert(0, arg);
                        else
                            newArgs.Add(arg);
                    });
                    myArgs = new List<object>(newArgs);
                    scope = MergeScopes((rawFunction as DragonPartialFunction).WrappedScope, scope);
                }
                else
                {
                    function = (DragonFunction)rawFunction;
                }

                var mc = myArgs.Count();
                var fc = function.Arguments.Count();
                var partialCheck = true;
                var argNames = new List<string>();

                function.Arguments.ForEach(arg =>
                                           {
                                               if (arg.HasDefault)
                                               {
                                                   scope[arg.Name] =
                                                       CompilerServices.CompileExpression(arg.DefaultValue, scope);
                                                   fc--;
                                               }

                                               if (arg.IsVarArg)
                                               {
                                                   scope[arg.Name] = varArgs;
                                                   fc--;
                                               }

                                               argNames.Add(arg.Name);
                                           });

                myArgs.ForEach(arg =>
                               {
                                   if (((FunctionArgument)arg).Name != null) partialCheck = false;
                               });

                if (partialCheck && mc < fc)
                {
                    var pArgs = myArgs.ConvertAll(arg => (FunctionArgument)arg);
                    var pfunc = new DragonPartialFunction(function, pArgs, scope);
                    return new DynamicMetaObject(Expression.Constant(pfunc, typeof(object)), GetRes());
                }

                var offset = 0;
                int ix;

                var visitor = new YieldCheckVisitor();
                visitor.Visit(function.Body);
                var hasYieldCall = visitor.HasYield;
                var lastIsFunc = function.Arguments.Any() && function.Arguments.Last().IsFunction;

                var xScope = MergeScopes(function.Context, scope);

                for (ix = 0; ix < myArgs.Count; ix++)
                {
                    var myArg = (FunctionArgument)myArgs[ix];
                    FunctionArgument xArg = null;

                    if (myArg.Name != null)
                    {
                        var margs = function.Arguments.Where(arg => arg.Name == myArg.Name);
                        if (margs.Any()) xArg = margs.First();
                        if (myArg.Name == "__yieldBlock")
                            xArg = lastIsFunc ? function.Arguments.Last() : new FunctionArgument("__yieldBlock");
                    }
                    else
                    {
                        if (ix + 1 == myArgs.Count && (lastIsFunc || hasYieldCall))
                            xArg = lastIsFunc ? function.Arguments.Last() : new FunctionArgument("__yieldBlock");
                    }

                    if (xArg == null && function.Arguments.Count > ix + offset) xArg = function.Arguments[ix + offset];

                    if (xArg == null) break;

                    if (myArg.Name != null) offset = xArg.Index - ix;

                    if (xArg.IsVarArg)
                    {
                        for (var jx = ix; jx < myArgs.Count; jx++)
                        {
                            var nArg = myArgs[jx] as FunctionArgument;
                            var val = CompilerServices.CompileExpression(nArg.Value, scope);
                            if ((nArg.Name != null && nArg.Name == "__yieldBlock") ||
                                (jx + 1 == myArgs.Count && (lastIsFunc || hasYieldCall)))
                            {
                                var oldxArg = xArg;
                                if (lastIsFunc)
                                    xArg = function.Arguments.Last();
                                else
                                    xArg = new FunctionArgument("__yieldBlock");
                                xScope[xArg.Name] = val;
                                xArg = oldxArg;
                            }
                            else
                            {
                                varArgs.Add(val);
                            }
                        }

                        xScope[xArg.Name] = varArgs;
                    }
                    else
                    {
                        if (xArg.IsLiteral)
                        {
                            if (!(myArg.Value is VariableExpression))
                                return DynamicMetaObject.Create(null, Expression.Constant(null));

                            var lvar = myArg.Value as VariableExpression;
                            xScope[xArg.Name] = lvar.HasSym
                                                    ? lvar.Sym.Name
                                                    : CompilerServices.CompileExpression(lvar.Name, scope);
                        }
                        else
                        {
                            xScope[xArg.Name] = CompilerServices.CompileExpression(myArg.Value, scope);
                        }
                    }
                }

                var xBody = new List<Expression>(function.Body.Body);

                var ret = xBody.Last();
                xBody.Remove(ret);
                var newLast = xBody.Last();
                if (newLast.NodeType == ExpressionType.Extension && newLast.GetType() == typeof(ReturnExpression))
                {
                    xBody.Add(ret);
                }
                else
                {
                    xBody.Remove(newLast);
                    xBody.Add(
                              DragonExpression.Assign(
                                                      new LeftHandValueExpression(
                                                       new VariableExpression(Expression
                                                                                  .Constant("<dragon_function_value>"))),
                                                      newLast));
                    xBody.Add(ret);
                }

                xScope["$:"] = scope;

                var realBody = new BlockExpression(xBody);
                realBody.SetScope(xScope);

                Expression expr = Expression.Block(new ParameterExpression[] { }, realBody);

                var xval = CompilerServices.CreateLambdaForExpression(expr)();
                if (xval == null && xScope["<dragon_function_value>"] != null) xval = xScope["<dragon_function_value>"];

                L(xScope.Variables)
                    .ForEach(
                             var =>
                             {
                                 if (scope.Variables.ContainsKey(var.Key) && !argNames.Contains(var.Key))
                                     scope[var.Key] = var.Value;
                             });
                L(xScope.SymVars)
                    .ForEach(var =>
                             {
                                 if (scope.SymVars.ContainsKey(var.Key)) scope[var.Key] = var.Value;
                             });

                return new DynamicMetaObject(Expression.Constant(xval, typeof(object)), GetRes());
            }

            #endregion
        }

        #endregion

        #region InvokeMember

        internal sealed class InvokeMember : InvokeMemberBinder, IInteropBinder
        {
            public InvokeMember(string name, CallInfo info, DragonScope scope)
                : base(name, false, info)
            {
                Scope = scope;
            }

            public Type /*!*/ ResultType => typeof(object);

            private static bool DragonScopeFound { get; set; }

            public DragonScope Scope { get; }

            private static object Arg(object val)
            {
                if (val is DragonScope)
                {
                    DragonScopeFound = true;
                    return val;
                }

                if (val is DragonDoNotWrapBoolean) return val;
                return val as FunctionArgument ?? new FunctionArgument(null, Expression.Constant(val));
            }

            private static DynamicMetaObject Dmo(dynamic val)
            {
                return DynamicMetaObject.Create(val, Expression.Constant(val));
            }

            public static DynamicMetaObject /*!*/ Bind(InvokeMemberBinder binder,
                                                       DragonMetaObject target, DynamicMetaObject[] args)
            {
                var realArgs = new List<DynamicMetaObject>();
                var first = true;
                args.ToList().ForEach(arg =>
                                      {
                                          if ((first && arg.Value != null) || !first) realArgs.Add(Dmo(Arg(arg.Value)));
                                          first = false;
                                      });

                if (!DragonScopeFound)
                    realArgs.Insert(0, Dmo(((InvokeMember)binder).Scope ?? new DragonScope()));
                else
                    DragonScopeFound = false;

                return binder.FallbackInvokeMember(target, realArgs.ToArray(), null);
            }

            #region implemented abstract members of InvokeMemberBinder

            private static object CreateInstanceOf(Type type)
            {
                var ctor = type.GetConstructors().FirstOrDefault();
                if (ctor == null) return null;
                var cargs = new List<object>();
                ctor.GetParameters()
                    .ToList()
                    .ForEach(
                             p =>
                                 cargs.Add(CompilerServices.CompileExpression(Expression.Default(p.ParameterType),
                                                                              new DragonScope())));
                return ctor.Invoke(cargs.ToArray());
            }

            private static Type GetDotNetParent(DragonInstance obj)
            {
                if (obj is DragonBoxedInstance) return null;
                var klass = obj.Class.Parent;
                while (klass != null)
                {
                    if (klass is DragonBoxedClass && ((DragonBoxedClass)klass).BoxedType != typeof(object))
                        return ((DragonBoxedClass)klass).BoxedType;
                    klass = klass.Parent;
                }

                return null;
            }

            internal static DragonFunction SearchForFunction(DragonClass @class, string name, DragonInstance obj,
                                                             List<FunctionArgument> args,
                                                             bool isInstance = false, bool exactMatch = false,
                                                             bool comingFromObjectRemove = false)
            {
                DragonFunction func = null;

                // First check to see if its undefined or removed
                if (isInstance)
                {
                    if (obj.UndefinedMethods.Contains(name) || @class.UndefinedMethods.Contains(name)) return null;
                    if ((!comingFromObjectRemove && obj.RemovedMethods.Contains(name)) ||
                        @class.RemovedMethods.Contains(name))
                        return SearchForFunction(@class.Parent, name, obj, args, true, exactMatch, true);
                    if (obj.SingletonMethods.ContainsKey(name)) return obj.SingletonMethods[name];
                    if (@class.InstanceMethods.ContainsKey(name))
                        func = @class.InstanceMethods[name].Resolve(args, exactMatch);
                    if (func != null) return func;
                    if (@class.Parent != null) return SearchForFunction(@class.Parent, name, obj, args, isInstance);
                    return null;
                }

                if (@class.UndefinedMethods.Contains(name)) return null;
                if (@class.RemovedMethods.Contains(name)) return SearchForFunction(@class.Parent, name, obj, args);
                if (@class.ClassMethods.ContainsKey(name))
                {
                    func = @class.ClassMethods[name].Resolve(args, exactMatch);
                    if (func != null) return func;
                }

                if (@class.Parent != null) return SearchForFunction(@class.Parent, name, obj, args, isInstance);
                return null;
            }

            public override DynamicMetaObject FallbackInvokeMember(DynamicMetaObject target, DynamicMetaObject[] args,
                                                                   DynamicMetaObject errorSuggestion)
            {
                dynamic @object = target.Value;
                DragonClass @class;

                var isInstance = false;

                var myArgs = new List<object>();
                args.ToList().ForEach(arg => myArgs.Add(arg.Value));
                var varArgs = new List<dynamic>();
                var scope = new DragonScope((DragonScope)myArgs[0]);

                myArgs.Remove(myArgs[0]);
                var isSuperCall = false;
                var isOp = false;
                var isPostfix = false;
                if (myArgs.Any() && myArgs.Last() is DragonDoNotWrapBoolean && !(myArgs.Last() is DragonUnaryBoolean))
                {
                    isSuperCall = ((DragonDoNotWrapBoolean)myArgs.Last()).Value;
                    myArgs.Remove(myArgs.Last());
                }

                if (myArgs.Any() && myArgs.Last() is DragonUnaryBoolean)
                {
                    isOp = true;
                    isPostfix = ((DragonUnaryBoolean)myArgs.Last()).Value;
                    myArgs.Remove(myArgs.Last());
                }

                if (@object is DragonInstance)
                {
                    @class = ((DragonInstance)@object).Class;
                    scope.MergeWithScope(@class.Context);
                    isInstance = true;
                }
                else if (@object is DragonClass)
                {
                    @class = (DragonClass)@object;
                    scope.MergeWithScope(@class.Context);
                }
                else // Native object
                {
                    var types = new List<Type>();
                    myArgs.ForEach(arg => types.Add(arg.GetType()));
                    var methods = ((object)@object).GetType().GetMethods();
                    var method = ((object)@object).GetType().GetMethod(Name, types.ToArray());
                    var methodCandidates = new List<MethodInfo>();
                    methods.ToList().ForEach(mi =>
                                             {
                                                 if (mi.Name == Name)
                                                 {
                                                     // Check Arguments
                                                     var @params = mi.GetParameters();
                                                     if (@params.Any() &&
                                                         @params.Last()
                                                                .GetCustomAttributes(typeof(ParamArrayAttribute), false)
                                                                .Any())
                                                         methodCandidates.Add(mi);
                                                 }
                                             });
                    if (method == null && methodCandidates.Any()) method = methodCandidates.First();

                    if (method == null)
                    {
                        if (scope[@object.GetType().Name] != null)
                        {
                            var boxobj = new DragonBoxedInstance(@object, scope, scope[@object.GetType().Name]);
                            var boxdmo = boxobj.GetMetaObject(Expression.Constant(boxobj));

                            return boxdmo.BindInvokeMember(new InvokeMember(Name, CallInfo, Scope), args);
                        }

                        var dobj = Dragon.Box(@object);
                        if (scope[((DragonInstance)dobj).Class.Name] != null)
                        {
                            var boxobj = new DragonBoxedInstance(@object, scope, scope[((DragonInstance)dobj).Class.Name]);
                            var boxdmo = boxobj.GetMetaObject(Expression.Constant(boxobj));

                            return boxdmo.BindInvokeMember(new InvokeMember(Name, CallInfo, Scope), args);
                        }

                        var dmo = dobj.GetMetaObject(Expression.Constant(@object));
                        return dmo.BindInvokeMember(new InvokeMember(Name, CallInfo, Scope), args);
                    }

                    var targs = new List<object>();
                    myArgs.ForEach(arg =>
                                   {
                                       if (arg is FunctionArgument)
                                       {
                                           var val = CompilerServices.CompileExpression(((FunctionArgument)arg).Value,
                                               scope);
                                           if (val is DragonString) val = (string)val;
                                           targs.Add(val);
                                       }
                                   });
                    var wargs = new List<object>();

                    var index = 0;
                    var xparams = method.GetParameters();
                    var hasParam = xparams.Any()
                                       ? xparams.Last().GetCustomAttributes(typeof(ParamArrayAttribute), false).Any()
                                       : false;
                    var xcount = method.GetParameters().Count() - (hasParam ? 1 : 0);
                    for (var i = 0; i < xcount; i++) wargs.Add(targs[index++]);
                    if (hasParam)
                    {
                        var xlt = typeof(List<object>).GetGenericTypeDefinition();
                        var at = xparams.Last().ParameterType;
                        var lt = xlt.MakeGenericType(at.GetElementType());
                        dynamic lx = lt.GetConstructor(Type.EmptyTypes).Invoke(null);
                        for (var j = index; j < targs.Count; j++) lx.Add(targs[j]);
                        wargs.Add(lx.ToArray());
                    }

                    return
                        new DynamicMetaObject(
                                              Expression.Constant(method.Invoke(@object, wargs.ToArray()),
                                                                  typeof(object)),
                                              BindingRestrictions.GetExpressionRestriction(Expression.Constant(true)));
                }

                // Find the function in the function tables;
                var function = SearchForFunction(isSuperCall ? @class.Parent : @class, Name,
                                                 isInstance ? @object as DragonInstance : null,
                                                 myArgs.ConvertAll(arg => (FunctionArgument)arg), isInstance, true);
                if (function == null)
                {
                    function = SearchForFunction(isSuperCall ? @class.Parent : @class, Name,
                                                 isInstance ? @object as DragonInstance : null,
                                                 myArgs.ConvertAll(arg => (FunctionArgument)arg), isInstance);

                    if (function == null)
                        return new DynamicMetaObject(Expression.Constant(null, typeof(object)),
                                                     BindingRestrictions
                                                         .GetExpressionRestriction(Expression.Constant(true)));
                }

                var mc = myArgs.Count();
                var fc = function is DragonNativeFunction
                             ? ((DragonNativeFunction)function).NumberOfArguments
                             : function.Arguments.Count();
                var partialCheck = true;
                var argNames = new List<string>();

                function.Arguments.ForEach(arg =>
                                           {
                                               if (arg.HasDefault)
                                               {
                                                   scope[arg.Name] =
                                                       CompilerServices.CompileExpression(arg.DefaultValue, scope);
                                                   fc--;
                                               }

                                               if (arg.IsVarArg)
                                               {
                                                   scope[arg.Name] = varArgs;
                                                   fc = 0;
                                               }

                                               argNames.Add(arg.Name);
                                           });

                myArgs.ForEach(arg =>
                               {
                                   if (((FunctionArgument)arg).Name != null) partialCheck = false;
                               });

                if (partialCheck && mc < fc)
                {
                    // preserve self and super for partial function and invoke member context
                    if (isInstance)
                    {
                        var obj = (DragonInstance)@object;
                        scope["self"] =
                            scope["super"] =
                                obj is DragonBoxedInstance ? ((DragonBoxedInstance)obj).BoxedObject : @object;
                        if (obj is DragonBoxedInstance)
                            scope = MergeScopes(((DragonBoxedInstance)obj).BoxedScope, scope);
                        if (target.Expression is VariableExpression or InstanceReferenceExpression)
                        {
                            if (target.Expression is InstanceReferenceExpression)
                            {
                                var iref = target.Expression as InstanceReferenceExpression;
                                scope["<dragon_context_self>"] = iref.Key;
                            }
                            else
                            {
                                var variable = target.Expression as VariableExpression;
                                scope["<dragon_context_self>"] = variable.Name;
                            }
                        }
                    }
                    else
                    {
                        if (Name == "new")
                        {
                            var newObj = new DragonInstance(@class);
                            newObj.SetScope(scope);
                            scope["self"] = scope["super"] = newObj;
                        }
                        else
                        {
                            scope["self"] = scope["super"] = isSuperCall ? @class.Parent : @class;
                        }
                    }

                    scope["<dragon_context_invokemember>"] = true;
                    string selfName;
                    var selfScope = scope.SearchForObject(scope["self"], out selfName);
                    if (selfScope != null && selfName != null)
                    {
                        scope["<dragon_context_selfscope>"] = selfScope;
                        scope["<dragon_context_selfname>"] = selfName;
                    }

                    var pArgs = myArgs.ConvertAll(arg => (FunctionArgument)arg);
                    var pfunc = new DragonPartialFunction(function, pArgs, scope);
                    return new DynamicMetaObject(Expression.Constant(pfunc, typeof(object)), GetRes());
                }

                var offset = 0;
                int ix;

                var visitor = new YieldCheckVisitor();
                visitor.Visit(function.Body);
                var hasYieldCall = visitor.HasYield;
                var lastIsFunc = function.Arguments.Any() && function.Arguments.Last().IsFunction;
                var xScope = MergeScopes(function.Context, scope);
                for (ix = 0; ix < myArgs.Count; ix++)
                {
                    var myArg = (FunctionArgument)myArgs[ix];
                    FunctionArgument xArg = null;

                    if (myArg.Name != null)
                    {
                        var margs = function.Arguments.Where(arg => arg.Name == myArg.Name);
                        if (margs.Any()) xArg = margs.First();
                        if (myArg.Name == "__yieldBlock")
                            xArg = lastIsFunc ? function.Arguments.Last() : new FunctionArgument("__yieldBlock");
                    }
                    else
                    {
                        if (ix + 1 == myArgs.Count && (lastIsFunc || hasYieldCall))
                            xArg = lastIsFunc ? function.Arguments.Last() : new FunctionArgument("__yieldBlock");
                    }

                    if (xArg == null && function.Arguments.Count > ix + offset) xArg = function.Arguments[ix + offset];

                    if (xArg == null) break;

                    if (myArg.Name != null) offset = xArg.Index - ix;

                    if (xArg.IsVarArg)
                    {
                        for (var jx = ix; jx < myArgs.Count; jx++)
                        {
                            var nArg = myArgs[jx] as FunctionArgument;
                            var val = CompilerServices.CompileExpression(nArg.Value, scope);
                            if ((nArg.Name != null && nArg.Name == "__yieldBlock") ||
                                (jx + 1 == myArgs.Count && (lastIsFunc || hasYieldCall)))
                            {
                                var oldxArg = xArg;
                                xArg = lastIsFunc ? function.Arguments.Last() : new FunctionArgument("__yieldBlock");
                                xScope[xArg.Name] = val;
                                xArg = oldxArg;
                            }
                            else
                            {
                                varArgs.Add(val);
                            }
                        }

                        xScope[xArg.Name] = varArgs;
                    }
                    else
                    {
                        if (xArg.IsLiteral)
                        {
                            if (!(myArg.Value is VariableExpression))
                                return DynamicMetaObject.Create(null, Expression.Constant(null));
                            var lvar = myArg.Value as VariableExpression;
                            xScope[xArg.Name] = lvar.HasSym
                                                    ? lvar.Sym.Name
                                                    : CompilerServices.CompileExpression(lvar.Name, scope);
                        }
                        else
                        {
                            var val = CompilerServices.CompileExpression(myArg.Value, scope);
                            xScope[xArg.Name] = val;
                        }
                    }
                }

                var xBody = new List<Expression>(function.Body.Body);

                var ret = xBody.Last();
                xBody.Remove(ret);
                var newLast = xBody.Last();
                if (newLast.NodeType == ExpressionType.Extension && newLast.GetType() == typeof(ReturnExpression))
                {
                    xBody.Add(ret);
                }
                else
                {
                    xBody.Remove(newLast);
                    xBody.Add(
                              DragonExpression.Assign(
                                                      new LeftHandValueExpression(
                                                       new VariableExpression(Expression
                                                                                  .Constant("<dragon_function_value>"))),
                                                      newLast));
                    xBody.Add(ret);
                }

                if (isInstance)
                {
                    var obj = (DragonInstance)@object;
                    xScope["self"] =
                        xScope["super"] = obj is DragonBoxedInstance ? ((DragonBoxedInstance)obj).BoxedObject : @object;
                    if (obj is DragonBoxedInstance) xScope = MergeScopes(((DragonBoxedInstance)obj).BoxedScope, xScope);
                }
                else
                {
                    if (Name == "new")
                    {
                        var newObj = new DragonInstance(@class);
                        newObj.SetScope(scope);
                        xScope["self"] = xScope["super"] = newObj;
                    }
                    else
                    {
                        xScope["self"] = xScope["super"] = isSuperCall ? @class.Parent : @class;
                    }
                }

                xScope["<dragon_context_invokemember>"] = true;
                xScope["$:"] = scope;
                string xSelfName;
                var xSelfScope = scope.SearchForObject(xScope["self"], out xSelfName);
                if (xSelfScope != null && xSelfName != null)
                {
                    xScope["<dragon_context_selfscope>"] = xSelfScope;
                    xScope["<dragon_context_selfname>"] = xSelfName;
                }

                if (isOp) xScope["__postfix"] = isPostfix;
                var realBody = new BlockExpression(xBody);
                realBody.SetScope(xScope);
                Expression expr = Expression.Block(new ParameterExpression[] { }, realBody);

                if (isSuperCall && isInstance) (@object as DragonInstance).Class = @class.Parent;

                var xval = CompilerServices.CreateLambdaForExpression(expr)();

                if ((object)xval == null && xScope["<dragon_function_value>"] != null)
                    xval = xScope["<dragon_function_value>"];

                if (xval != null)
                {
                    if (xval is DragonBoxedInstance && ((DragonInstance)xval).Class.Name == "Number")
                        xval = ((DragonBoxedInstance)xval).BoxedObject;
                    if (Name == "new" && !(xval is DragonInstance))
                    {
                        if (!(@class is DragonBoxedClass)) // we inherited from .NET but we have a Dragon class
                        {
                            object yval = xval;
                            xval = new DragonInstance(@class);
                            ((DragonInstance)xval).BackingObject = yval;
                            DragonBoxedInstance.SyncInstanceVariablesTo(xval, yval);
                        }
                        else
                        {
                            xval = Dragon.Box(xval);
                        }
                    }
                    else if (Name == "new" && GetDotNetParent(xval) != null)
                    {
                        // we got a Dragon object with a Dragon class but it has a .NET parent
                        ((DragonInstance)xval).BackingObject = CreateInstanceOf(GetDotNetParent(xval));
                    }
                }

                if (isSuperCall && isInstance) (@object as DragonInstance).Class = @class;

                L(xScope.Variables)
                    .ForEach(
                             var =>
                             {
                                 if (scope.Variables.ContainsKey(var.Key) && !argNames.Contains(var.Key))
                                     scope[var.Key] = var.Value;
                             });
                L(xScope.SymVars)
                    .ForEach(var =>
                             {
                                 if (scope.SymVars.ContainsKey(var.Key)) scope[var.Key] = var.Value;
                             });

                return new DynamicMetaObject(Expression.Constant(xval, typeof(object)), GetRes());
            }

            public override DynamicMetaObject FallbackInvoke(DynamicMetaObject target, DynamicMetaObject[] args,
                                                             DynamicMetaObject errorSuggestion)
            {
                return new DynamicMetaObject(Expression.Constant(null, typeof(object)), GetRes());
            }

            #endregion
        }

        #endregion

        #region GetMember

        internal sealed class GetMember : GetMemberBinder, IInteropBinder
        {
            public GetMember(string name, DragonScope scope) : base(name, false)
            {
                Scope = scope;
            }

            public Type /*!*/ ResultType => typeof(object);

            public DragonScope Scope { get; }

            public static DynamicMetaObject Bind(GetMemberBinder binder, DragonMetaObject target)
            {
                return binder.FallbackGetMember(target);
            }

            #region implemented abstract members of GetMemberBinder

            public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target,
                                                                DynamicMetaObject errorSuggestion)
            {
                dynamic xval;
                if (target.Value is DragonInstance)
                {
                    var obj = (DragonInstance)target.Value;
                    if (Name == "class")
                        return new DynamicMetaObject(Expression.Constant(obj.Class, typeof(object)), GetRes());
                    if (obj is DragonBoxedInstance)
                    {
                        var boxobj = ((DragonBoxedInstance)obj).BoxedObject;
                        var fi = boxobj.GetType()
                                     .GetField(Name,
                                               BindingFlags.Instance | BindingFlags.Public |
                                               BindingFlags.NonPublic);
                        try
                        {
                            var val = fi.GetValue(boxobj);
                            return new DynamicMetaObject(Expression.Constant(val, typeof(object)),
                                                         GetRes());
                        }
                        catch (NullReferenceException)
                        {
                        }
                    }

                    var get = string.Format("get_{0}", Name.Capitalize());
                    var put = string.Format("put_{0}", Name.Capitalize());
                    var dmo = obj.GetMetaObject(Expression.Constant(obj));
                    var args = new List<DynamicMetaObject>();
                    args.Add(DynamicMetaObject.Create(Scope, Expression.Constant(Scope)));
                    if (InvokeMember.SearchForFunction(obj.Class, get, obj, new List<FunctionArgument>(), true) != null)
                        return dmo.BindInvokeMember(new InvokeMember(get, new CallInfo(0), Scope), args.ToArray());
                    if (InvokeMember.SearchForFunction(obj.Class, put, obj, new List<FunctionArgument>(), true) != null)
                        return dmo.BindInvokeMember(new InvokeMember(put, new CallInfo(0), Scope), args.ToArray());
                    if (InvokeMember.SearchForFunction(obj.Class, Name, obj, new List<FunctionArgument>(), true) !=
                        null)
                        return dmo.BindInvokeMember(new InvokeMember(Name, new CallInfo(0), Scope), args.ToArray());
                    xval = (target.Value as DragonInstance).InstanceVariables[Name];
                }
                else if (target.Value is DragonClass)
                {
                    var @class = target.Value as DragonClass;
                    var dmo = @class.GetMetaObject(Expression.Constant(@class));
                    var args = new List<DynamicMetaObject>();
                    args.Add(DynamicMetaObject.Create(Scope, Expression.Constant(Scope)));
                    if (InvokeMember.SearchForFunction(@class, Name, null, new List<FunctionArgument>()) != null)
                        return dmo.BindInvokeMember(new InvokeMember(Name, new CallInfo(0), Scope), args.ToArray());
                    xval = ((DragonClass)target.Value).Context[Name];
                }
                else
                {
                    // native object
                    DragonInstance obj = Dragon.Box(target.Value, Scope);
                    var dmo = obj.GetMetaObject(Expression.Constant(obj));
                    return dmo.BindGetMember(new GetMember(Name, Scope));
                }

                return new DynamicMetaObject(Expression.Constant(xval, typeof(object)),
                                             GetRes());
            }

            #endregion
        }

        #endregion

        #region SetMember

        internal sealed class SetMember : SetMemberBinder, IInteropBinder
        {
            public SetMember(string name, DragonScope scope) : base(name, false)
            {
                Scope = scope;
            }

            public Type /*!*/ ResultType => typeof(object);

            public DragonScope Scope { get; }

            public static DynamicMetaObject Bind(SetMemberBinder binder, DragonMetaObject target,
                                                 DynamicMetaObject value)
            {
                return binder.FallbackSetMember(target, value);
            }

            #region implemented abstract members of SetMemberBinder

            private static FunctionArgument Arg(object val)
            {
                return val as FunctionArgument ?? new FunctionArgument(null, Expression.Constant(val));
            }

            private static DynamicMetaObject Dmo(dynamic val)
            {
                return DynamicMetaObject.Create(val, Expression.Constant(val));
            }

            public override DynamicMetaObject FallbackSetMember(DynamicMetaObject target, DynamicMetaObject value,
                                                                DynamicMetaObject errorSuggestion)
            {
                if (Name == "class") return new DynamicMetaObject(Expression.Constant(null), GetRes());
                if (target.Value is DragonInstance)
                {
                    var obj = (DragonInstance)target.Value;
                    if (obj is DragonBoxedInstance)
                    {
                        var boxobj = ((DragonBoxedInstance)obj).BoxedObject;
                        var fi = boxobj.GetType()
                                     .GetField(Name,
                                               BindingFlags.Instance | BindingFlags.Public |
                                               BindingFlags.NonPublic);
                        try
                        {
                            dynamic val = value.Value;
                            if (val is DragonNumber) val = DragonNumber.Convert(val);
                            fi.SetValue(boxobj, val);
                            return new DynamicMetaObject(Expression.Constant(value.Value, typeof(object)),
                                                         GetRes());
                        }
                        catch (NullReferenceException)
                        {
                        }
                    }

                    var set = string.Format("set_{0}", Name.Capitalize());
                    var dmo = obj.GetMetaObject(Expression.Constant(obj));
                    var args = new List<DynamicMetaObject>();
                    args.Add(DynamicMetaObject.Create(Scope, Expression.Constant(Scope)));
                    args.Add(Dmo(Arg(value.Value)));
                    if (
                        InvokeMember.SearchForFunction(obj.Class, set, obj,
                                                       new List<FunctionArgument> { Arg(value.Value) }, true) != null)
                        return dmo.BindInvokeMember(new InvokeMember(set, new CallInfo(0), Scope), args.ToArray());
                    if (
                        InvokeMember.SearchForFunction(obj.Class, string.Format("{0}=", Name), obj,
                                                       new List<FunctionArgument>(), true) != null)
                        return
                            dmo.BindInvokeMember(new InvokeMember(string.Format("{0}=", Name), new CallInfo(0), Scope),
                                                 args.ToArray());
                    if (value.Value is DragonFunction)
                    {
                        ((DragonFunction)value.Value).Name = Name;
                        ((DragonInstance)target.Value).SingletonMethods[Name] = (DragonFunction)value.Value;
                    }
                    else
                    {
                        (target.Value as DragonInstance).InstanceVariables[Name] = value.Value;
                    }
                }
                else if (target.Value is DragonClass)
                {
                    if (value.Value is DragonFunction)
                    {
                        ((DragonFunction)value.Value).Name = Name;
                        DragonClass.AddMethod(((DragonClass)target.Value).InstanceMethods, (DragonFunction)value.Value);
                    }
                    else
                    {
                        ((DragonClass)target.Value).Context[Name] = value.Value;
                    }
                }
                else // native object
                {
                    DragonInstance obj = Dragon.Box(target.Value, Scope);
                    var dmo = obj.GetMetaObject(Expression.Constant(obj));
                    return dmo.BindSetMember(new SetMember(Name, Scope), value);
                }

                return new DynamicMetaObject(Expression.Constant(value.Value, typeof(object)),
                                             GetRes());
            }

            #endregion
        }

        #endregion

        #region Helper Methods

        private static Array GetArray(object container)
        {
            Func<PropertyInfo, object> gv = p => p.GetValue(container, null);

            var a = from p in container.GetType().GetProperties()
                    where gv(p) != null && gv(p).GetType().IsArray
                    select (Array)gv(p);
            return a.FirstOrDefault();
        }

        private static MethodInfo GetIndexerProperty(object container, bool getter)
        {
            var i = from p in container.GetType().GetProperties()
                    where p.GetIndexParameters().Any()
                    select p;

            if (i.Any()) return getter ? i.First().GetGetMethod() : i.First().GetSetMethod();
            return null;
        }

        private static List<dynamic> Flatten(IEnumerable<dynamic> list)
        {
            var @new = new List<dynamic>();
            foreach (var val in list)
            {
                if (!val.GetType().IsArray)
                    @new.Add(val);
                else
                    Flatten(new List<dynamic>(val)).ForEach(xval => @new.Add(xval));
            }

            return @new;
        }

        private static void SetVar(DragonScope scope, KeyValuePair<string, dynamic> var)
        {
            if (!scope.Variables.ContainsKey(var.Key)) scope[var.Key] = var.Value;
        }

        private static void SetVar(DragonScope scope, KeyValuePair<Symbol, dynamic> var)
        {
            if (!scope.SymVars.ContainsKey(var.Key)) scope[var.Key] = var.Value;
        }

        private static List<KeyValuePair<TKey, TValue>> L<TKey, TValue>(Dictionary<TKey, TValue> dict)
        {
            return dict.ToList();
        }

        private static dynamic Check(object obj)
        {
            return obj switch
                   {
                       DragonBoxedInstance instance => instance.BoxedObject is DragonNumber number
                                                           ? DragonNumber.Convert(number)
                                                           : instance,
                       DragonNumber number => DragonNumber.Convert(number),
                       var _ => obj
                   };
        }

        private static DragonScope MergeScopes(DragonScope dest, DragonScope src)
        {
            var scope = new DragonScope { ParentScope = src.ParentScope };
            L(src.Variables).ForEach(var => SetVar(scope, var));
            L(dest.Variables).ForEach(var => SetVar(scope, var));
            L(src.SymVars).ForEach(var => SetVar(scope, var));
            L(dest.SymVars).ForEach(var => SetVar(scope, var));
            return scope;
        }

        private static BindingRestrictions GetRes()
        {
            return BindingRestrictions.GetExpressionRestriction(new TrueOnce());
        }

        // From IronRuby, Copyright:
        /* ****************************************************************************
			 *
		 * Copyright (c) Microsoft Corporation.
			 *
			 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A
			 * copy of the license can be found in the License.html file at the root of this distribution. If
			 * you cannot locate the  Apache License, Version 2.0, please send an email to
			 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound
			 * by the terms of the Apache License, Version 2.0.
			 *
			 * You must not remove this notice, or any other, from this software.
			 *
			 *
			 * ***************************************************************************/

        internal static string ToClrOperatorName(string dragonName)
        {
            switch (dragonName)
            {
            case "+":
                return "op_Addition";
            case "-":
                return "op_Subtraction";
            case "/":
                return "op_Division";
            case "*":
                return "op_Multiply";
            case "%":
                return "op_Modulus";
            case "==":
                return "op_Equality";
            case "!=":
                return "op_Inequality";
            case ">":
                return "op_GreaterThan";
            case ">=":
                return "op_GreaterThanOrEqual";
            case "<":
                return "op_LessThan";
            case "<=":
                return "op_LessThanOrEqual";
            case "-@":
                return "op_UnaryNegation";
            case "+@":
                return "op_UnaryPlus";
            case "<<":
                return "op_LeftShift";
            case ">>":
                return "op_RightShift";
            case "^":
                return "op_ExclusiveOr";
            case "~":
                return "op_OnesComplement";
            case "&":
                return "op_BitwiseAnd";
            case "|":
                return "op_BitwiseOr";

            case "**":
                return "Power";
            case "<=>":
                return "Compare";
            case "=~":
                return "Match";

            default:
                return null;
            }
        }

        internal static string MapExpressionType(ExpressionType type)
        {
            switch (type)
            {
            case ExpressionType.Add:
                return "+";
            case ExpressionType.Subtract:
                return "-";
            case ExpressionType.Divide:
                return "/";
            case ExpressionType.Multiply:
                return "*";
            case ExpressionType.Modulo:
                return "%";
            case ExpressionType.Equal:
                return "==";
            case ExpressionType.NotEqual:
                return "!=";
            case ExpressionType.GreaterThan:
                return ">";
            case ExpressionType.GreaterThanOrEqual:
                return ">=";
            case ExpressionType.LessThan:
                return "<";
            case ExpressionType.LessThanOrEqual:
                return "<=";
            case ExpressionType.Negate:
                return "-@";
            case ExpressionType.UnaryPlus:
                return "+@";
            case ExpressionType.LeftShift:
                return "<<";
            case ExpressionType.RightShift:
                return ">>";
            case ExpressionType.ExclusiveOr:
                return "^";
            case ExpressionType.OnesComplement:
                return "~";
            case ExpressionType.And:
                return "&";
            case ExpressionType.Or:
                return "|";
            case ExpressionType.Power:
                return "**";
            }

            return null;
        }

        private class TrueOnce : DragonExpression
        {
            private bool _return;

            public TrueOnce()
            {
                _return = true;
            }

            public override Type Type => typeof(bool);

            public override string ToString()
            {
                return string.Format("[TrueOnce: Type={0}]", Type);
            }

            public override Expression Reduce()
            {
                if (_return)
                {
                    _return = false;
                    return Constant(true);
                }

                return Constant(false);
            }
        }

        #endregion
    }
}