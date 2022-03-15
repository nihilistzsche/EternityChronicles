// -----------------------------------------------------------------------
// <copyright file="Is.cs" company="Microsoft">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using IronDragon.Runtime;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace EternityChronicles.Tests.IronDragon
{
    /// <summary>
    ///     TODO: Update summary.
    /// </summary>
    public class DragonIs : Is
    {
        public static FunctionConstraint Function(DragonFunction expected)
        {
            return new FunctionConstraint(expected);
        }
    }

    public static class ConstraintExpressionExtensions
    {
        public static FunctionConstraint Function(this ConstraintExpression expr, DragonFunction expected)
        {
            return DragonIs.Function(expected);
        }
    }
}