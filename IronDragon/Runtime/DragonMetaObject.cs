// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using IronDragon.Expressions;

// <copyright file="DragonMetaObject.cs" Company="Michael Tindal">
// Copyright 2011-2013 Michael Tindal
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
// -----------------------------------------------------------------------
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

namespace IronDragon.Runtime {
    public interface IDragonDynamicMetaObjectProvider : IDynamicMetaObjectProvider, IScopeExpression {}

    public abstract class DragonMetaObject : DynamicMetaObject, IScopeExpression {
        private DragonScope _scope;

        #region IScopeExpression implementation

        public void SetScope(DragonScope scope) {
            _scope = scope;
        }

        public DragonScope Scope => _scope;

        #endregion

        internal DragonMetaObject(Expression expression, BindingRestrictions restrictions, object value)
            : base(expression, restrictions, value) {}

        internal static BindingRestrictions GetRes() {
            return
                BindingRestrictions.GetExpressionRestriction(Expression.Equal(new TrueOnce(), Expression.Constant(true)));
        }

        public override DynamicMetaObject BindInvoke(InvokeBinder binder, DynamicMetaObject[] args) {
            return InteropBinder.Invoke.Bind(new InteropBinder.Invoke(Scope, binder.CallInfo), this, args);
        }

        public override DynamicMetaObject BindGetIndex(GetIndexBinder binder, DynamicMetaObject[] indexes) {
            return InteropBinder.GetIndex.Bind(new InteropBinder.GetIndex(Scope, binder.CallInfo), this, indexes);
        }

        public override DynamicMetaObject BindSetIndex(SetIndexBinder binder, DynamicMetaObject[] indexes,
            DynamicMetaObject value) {
            return InteropBinder.SetIndex.Bind(new InteropBinder.SetIndex(Scope, binder.CallInfo), this, indexes, value);
        }

        private static FunctionArgument Arg(object val) {
            return val as FunctionArgument ?? new FunctionArgument(null, Expression.Constant(val));
        }

        private static DynamicMetaObject DMO(dynamic val) {
            return Create(val, Expression.Constant(val));
        }

        private static List<FunctionArgument> L(params FunctionArgument[] args) {
            return new(args);
        }

        private static DynamicMetaObject[] _DMO(params DynamicMetaObject[] args) {
            return args;
        }

        public override DynamicMetaObject BindUnaryOperation(UnaryOperationBinder binder) {
            if (Value is DragonInstance) {
                // Check to see which method we should try to call
                var DragonName = InteropBinder.MapExpressionType(binder.Operation);

                if (DragonName == null) {
                    return InteropBinder.Unary.Bind(binder, this);
                }

                if (
                    InteropBinder.InvokeMember.SearchForFunction(((DragonInstance) Value).Class, DragonName,
                        (DragonInstance) Value, L(), true) != null) {
                    return
                        InteropBinder.InvokeMember.Bind(
                            new InteropBinder.InvokeMember(DragonName, new CallInfo(0), Scope), this, _DMO(DMO(Scope)));
                }
                var clrName = InteropBinder.ToClrOperatorName(DragonName);
                if (
                    InteropBinder.InvokeMember.SearchForFunction(((DragonInstance) Value).Class, clrName,
                        (DragonInstance) Value, L(), true) != null) {
                    return InteropBinder.Unary.Bind(binder, this);
                }

                return new DynamicMetaObject(Expression.Constant(null),
                    BindingRestrictions.GetExpressionRestriction(Expression.Constant(true)));
            }

            return InteropBinder.Unary.Bind(binder, this);
        }

        public override DynamicMetaObject BindBinaryOperation(BinaryOperationBinder binder, DynamicMetaObject arg) {
            if (Value is DragonInstance) {
                var DragonName = InteropBinder.MapExpressionType(binder.Operation);
                if (DragonName == null) {
                    return InteropBinder.Binary.Bind(binder, this, arg);
                }

                if (
                    InteropBinder.InvokeMember.SearchForFunction(((DragonInstance) Value).Class, DragonName,
                        (DragonInstance) Value, L(Arg(arg.Value)), true) != null) {
                    return
                        InteropBinder.InvokeMember.Bind(
                            new InteropBinder.InvokeMember(DragonName, new CallInfo(0), Scope), this,
                            _DMO(DMO(Scope), DMO(Arg(arg.Value))));
                }
                var clrName = InteropBinder.ToClrOperatorName(DragonName);
                if (
                    InteropBinder.InvokeMember.SearchForFunction(((DragonInstance) Value).Class, clrName,
                        (DragonInstance) Value, L(Arg(arg.Value)), true) != null) {
                    return
                        InteropBinder.InvokeMember.Bind(
                            new InteropBinder.InvokeMember(clrName, new CallInfo(0), Scope), this,
                            _DMO(DMO(Scope), DMO(Arg(arg.Value))));
                }
                return InteropBinder.Binary.Bind(binder, this, arg);
            }
            return InteropBinder.Binary.Bind(binder, this, arg);
        }

        private class TrueOnce : DragonExpression {
            private bool _return;

            public TrueOnce() {
                _return = true;
            }

            public override Type Type => typeof (bool);

            public override string ToString() {
                return string.Format("[TrueOnce: Type={0}]", Type);
            }

            public override Expression Reduce() {
                if (_return) {
                    _return = false;
                    return Constant(true);
                }
                return Constant(false);
            }
        }
    }

    public abstract class DragonMetaObject<T> : DragonMetaObject {
        protected DragonMetaObject(Expression expression, BindingRestrictions restrictions, T value)
            : base(expression, restrictions, value) {}

        public new T Value => (T) base.Value;
    }
}