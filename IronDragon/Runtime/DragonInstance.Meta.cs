// DragonInstance.Meta.cs in EternityChronicles/IronDragon
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