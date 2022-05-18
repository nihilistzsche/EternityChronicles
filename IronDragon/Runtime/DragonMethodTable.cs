// DragonMethodTable.cs in EternityChronicles/IronDragon
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
using System.Linq;
using System.Text;
using IronDragon.Builtins;

// <copyright file="DragonMethodTable.cs" Company="Michael Tindal">
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
    ///     Table used for overloading methods.
    /// </summary>
    public partial class DragonMethodTable : DragonFunction
    {
        public DragonMethodTable(string name) : base(name, null, null, null)
        {
            Functions = new List<DragonFunction>();
        }

        internal List<DragonFunction> Functions { get; }

        public void AddFunction(DragonFunction function)
        {
            DragonFunction funcToRemove = null;
            Functions.ForEach(func =>
                              {
                                  if (func.Name.Equals(function.Name) &&
                                      func.Arguments.Count == function.Arguments.Count &&
                                      !(function is DragonNativeFunction)) funcToRemove = func;
                              });
            if (funcToRemove != null)
                Functions.Remove(funcToRemove);
            Functions.Add(function);
        }

        internal DragonFunction Resolve(List<FunctionArgument> args, bool exactMatch = false)
        {
            if (!Functions.Any()) return null;
            if (Functions.Count == 1 && !exactMatch) return Functions[0];
            var q = Functions.Where(f => f.Arguments.Count == args.Count);
            IEnumerable<DragonFunction> fq;
            IEnumerable<DragonFunction> rq;
            if (q.Any())
            {
                if (q.Count() == 1) return q.First();
                var nq = Functions.Where(f => CheckForNameMatch(f, args));
                if (nq.Any()) return nq.First();
                fq = Functions.Where(f => f is DragonNativeFunction)
                              .Where(f => CheckForMatch(f as DragonNativeFunction, args));
                if (fq.Any())
                {
                    var gq = fq.Where(f => CheckForNameMatch(f, args));
                    if (gq.Any()) return gq.First();
                    return fq.First();
                }

                rq = Functions.Where(f => CheckForMatch(f, args));
                if (rq.Any())
                {
                    var sq = rq.Where(f => CheckForNameMatch(f, args));
                    if (sq.Any()) return sq.First();
                    return rq.First();
                }

                return null;
            }

            fq = Functions.Where(f => f is DragonNativeFunction)
                          .Where(f => CheckForMatch(f as DragonNativeFunction, args));
            if (fq.Any())
            {
                var gq = fq.Where(f => CheckForNameMatch(f, args));
                if (gq.Any()) return gq.First();
                return fq.First();
            }

            rq = Functions.Where(f => CheckForMatch(f, args));
            if (rq.Any())
            {
                var sq = rq.Where(f => CheckForNameMatch(f, args));
                if (sq.Any()) return sq.First();
                return rq.First();
            }

            return null;
        }

        private bool CheckForNameMatch(DragonFunction function, List<FunctionArgument> args)
        {
            var match = false;
            for (var i = 0; i < args.Count; i++)
            {
                if (args[i].Name != null)
                {
                    var i1 = i;
                    var nameMatch = function.Arguments.Where(arg => arg.Name == args[i1].Name);
                    if (!nameMatch.Any()) return false;
                    match = true;
                }
            }

            return match;
        }

        private bool CheckForMatch(DragonFunction function, List<FunctionArgument> args)
        {
            if (args.Count == function.Arguments.Count) return true;
            if (args.Count > function.Arguments.Count)
                return function.Arguments.Any() && function.Arguments.Last().IsVarArg;
            var myCount = args.Count;
            var theirCount = function.Arguments.Count;
            function.Arguments.ForEach(arg =>
                                       {
                                           if (arg.HasDefault) theirCount--;
                                       });
            var vo = 0;
            if (function.Arguments.Any() && function.Arguments.Last().IsVarArg) vo = 1;
            if (myCount == theirCount) return true;
            if (myCount + vo == theirCount) return true;
            return false;
        }

        private bool CheckForMatch(DragonNativeFunction function, List<FunctionArgument> args)
        {
            if (function.Arguments.Count == args.Count)
            {
                var innerArgs = new List<object>();
                args.ForEach(arg =>
                             {
                                 var val = CompilerServices.CreateLambdaForExpression(arg.Value)();
                                 if (val is DragonString) val = (string)val;
                                 if (val is DragonNumber) val = DragonNumber.Convert((DragonNumber)val);
                                 innerArgs.Add(val);
                             });
                var match = true;
                var i = 0;
                foreach (var param in function.Method.GetParameters())
                {
                    if (innerArgs[i++].GetType() != param.ParameterType)
                    {
                        match = false;
                        break;
                    }
                }

                return match;
            }

            if (args.Count > function.Arguments.Count)
            {
                if (function.Arguments.Any() && function.Arguments.Last().IsVarArg) return true;
                return false;
            }

            var myCount = args.Count;
            var theirCount = function.Arguments.Count;
            function.Arguments.ForEach(arg =>
                                       {
                                           if (arg.HasDefault) theirCount--;
                                       });
            var vo = 0;
            if (function.Arguments.Any() && function.Arguments.Last().IsVarArg) vo = 1;
            if (myCount == theirCount) return true;
            if (myCount + vo == theirCount) return true;
            return false;
        }

        public override string ToString()
        {
            var sb = new StringBuilder(string.Format("[DragonMethodTable]: {0} ({1})", Name, Functions.Count));
            sb.AppendLine();
            Functions.ForEach(func =>
                              {
                                  sb.AppendFormat("  {0}(", func.Name);
                                  if (func is DragonNativeFunction)
                                  {
                                      var snf = (DragonNativeFunction)func;
                                      var sep = "";
                                      foreach (var p in snf.Method.GetParameters())
                                      {
                                          sb.AppendFormat("{0}{1} {2}", sep, p.ParameterType.Name, p.Name);
                                          sep = ",";
                                      }
                                  }
                                  else
                                  {
                                      var sep = "";
                                      foreach (var p in func.Arguments)
                                      {
                                          sb.AppendFormat("{0}{1}", sep, p.Name);
                                          sep = ",";
                                      }
                                  }

                                  sb.AppendLine(")");
                              });
            return sb.ToString();
        }
    }
}