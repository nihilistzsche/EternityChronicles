// -----------------------------------------------------------------------
// <copyright file="BlockExpression.cs" Company="Michael Tindal">
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
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using IronDragon.Runtime;

namespace IronDragon.Expressions {
    /// <summary>
    ///     TODO: Update summary.
    /// </summary>
    public class BlockExpression : DragonExpression {
        internal BlockExpression(List<Expression> body, DragonScope scope) {
            body.RemoveAll(e => e == null);

            Body = body;
            SetScope(scope);
        }

        internal BlockExpression(List<Expression> body) {
            Body = body;
        }

        public List<Expression> Body { get; internal set; }

        public override Type Type => Body.Last().Type;

        public override void SetChildrenScopes(DragonScope scope) {
            foreach (var expr in Body) {
                expr.SetScope(scope);
            }
        }

        public override Expression Reduce() {
            return Block(Body);
        }

        public override string ToString() {
            var str = new StringBuilder("{ ");
            foreach (var e in Body) {
                str.Append(e);
                str.Append("; ");
            }
            str.Append("}");
            return str.ToString();
        }
    }
}