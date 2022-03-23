// DragonNativeFunction.cs in EternityChronicles/IronDragon
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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using IronDragon.Expressions;
using IronDragon.Parser;
using BlockExpression = IronDragon.Expressions.BlockExpression;

// <copyright file="DragonPartialFunction.cs" Company="Michael Tindal">
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
    public partial class DragonNativeFunction : DragonFunction
    {
        private static readonly Dictionary<MethodBase, List<FunctionArgument>> ArgumentCache =
            new();

        public DragonNativeFunction(Type target, MethodBase method)
            : base(
                   GetExportName(method) ?? (method.IsConstructor ? "new" : method.Name), GenerateArguments(method),
                   GenerateBody(target, method), new DragonScope())
        {
            Func<MethodBase, bool> chkDoNotExportMethod = t =>
                                                          {
                                                              var a = t
                                                                      .GetCustomAttributes(typeof(DragonDoNotExportAttribute),
                                                                          false).FirstOrDefault();
                                                              return a != null;
                                                          };
            if (chkDoNotExportMethod(method))
            {
                Name = "<__doNotExport>";
                return;
            }

            Target = target;
            Method = method;
            NumberOfArguments = method.GetParameters().Count();
        }

        public Type Target { get; }

        public MethodBase Method { get; }

        public int NumberOfArguments { get; }

        public static List<FunctionArgument> GenerateArguments(MethodBase method)
        {
            if (ArgumentCache.ContainsKey(method)) return ArgumentCache[method];
            var args = new List<FunctionArgument>();
            method.GetParameters().ToList().ForEach(p =>
                                                    {
                                                        var arg = new FunctionArgument(p.Name);
                                                        if (p.GetCustomAttributes(typeof(ParamArrayAttribute), false)
                                                             .Any()) arg.IsVarArg = true;
                                                        if (p.DefaultValue != null &&
                                                            p.DefaultValue.GetType() != typeof(DBNull))
                                                        {
                                                            arg.HasDefault = true;
                                                            arg.DefaultValue = Expression.Constant(p.DefaultValue);
                                                        }

                                                        args.Add(arg);
                                                    });
            ArgumentCache[method] = args;
            return args;
        }

        public static BlockExpression GenerateBody(Type type, MethodBase method)
        {
            var body = new List<Expression>();
            body.Add(
                     DragonExpression.Invoke(
                                             Expression.Constant(method.IsConstructor ? typeof(DragonInstance) : type,
                                                                 typeof(Type)),
                                             Expression.Constant(method, typeof(MethodBase)), ArgumentCache[method]));
            body.Add(Expression.Label(DragonParser.ReturnTarget, Expression.Constant(null, typeof(object))));
            return DragonExpression.DragonBlock(body.ToArray());
        }

        public static string GetExportName(MethodBase t)
        {
            var a = t.GetCustomAttributes(typeof(DragonExportAttribute), false).FirstOrDefault();
            return a != null ? ((DragonExportAttribute)a).Name : null;
        }

        public override string ToString()
        {
            return string.Format("[DragonNativeFunction: TargetType={0}, Scope={1}]", Target, Scope);
        }
    }
}