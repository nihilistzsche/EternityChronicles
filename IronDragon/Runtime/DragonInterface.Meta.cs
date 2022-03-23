// DragonInterface.Meta.cs in EternityChronicles/IronDragon
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
    public partial class DragonInterface : IDragonDynamicMetaObjectProvider
    {
        public new DynamicMetaObject GetMetaObject(Expression /*!*/parameter)
        {
            var m = new Meta(parameter, BindingRestrictions.Empty, this);
            m.SetScope(Scope);
            return m;
        }

        internal new sealed class Meta : DragonMetaObject<DragonInterface>
        {
            public Meta(Expression expression, BindingRestrictions restrictions, DragonInterface value)
                : base(expression, restrictions, value)
            {
            }

            public override DynamicMetaObject BindCreateInstance(CreateInstanceBinder binder, DynamicMetaObject[] args)
            {
                return new DynamicMetaObject(Expression.Constant(null),
                                             BindingRestrictions.GetExpressionRestriction(Expression.Constant(true)));
            }

            public override DynamicMetaObject BindInvoke(InvokeBinder binder, DynamicMetaObject[] args)
            {
                return new DynamicMetaObject(Expression.Constant(null),
                                             BindingRestrictions.GetExpressionRestriction(Expression.Constant(true)));
            }

            public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder,
                                                               params DynamicMetaObject[] args)
            {
                return new DynamicMetaObject(Expression.Constant(null),
                                             BindingRestrictions.GetExpressionRestriction(Expression.Constant(true)));
            }

            public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
            {
                return new DynamicMetaObject(Expression.Constant(null),
                                             BindingRestrictions.GetExpressionRestriction(Expression.Constant(true)));
            }

            public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value)
            {
                return new DynamicMetaObject(Expression.Constant(null),
                                             BindingRestrictions.GetExpressionRestriction(Expression.Constant(true)));
            }
        }

        #region IScopeExpression implementation

        public new void SetScope(DragonScope scope)
        {
            Scope = scope;
        }

        public new DragonScope Scope { get; private set; }

        #endregion
    }
}