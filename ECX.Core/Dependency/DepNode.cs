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

namespace ECX.Core.Dependency
{
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Represents a node in the dependency tree.
    /// </summary>
    /// <remarks>None.</remarks>
    public class DepNode {
		private DepConstraint _constraint;
		private DepOps _op;
		private DepNode _parent;
		private List<DepNode> _children;

		/// <summary>
		/// Creates a new DepNode object with the given operator and constraint.
		/// </summary>
		/// <param name="op">The operator being used.</param>
		/// <param name="constraint">The constraint for this node.</param>
		/// <remarks>None.</remarks>
		public DepNode (DepOps op, DepConstraint constraint) {
			_parent = null;
			_children = new List<DepNode> ();
			_op = op;
			_constraint = constraint;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ECX.Dependency.Core.DepNode"/> class.
		/// </summary>
		/// <param name="op">The operator being used.</param>
		public DepNode(DepOps op) : this(op, null) {
		}

		/// <summary>
		/// Gets this nodes parent node.
		/// </summary>
		/// <remarks>None.</remarks>
		public DepNode Parent {
			get => _parent;
			set => _parent = value;
		}

		/// <summary>
		/// Gets the children of this node.
		/// </summary>
		/// <remarks>None.</remarks>
		public List<DepNode> Children => _children;

	    /// <summary>
		/// Adds the child to list of this nodes children and sets the child node's parent to this <see cref="ECX.Dependency.Core.DepNode"/>.
		/// </summary>
		/// <param name="child">The child to add.</param>
		public void AddChild(DepNode child) {
			child.Parent = this;
			_children.Add (child);
		}

		/// <summary>
		/// Gets or sets the operator for this node.
		/// </summary>
		/// <remarks>None.</remarks>
		public DepOps DepOp {
			get => _op;
			set => _op = value;
		}

		/// <summary>
		/// Gets or sets the constraint for this node.
		/// </summary>
		/// <remarks>None.</remarks>
		public DepConstraint Constraint {
			get => _constraint;
			set => _constraint = value;
		}

		private string GenDepthString(int depth) {
			var sb = new StringBuilder();
			for(int i = 0; i < depth; i++)
				sb.Append (" ");
			return sb.ToString();
		}

		/// <summary>
		/// Converts a given dependency operator to a string.
		/// </summary>
		/// <returns>The string represeting the operator.</returns>
		/// <param name="_op">The operator.</param>
		protected string OpToString (DepOps op) {
			switch (op)
			{
			case DepOps.And:
				return "&&";
			case DepOps.Equal:
				return "==";
			case DepOps.GreaterThan:
				return ">>";
			case DepOps.GreaterThanEqual:
				return ">=";
			case DepOps.LessThan:
				return "<<";
			case DepOps.LessThanEqual:
				return "<=";
			case DepOps.Loaded:
				return "##";
			case DepOps.NotLoaded:
				return "!#";
			case DepOps.NotEqual:
				return "!=";
			case DepOps.Opt:
				return "??";
			case DepOps.Or:
				return "||";
			case DepOps.Xor:
				return "^^";
			}
			
			return "";
		}

		/// <summary>
		/// Returns a string with the given depth.
		/// </summary>
		/// <returns>The string.</returns>
		/// <param name="depth">Depth.</param>
		public string ToString(int depth) {
			var sb = new StringBuilder(GenDepthString(depth));
			sb.Append (OpToString(DepOp));
			if(Constraint != null) {
				sb.AppendFormat(" {0}", Constraint);
			}
			sb.AppendLine();
			Children.ForEach(node => { sb.AppendFormat (node.ToString(depth+2)); sb.AppendLine(); });
			return sb.ToString();
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents the current <see cref="ECX.Dependency.Core.DepNode"/>.
		/// </summary>
		/// <returns>A <see cref="System.String"/> that represents the current <see cref="ECX.Dependency.Core.DepNode"/>.</returns>
		public override string ToString ()
		{
			return ToString (0);
		}
	}
}
