// DragonInstance.Meta.cs
// 
// Copyright (C) 2022 Michael Tindal (nihilistzsche AT gmail DOT com)
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//    http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;

namespace IronDragon.Runtime
{
    /// <summary>
    ///     TODO: Update summary.
    /// </summary>
    public partial class DragonInstance : IDragonDynamicMetaObjectProvider
    {
        public DynamicMetaObject /*!*/ GetMetaObject(Expression /*!*/parameter)
        {
            var m = new Meta(parameter, BindingRestrictions.Empty, this);
            m.SetScope(Scope);
            return m;
        }

        internal sealed class Meta : DragonMetaObject<DragonInstance>
        {
            public Meta(Expression expression, BindingRestrictions restrictions, DragonInstance value)
                : base(expression, restrictions, value)
            {
            }

            private static FunctionArgument Arg(object val)
            {
                return val as FunctionArgument ?? new FunctionArgument(null, Expression.Constant(val));
            }

            private static DynamicMetaObject DMO(dynamic val)
            {
                return Create(val, System.Linq.Expressions.Expression.Constant(val));
            }

            private static List<FunctionArgument> L(params FunctionArgument[] args)
            {
                return new List<FunctionArgument>(args);
            }

            private static DynamicMetaObject[] _DMO(params DynamicMetaObject[] args)
            {
                return args;
            }

            public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder,
                                                               params DynamicMetaObject[] args)
            {
                if (!(Value is DragonBoxedInstance) && Value.BackingObject != null)
                    DragonBoxedInstance.SyncInstanceVariablesFrom(Value, Value.BackingObject);
                var dmo =
                    InteropBinder.InvokeMember.Bind(
                                                    new InteropBinder.InvokeMember(binder.Name, binder.CallInfo, Scope),
                                                    this, args);
                if (!(Value is DragonBoxedInstance) && Value.BackingObject != null)
                    DragonBoxedInstance.SyncInstanceVariablesTo(Value, Value.BackingObject);
                return dmo;
            }

            public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
            {
                if (!(Value is DragonBoxedInstance) && Value.BackingObject != null)
                    DragonBoxedInstance.SyncInstanceVariablesFrom(Value, Value.BackingObject);
                var dmo = InteropBinder.GetMember.Bind(new InteropBinder.GetMember(binder.Name, Scope),
                                                       this);
                if (!(Value is DragonBoxedInstance) && Value.BackingObject != null)
                    DragonBoxedInstance.SyncInstanceVariablesTo(Value, Value.BackingObject);
                return dmo;
            }

            public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value)
            {
                if (!(Value is DragonBoxedInstance) && Value.BackingObject != null)
                    DragonBoxedInstance.SyncInstanceVariablesFrom(Value, Value.BackingObject);
                var dmo = InteropBinder.SetMember.Bind(new InteropBinder.SetMember(binder.Name, Scope),
                                                       this, value);
                if (!(Value is DragonBoxedInstance) && Value.BackingObject != null)
                    DragonBoxedInstance.SyncInstanceVariablesTo(Value, Value.BackingObject);
                return dmo;
            }
        }

        #region IScopeExpression implementation

        public DragonScope Scope { get; private set; }

        public void SetScope(DragonScope scope)
        {
            Scope = scope;
        }

        #endregion
    }
}