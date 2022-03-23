// SingletonDefinitionExpression.cs in EternityChronicles/IronDragon
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
using System.Linq.Expressions;
using IronDragon.Parser;
using IronDragon.Runtime;

namespace IronDragon.Expressions
{
    public class SingletonDefinitionExpression : FunctionDefinitionExpression
    {
        public SingletonDefinitionExpression(Expression singleton, string name, List<FunctionArgument> arguments,
                                             Expression body) : base(name, arguments, body)
        {
            Singleton = singleton;
        }

        public Expression Singleton { get; }

        public override Expression Reduce()
        {
            var ci = 0;
            Arguments.ForEach(arg => arg.Index = ci++);
            var realBody = new List<Expression>(((BlockExpression)Body).Body);
            if (Name == "new")
                realBody.Add(Return(new List<FunctionArgument> { new(null, Variable(Constant("self"))) }));
            realBody.Add(Label(DragonParser.ReturnTarget, Constant(null, typeof(object))));

            return Operation.SingletonDefine(typeof(DragonFunction), Constant(Singleton), Constant(Name),
                                             Constant(Arguments),
                                             Constant(DragonParser.CreateBlock(realBody)),
                                             Constant(Scope));
        }
    }
}