// -----------------------------------------------------------------------
// <copyright file="DragonClass.Meta.cs" Company="Michael Tindal">
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

using System.Dynamic;
using System.Linq.Expressions;

namespace IronDragon.Runtime
{
    /// <summary>
    ///     TODO: Update summary.
    /// </summary>
    public partial class DragonClass : IDragonDynamicMetaObjectProvider
    {
        public DynamicMetaObject GetMetaObject(Expression /*!*/parameter)
        {
            var m = new Meta(parameter, BindingRestrictions.Empty, this);
            m.SetScope(Scope);
            return m;
        }

        internal sealed class Meta : DragonMetaObject<DragonClass>
        {
            public Meta(Expression expression, BindingRestrictions restrictions, DragonClass value)
                : base(expression, restrictions, value)
            {
            }

            public override DynamicMetaObject BindCreateInstance(CreateInstanceBinder binder, DynamicMetaObject[] args)
            {
                return
                    InteropBinder.InvokeMember.Bind(
                                                    new InteropBinder.InvokeMember("new", binder.CallInfo, Scope), this,
                                                    args);
            }

            public override DynamicMetaObject BindInvoke(InvokeBinder binder, DynamicMetaObject[] args)
            {
                return InteropBinder.InvokeMember.Bind(new InteropBinder.InvokeMember("new", binder.CallInfo, Scope),
                                                       this, args);
            }

            public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder,
                                                               params DynamicMetaObject[] args)
            {
                return
                    InteropBinder.InvokeMember.Bind(
                                                    new InteropBinder.InvokeMember(binder.Name, binder.CallInfo, Scope),
                                                    this, args);
            }

            public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
            {
                return InteropBinder.GetMember.Bind(new InteropBinder.GetMember(binder.Name, Scope), this);
            }

            public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value)
            {
                return InteropBinder.SetMember.Bind(new InteropBinder.SetMember(binder.Name, Scope), this, value);
            }
        }

        #region IScopeExpression implementation

        public void SetScope(DragonScope scope)
        {
            Scope = scope;
        }

        public DragonScope Scope { get; private set; }

        #endregion
    }
}