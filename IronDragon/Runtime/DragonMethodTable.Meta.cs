// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;

// <copyright file="DragonNativeFunction.Meta.cs" Company="Michael Tindal">
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

namespace IronDragon.Runtime
{
    /// <summary>
    ///     TODO: Update summary.
    /// </summary>
    public partial class DragonMethodTable : IDragonDynamicMetaObjectProvider
    {
        public new DynamicMetaObject /*!*/ GetMetaObject(Expression /*!*/parameter)
        {
            var m = new Meta(parameter, BindingRestrictions.Empty, this);
            m.SetScope(Scope);
            return m;
        }

        internal new sealed class Meta : DragonMetaObject<DragonMethodTable>
        {
            public Meta(Expression expression, BindingRestrictions restrictions, DragonMethodTable value)
                : base(expression, restrictions, value)
            {
            }

            private static bool DragonScopeFound { get; set; }

            private static object Arg(object val)
            {
                if (val is DragonScope)
                {
                    DragonScopeFound = true;
                    return val;
                }

                return val as FunctionArgument ?? new FunctionArgument(null, Expression.Constant(val));
            }

            public override DynamicMetaObject BindInvoke(InvokeBinder binder, DynamicMetaObject[] args)
            {
                var realArgs = new List<object>();
                args.ToList().ForEach(arg => realArgs.Add(Arg(arg.Value)));
                if (DragonScopeFound) realArgs.RemoveAt(0);
                DragonScopeFound = false;
                var _args = realArgs.ConvertAll(arg => (FunctionArgument)arg);
                var func  = Value.Resolve(_args);
                if (func == null)
                    return new DynamicMetaObject(Expression.Constant(null),
                    BindingRestrictions.GetExpressionRestriction(Expression.Constant(true)));
                return func.GetMetaObject(Expression).BindInvoke(binder, args);
            }
        }
    }
}