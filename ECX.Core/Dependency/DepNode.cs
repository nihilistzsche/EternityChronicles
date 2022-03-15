//
// DepNode.cs
//
// Author:
//     Michael Tindal <mj.tindal@icloud.com>
//
// Copyright (C) 2005-2013 Michael Tindal and the individuals listed on
// the ChangeLog entries.
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
//

using System.Collections.Generic;
using System.Text;

namespace ECX.Core.Dependency
{
    /// <summary>
    ///     Represents a node in the dependency tree.
    /// </summary>
    /// <remarks>None.</remarks>
    public class DepNode
    {
        /// <summary>
        ///     Creates a new DepNode object with the given operator and constraint.
        /// </summary>
        /// <param name="op">The operator being used.</param>
        /// <param name="constraint">The constraint for this node.</param>
        /// <remarks>None.</remarks>
        public DepNode(DepOps op, DepConstraint constraint)
        {
            Parent     = null;
            Children   = new List<DepNode>();
            DepOp      = op;
            Constraint = constraint;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DepNode" /> class.
        /// </summary>
        /// <param name="op">The operator being used.</param>
        public DepNode(DepOps op) : this(op, null)
        {
        }

        /// <summary>
        ///     Gets this nodes parent node.
        /// </summary>
        /// <remarks>None.</remarks>
        public DepNode Parent { get; set; }

        /// <summary>
        ///     Gets the children of this node.
        /// </summary>
        /// <remarks>None.</remarks>
        public List<DepNode> Children { get; }

        /// <summary>
        ///     Gets or sets the operator for this node.
        /// </summary>
        /// <remarks>None.</remarks>
        public DepOps DepOp { get; set; }

        /// <summary>
        ///     Gets or sets the constraint for this node.
        /// </summary>
        /// <remarks>None.</remarks>
        public DepConstraint Constraint { get; set; }

        /// <summary>
        ///     Adds the child to list of this nodes children and sets the child node's parent to this
        ///     <see cref="DepNode" />.
        /// </summary>
        /// <param name="child">The child to add.</param>
        public void AddChild(DepNode child)
        {
            child.Parent = this;
            Children.Add(child);
        }

        private string GenDepthString(int depth)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < depth; i++) sb.Append(" ");

            return sb.ToString();
        }

        /// <summary>
        ///     Converts a given dependency operator to a string.
        /// </summary>
        /// <returns>The string represeting the operator.</returns>
        /// <param name="_op">The operator.</param>
        protected string OpToString(DepOps op)
        {
            return op switch
            {
                DepOps.And              => "&&",
                DepOps.Equal            => "==",
                DepOps.GreaterThan      => ">>",
                DepOps.GreaterThanEqual => ">=",
                DepOps.LessThan         => "<<",
                DepOps.LessThanEqual    => "<=",
                DepOps.Loaded           => "##",
                DepOps.NotLoaded        => "!#",
                DepOps.NotEqual         => "!=",
                DepOps.Opt              => "??",
                DepOps.Or               => "||",
                DepOps.Xor              => "^^",
                _                       => ""
            };
        }

        /// <summary>
        ///     Returns a string with the given depth.
        /// </summary>
        /// <returns>The string.</returns>
        /// <param name="depth">Depth.</param>
        public string ToString(int depth)
        {
            var sb = new StringBuilder(GenDepthString(depth));
            sb.Append(OpToString(DepOp));
            if (Constraint != null) sb.AppendFormat(" {0}", Constraint);
            sb.AppendLine();
            Children.ForEach(node =>
            {
                sb.AppendFormat(node.ToString(depth + 2));
                sb.AppendLine();
            });
            return sb.ToString();
        }

        /// <summary>
        ///     Returns a <see cref="System.String" /> that represents the current <see cref="DepNode" />.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents the current <see cref="DepNode" />.</returns>
        public override string ToString()
        {
            return ToString(0);
        }
    }
}