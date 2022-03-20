// ForInExpression.cs in EternityChronicles/IronDragon
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using IronDragon.Parser;
using IronDragon.Runtime;

namespace IronDragon.Expressions
{
    /// <summary>
    ///     Represents a foreach (using for/in syntax) statement in Dragon.
    /// </summary>
    public class ForInExpression : DragonExpression
    {
        internal ForInExpression(string variableName, Expression enumerator, Expression body)
        {
            VariableName = variableName;
            Enumerator = enumerator;
            Body = body;
        }

        public string VariableName { get; }

        public Expression Enumerator { get; }

        public Expression Body { get; }

        public override Type Type => Body.Type;

        public override Expression Reduce()
        {
            var forInLabel = Label("<dragon_for_in>");
            ParameterExpression forInReturn = null;
            var useReturn = true;
            if (Body.Type == typeof(void))
                useReturn = false;
            else
                forInReturn = Variable(Body.Type, "<dragon_for_in_return>");
            var forInTest = Variable(typeof(bool), "<dragon_for_in_test>");
            var forInCurrent = Variable(Constant(VariableName));
            var forInCurrentLh = LeftHandValue(forInCurrent);
            var forInEnumerator = Variable(typeof(IEnumerator), "<dragon_for_in_enumerator>");
            var realBody = new List<Expression>
                           {
                               Assign(forInEnumerator,
                                      Call(Convert(Enumerator, typeof(IEnumerable)),
                                           typeof(IEnumerable).GetMethod("GetEnumerator"))),
                               Label(forInLabel),
                               Label(DragonParser.ContinueTarget),
                               Assign(forInTest, Call(forInEnumerator, typeof(IEnumerator).GetMethod("MoveNext")))
                           };
            var currentAssign = Assign(forInCurrentLh,
                                       Call(forInEnumerator,
                                            typeof(IEnumerator).GetMethod("get_Current")));
            currentAssign.SetScope(((DragonExpression)Body).Scope);
            realBody.Add(IfThen(forInTest, currentAssign));
            realBody.Add(Label(DragonParser.RetryTarget));
            realBody.Add(useReturn ? IfThen(forInTest, Assign(forInReturn, Body)) : IfThen(forInTest, Body));
            realBody.Add(IfThen(forInTest, Goto(forInLabel)));
            realBody.Add(Label(DragonParser.BreakTarget));
            if (useReturn)
            {
                realBody.Add(Convert(forInReturn, Body.Type));

                return Block(new[]
                             {
                                 forInTest,
                                 forInEnumerator,
                                 forInReturn
                             }, realBody);
            }

            return Block(new[]
                         {
                             forInTest,
                             forInEnumerator
                         }, realBody);
        }

        public override string ToString()
        {
            return "";
        }

        public override void SetChildrenScopes(DragonScope scope)
        {
            Enumerator.SetScope(scope);
            Body.SetScope(scope);
        }
    }
}