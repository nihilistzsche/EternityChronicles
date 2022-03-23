// ForInExpression.cs in EternityChronicles/IronDragon
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