// CompilerServices.cs in EternityChronicles/IronDragon
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
using IronDragon.Expressions;

namespace IronDragon.Runtime
{
    /// <summary>
    ///     TODO: Update summary.
    /// </summary>
    public class CompilerServices
    {
        /// <summary>
        ///     Creates a callable dynamic block object from a given expression.
        /// </summary>
        /// <param name="body">The body of the block to turn into a callable block.</param>
        /// <returns>A callable block with return type and parameters collected by analyzing the given body.</returns>
        internal static dynamic CreateLambdaForExpression(Expression body)
        {
            var func = typeof(Func<object>).GetGenericTypeDefinition();
            if (body.Type == typeof(void))
                func = func.MakeGenericType(typeof(object));
            else
                func = func.MakeGenericType(body.Type);
            var lambda = (from m in typeof(Expression).GetMethods()
                          where m.Name == "Lambda"
                          where m.GetParameters()[0].ParameterType == typeof(Expression)
                          where m.GetParameters()[1].ParameterType == typeof(ParameterExpression[])
                          select m).First();

            // Collect parameters here by analyzing the tree using a custom visitor pattern to see which
            // variables are assigned (without a set statement) or used in a block.
            lambda = lambda.MakeGenericMethod(func);

            Expression realBody;
            if (body.Type == typeof(void))
            {
                var xbody = new List<Expression>();
                xbody.Add(body);
                xbody.Add(Expression.Constant(null, typeof(object)));
                realBody = Expression.Block(xbody);
            }
            else
            {
                realBody = body;
            }

            dynamic block =
                ((LambdaExpression)
                    lambda.Invoke(typeof(Expression), new object[] { realBody, new ParameterExpression[] { } }))
                .Compile();
            return block;
        }

        internal static dynamic CompileExpression(Expression e, DragonScope scope)
        {
            Expression newExpression = DragonExpression.Convert(e, typeof(object));
            newExpression.SetScope(scope);
            var l = CreateLambdaForExpression(newExpression);
            return l();
        }
    }
}