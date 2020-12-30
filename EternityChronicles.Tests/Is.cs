// -----------------------------------------------------------------------
// <copyright file="Is.cs" company="Microsoft">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using IronDragon.Runtime;
using NUnit.Framework.Constraints;
using EternityChronicles.Tests.IronDragon;

namespace EternityChronicles.Tests {
    /// <summary>
    ///     TODO: Update summary.
    /// </summary>
    public class Is : NUnit.Framework.Is {
        public static FunctionConstraint Function(DragonFunction expected) {
            return new(expected);
        }
    }

    public static class ConstraintExpressionExtensions {
        public static FunctionConstraint Function(this ConstraintExpression expr, DragonFunction expected) {
            return Is.Function(expected);
        }
    }
}