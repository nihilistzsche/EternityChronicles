// -----------------------------------------------------------------------
// <copyright file="AccessTests.cs" Company="Michael Tindal">
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
            var real   = CompileAndExecute("x = [1,2,3]; v=x[1]--;[v, x[1]];");
            Assert.That(real, Is.EqualTo(expect));
        }

        [Test]
        public void TestAccessPostIncrement()
        {
            var expect = new DragonArray { 2, 3 };
            var real   = CompileAndExecute("x = [1,2,3]; v=x[1]++;[v, x[1]];");
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
                SD(new Dictionary<object, object>
                   { { XS("hello"), "world" }, { XS("john"), "doe" }, { XS("jack"), "black" } });

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
            var expect = SD(new Dictionary<object, object> { { "hello", "world" } });

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