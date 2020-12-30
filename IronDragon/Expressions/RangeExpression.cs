using System;
using System.Linq.Expressions;
using IronDragon.Runtime;

namespace IronDragon.Expressions {
	/// <summary>
	///     TODO: Update summary.
	/// </summary>
	public class RangeExpression : DragonExpression {
		internal RangeExpression (Expression start, Expression end, bool inclusive = false) {
			Start = start;
			End = end;
			Inclusive = inclusive;
		}

		public Expression Start { get; private set; }

		public Expression End { get; private set; }

		public bool Inclusive { get; private set; }

		public override Type Type {
			get { return typeof (object); }
		}

		// Should not actually reduce, used by the runtime directly
		public override Expression Reduce () {
			return Operation.Range (Type, Constant (Start), Constant (End), Constant (Inclusive));
		}

		public override void SetChildrenScopes (DragonScope scope) {
			Start.SetScope (scope);
			End.SetScope (scope);
		}
		public override string ToString () {
			return Start + (Inclusive ? "..." : "..") + End;
		}
	}
}