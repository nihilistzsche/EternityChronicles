// FunctionConstraint.cs in EternityChronicles/EternityChronicles.Tests
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
    public class FunctionConstraint : Constraint
    {
        private object actual;

        internal FunctionConstraint(DragonFunction expected)
        {
            Expected = expected;
        }

        public DragonFunction Expected { get; }

        public override string Description
        {
            get => base.Description;

            protected set => base.Description = value;
        }

        private ConstraintResult Check()
        {
            var real = (DragonFunction)actual;
            var success = false;
            try
            {
                Assert.That(real.Name, Is.EqualTo(Expected.Name));
                Assert.That(real.Arguments, Is.EqualTo(Expected.Arguments));
                Assert.That(real.Body.ToString().Substring(0, Expected.Body.ToString().Length).Replace("; {", "; }"),
                            Is.EqualTo(Expected.Body.ToString()));
                success = true;
            }
            catch (AssertionException)
            {
                Description = $"Expected {actual}, but got {Expected} instead";
                success = false;
            }

            return new ConstraintResult(this, actual, success);
        }

        public override ConstraintResult ApplyTo<TActual>(TActual actual)
        {
            this.actual = actual;
            if (actual is not DragonFunction)
            {
                Description = $"Expected DragonFunction, but got {actual.GetType()} instead";
                return new ConstraintResult(this, actual, false);
            }

            return Check();
        }
    }
}