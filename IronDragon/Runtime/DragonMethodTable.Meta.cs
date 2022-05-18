// DragonMethodTable.Meta.cs in EternityChronicles/IronDragon
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

            public override DynamicMetaObject BindInvoke(InvokeBinder binder, DynamicMetaObject[] rawArgs)
            {
                var realArgs = new List<object>();
                rawArgs.ToList().ForEach(arg => realArgs.Add(Arg(arg.Value)));
                if (DragonScopeFound) realArgs.RemoveAt(0);
                DragonScopeFound = false;
                var args = realArgs.ConvertAll(arg => (FunctionArgument)arg);
                var func = Value.Resolve(args);
                if (func == null)
                    return new DynamicMetaObject(Expression.Constant(null),
                                                 BindingRestrictions
                                                     .GetExpressionRestriction(Expression.Constant(true)));
                return func.GetMetaObject(Expression).BindInvoke(binder, rawArgs);
            }
        }
    }
}