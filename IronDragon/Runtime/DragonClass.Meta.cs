// DragonClass.Meta.cs in EternityChronicles/IronDragon
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