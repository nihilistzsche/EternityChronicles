// -----------------------------------------------------------------------
// <copyright file="Forcs" Company="Michael Tindal">
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

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using IronDragon.Parser;
using IronDragon.Runtime;

namespace IronDragon.Expressions {
    /// <summary>
    ///     Represents a standard for loop in Dragon.
    /// </summary>
    public class ForExpression : DragonExpression {
        internal ForExpression(Expression init, Expression test, Expression step, Expression body) {
            Init = init;
            Test = test;
            Step = step;
            Body = body;
        }

        public Expression Init { get; }

        public Expression Test { get; }

        public Expression Step { get; }

        public Expression Body { get; }

        public override Type Type => Body.Type;

        public override Expression Reduce() {
            var forLabel = Label("<dragon_for>");
            VariableExpression forReturn = null;
            LeftHandValueExpression forReturnLh = null;
            var useReturn = true;
            if (Body.Type == typeof (void)) {
                useReturn = false;
            }
            else {
                forReturn = Variable(Constant("<dragon_for_return>"));
                forReturnLh = LeftHandValue(forReturn);
                forReturn.Scope = ((DragonExpression) Body).Scope;
                forReturnLh.Scope = ((DragonExpression) Body).Scope;
            }
            var forTest = Variable(Constant("<dragon_for_test>"));
            var forTestLh = LeftHandValue(forTest);
            forTest.Scope = ((DragonExpression) Body).Scope;
            forTestLh.Scope = ((DragonExpression) Body).Scope;
            var realBody = new List<Expression> {
                Init,
                Label(forLabel),
            };
            var testAssign = Assign(forTestLh, Test);
            realBody.Add(Label(DragonParser.RetryTarget));
            testAssign.Scope = (Body as DragonExpression).Scope;
            realBody.Add(testAssign);
            IfExpression testIf;
            if (useReturn) {
                var returnAssign = Assign(forReturnLh, Body);
                returnAssign.Scope = (Body as DragonExpression).Scope;
                testIf = IfThen(forTest, returnAssign);
            }
            else {
                testIf = IfThen(forTest, Body);
            }
            testIf.Scope = ((DragonExpression) Body).Scope;
            realBody.Add(testIf);
            realBody.Add(Label(DragonParser.ContinueTarget));
            realBody.Add(Step);
            realBody.Add(IfThen(forTest, Goto(forLabel)));
            realBody.Add(Label(DragonParser.BreakTarget));
            if (useReturn) {
                realBody.Add(forReturn);
            }

            var block = new BlockExpression(realBody) {Scope = (Body as DragonExpression).Scope};
            return Convert(block, Type);
        }

        public override void SetChildrenScopes(DragonScope scope) {
            Body.SetScope(scope);
            Init.SetScope(((DragonExpression) Body).Scope);
            Test.SetScope(((DragonExpression) Body).Scope);
            Step.SetScope(((DragonExpression) Body).Scope);
        }

        public override string ToString() {
            var str = new StringBuilder("for (");
            str.AppendFormat("{0}; ", Init);
            str.AppendFormat("{0}; ", Test);
            str.AppendFormat("{0})", Step);
            str.Append(Body);
            return str.ToString();
        }
    }
}