// DepNode.cs in EternityChronicles/ECX.Core
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
            Parent = null;
            Children = new List<DepNode>();
            DepOp = op;
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
                       DepOps.And => "&&",
                       DepOps.Equal => "==",
                       DepOps.GreaterThan => ">>",
                       DepOps.GreaterThanEqual => ">=",
                       DepOps.LessThan => "<<",
                       DepOps.LessThanEqual => "<=",
                       DepOps.Loaded => "##",
                       DepOps.NotLoaded => "!#",
                       DepOps.NotEqual => "!=",
                       DepOps.Opt => "??",
                       DepOps.Or => "||",
                       DepOps.Xor => "^^",
                       _ => ""
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