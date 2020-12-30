// -----------------------------------------------------------------------
// <copyright file="FunctionConstraint.cs" company="Microsoft">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using IronDragon.Runtime;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace EternityChronicles.Tests.IronDragon {
    /// <summary>
    ///     TODO: Update summary.
    /// </summary>
    public class FunctionConstraint : Constraint {
        private object actual;

        internal FunctionConstraint(DragonFunction expected) {
            Expected = expected;
        }

        public DragonFunction Expected { get; }

        private ConstraintResult Check() {
            var real = (DragonFunction) actual;
            var success = false;
            try {
                Assert.That(real.Name, NUnit.Framework.Is.EqualTo(Expected.Name));
                Assert.That(real.Arguments, NUnit.Framework.Is.EqualTo(Expected.Arguments));
                Assert.That(real.Body.ToString().Substring(0, Expected.Body.ToString().Length).Replace("; {", "; }"),
                    NUnit.Framework.Is.EqualTo(Expected.Body.ToString()));
                success = true;
            }
            catch (AssertionException) {
                Description = $"Expected {actual}, but got {Expected} instead";
                success = false;
            }
            return new ConstraintResult(this, actual, success);
        }

        public override ConstraintResult ApplyTo<TActual>(TActual actual)
        {
            this.actual = actual;
            if (!(actual is DragonFunction)) {
                Description = $"Expected DragonFunction, but got {actual.GetType()} instead";
                return new ConstraintResult(this, actual, false);
            }
            return Check();
        }

        public override string Description
        {
            get => base.Description;

            protected set => base.Description = value;
        }
    }
}