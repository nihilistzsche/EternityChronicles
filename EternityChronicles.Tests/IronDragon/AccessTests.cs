// AccessTests.cs in EternityChronicles/EternityChronicles.Tests
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
using IronDragon.Builtins;
using NUnit.Framework;

namespace EternityChronicles.Tests.IronDragon
{
    [TestFixture]
    public class AccessTests : DragonAbstractTestFixture
    {
        [Test]
        public void TestAccessPostDecrement()
        {
            var expect = new DragonArray { 2, 1 };
            var real = CompileAndExecute("x = [1,2,3]; v=x[1]--;[v, x[1]];");
            Assert.That(real, Is.EqualTo(expect));
        }

        [Test]
        public void TestAccessPostIncrement()
        {
            var expect = new DragonArray { 2, 3 };
            var real = CompileAndExecute("x = [1,2,3]; v=x[1]++;[v, x[1]];");
            Assert.That(real, Is.EqualTo(expect));
        }

        [Test]
        public void TestAccessPreDecrement()
        {
            Assert.That(CompileAndExecute("x = [1,2,3]; --x[1];"), Is.EqualTo(1));
        }

        [Test]
        public void TestAccessPreIncrement()
        {
            Assert.That(CompileAndExecute("x = [1,2,3]; ++x[1];"), Is.EqualTo(3));
        }

        [Test]
        public void TestAnotherHash()
        {
            var expect =
                Sd(new Dictionary<object, object>
                   { { Xs("hello"), "world" }, { Xs("john"), "doe" }, { Xs("jack"), "black" } });

            var real = CompileAndExecute("{:hello => 'world',:john => 'doe',:jack => 'black'};");

            Assert.That(real, Is.EqualTo(expect));
        }

        [Test]
        public void TestArrayAccess()
        {
            Assert.That(CompileAndExecute("i = [1,2,3]; x = 1; i[x];"), Is.EqualTo(2));
        }

        [Test]
        public void TestArrayAccess2()
        {
            Assert.That(CompileAndExecute("i = [1,2,3]; z = 0; for(x = 0; x < 3; x += 1) { z += i[x]; };"),
                        Is.EqualTo(6));
        }

        [Test]
        public void TestArrayAssignment()
        {
            var expect = new DragonArray { 1, 6, 3 };

            var real = CompileAndExecute("x = [1, 2, 3]; x[1] = 6; x;");
            Assert.That(real, Is.EqualTo(expect));
        }

        [Test]
        public void TestArrayBinaryOp()
        {
            Assert.That(CompileAndExecute("a = [1,2]; a[1] += 2;"), Is.EqualTo(4));
        }

        [Test]
        public void TestArrayDynamicReassignment()
        {
            var expect = new DragonArray { 1, "hello", 3 };

            var real = CompileAndExecute("x = [1, 2, 3]; x[1] = 'hello'; x;");

            Assert.That(real, Is.EqualTo(expect));
        }

        [Test]
        public void TestDictionaryBinaryOp()
        {
            Assert.That(CompileAndExecute("a = { :t => 1, :x => 2 }; a[:t] *= 4;"), Is.EqualTo(4));
        }

        [Test]
        public void TestExpressionDictionary()
        {
            Assert.That(CompileAndExecute("x = 17; z = { x => 10 }; z[x];"), Is.EqualTo(10));
        }

        [Test]
        public void TestHash()
        {
            var expect = Sd(new Dictionary<object, object> { { "hello", "world" } });

            var real = CompileAndExecute("{ 'hello' => 'world' };");

            Assert.That(real, Is.EqualTo(expect));
        }

        [Test]
        public void TestHashSymbolDifferent()
        {
            var expect = new DragonArray { "world", "universe" };

            var real =
                CompileAndExecute("x = { :hello => 'world', 'hello' => 'universe' }; [x[:hello], x['hello']];");

            Assert.That(real, Is.EqualTo(expect));
        }

        [Test]
        public void TestNestedArray()
        {
            var expect = new DragonArray
                         {
                             new DragonArray { 1, 2, 3 },
                             new DragonArray { 4, 5, 6 },
                             new DragonArray { 7, 8, 9 }
                         };

            var real = CompileAndExecute("x = [[1,2,3],[4,5,6],[7,8,9]];");
            Assert.That(real, Is.EqualTo(expect));
        }

        [Test]
        public void TestNestedArrayAssignment()
        {
            var expect = new DragonArray
                         {
                             new DragonArray { 1, 6, 3 },
                             new DragonArray { 4, 10, 6 },
                             new DragonArray { 7, 14, 9 }
                         };

            var real =
                CompileAndExecute("x = [[1,2,3],[4,5,6],[7,8,9]]; x[0][1] = 6; x[1][1] = 10; x[2][1] = 14; x;");
            Assert.That(real, Is.EqualTo(expect));
        }

        [Test]
        public void TestSimpleArray()
        {
            var expect = new DragonArray { 1, "hello", 2.0 };

            var real = CompileAndExecute("x = [1, 'hello', 2.0];");
            Assert.That(real, Is.EqualTo(expect));
        }

        [Test]
        public void TestSimpleArrayTwo()
        {
            var expect = new DragonArray { 1, 2, 3 };

            var real = CompileAndExecute("x = [1, 2, 3];");

            Assert.That(real, Is.EqualTo(expect));
        }

        [Test]
        public void TestRangeAccess()
        {
            var expect = new DragonArray { 2, 3, 4 };

            var real = CompileAndExecute("x = [1, 2, 3, 4, 5]; x[1..4];");

            Assert.That(real, Is.EqualTo(expect));
        }

        [Test]
        public void TestInclusiveRangeAccess()
        {
            var expect = new DragonArray { 2, 3, 4, 5 };

            var real = CompileAndExecute("x = [1, 2, 3, 4, 5]; x[1...4];");

            Assert.That(real, Is.EqualTo(expect));
        }

        [Test]
        public void TestRangeVarAccess()
        {
            var expect = new DragonArray { 3, 5, 7 };

            var real = CompileAndExecute("s = 1; e = 4; r = s..e; x = [1, 3, 5, 7, 9]; x[r];");

            Assert.That(real, Is.EqualTo(expect));
        }
    }
}
