// FlowTests.cs in EternityChronicles/EternityChronicles.Tests
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

using NUnit.Framework;

namespace EternityChronicles.Tests.IronDragon
{
    [TestFixture]
    public class FlowTests : DragonAbstractTestFixture
    {
        [Test]
        public void TestBooleanNumberFalse()
        {
            Assert.That(CompileAndExecute("if(0) { true; } else { false; };"), Is.EqualTo(false));
        }

        [Test]
        public void TestBooleanNumberTrue()
        {
            Assert.That(CompileAndExecute("if(1) { true; };"), Is.EqualTo(true));
        }

        [Test]
        public void TestBooleanObjectFalse()
        {
            Assert.That(CompileAndExecute("if(nil) { true; } else { false; };"), Is.EqualTo(false));
        }

        [Test]
        public void TestBooleanObjectTrue()
        {
            Assert.That(CompileAndExecute("if('hello') { true; };"), Is.EqualTo(true));
        }

        [Test]
        public void TestBreak()
        {
            Assert.That(CompileAndExecute("i = 10; while((i -= 1) > 0) { break if(i == 5); }; i;"),
                        Is.EqualTo(5));
        }

        [Test]
        public void TestContinue()
        {
            Assert.That(
                        CompileAndExecute("x = 1; i = 0; while((i += 1) < 10) { continue if (i % 2) == 1; set x *= 2; };"),
                        Is.EqualTo(16));
        }

        [Test]
        public void TestDoUntil()
        {
            Assert.That(CompileAndExecute("i = 0; x = 0; y = 2; do { x = ((x + y) * 2); } until((i += 1) == 10);"),
                        Is.EqualTo(4092));
        }

        [Test]
        public void TestDoWhile()
        {
            Assert.That(CompileAndExecute("i = 0; x = 0; y = 2; do { x = ((x + y) * 2); } while((i += 1) < 10);"),
                        Is.EqualTo(4092));
        }

        [Test]
        public void TestElse()
        {
            Assert.That(CompileAndExecute("i = 10; if(i % 2 == 1) { i ** 2; } else { (i * 2)/4; };"),
                        Is.EqualTo(5));
        }

        [Test]
        public void TestFor()
        {
            Assert.That(CompileAndExecute("i = 0; for(x = 0; x < 10; x += 2) { i += 1; };"),
                        Is.EqualTo(5));
        }

        [Test]
        public void TestForContinue()
        {
            Assert.That(CompileAndExecute("z = 0; for(i = 0; i < 10; i += 1) { continue if (i % 2) == 1; z += 1; };"),
                        Is.EqualTo(5));
        }

        [Test]
        public void TestForIn()
        {
            Assert.That(CompileAndExecute("num = 0; for(i in [1,2,4]) { num += i; };"), Is.EqualTo(7));
        }

        [Test]
        public void TestIf()
        {
            Assert.That(CompileAndExecute("i = 9.0; if(i % 2 == 1) { i ** 2; };"), Is.EqualTo(81));
        }

        [Test]
        public void TestIfElse()
        {
            Assert.That(CompileAndExecute("i = 2; if(i >= 2) { i << 2; } else { i; };"), Is.EqualTo(8));
        }

        [Test]
        public void TestIfElseIf()
        {
            Assert.That(CompileAndExecute("if(2 < 1) { 7; } else if (3 > 2) { 14; };"), Is.EqualTo(14));
        }

        [Test]
        public void TestIfElseIf2()
        {
            Assert.That(CompileAndExecute("if(1 < 2) { 7; } else if (2 < 3) { 14; } else { 21; };"),
                        Is.EqualTo(7));
        }

        [Test]
        public void TestIfElseIfElse()
        {
            Assert.That(CompileAndExecute("if(2 < 1) { 7; } else if (2 > 3) { 14; } else { 21; };"),
                        Is.EqualTo(21));
        }

        [Test]
        public void TestIfLogicalAnd1()
        {
            Assert.That(CompileAndExecute("if((2 < 3) && (4 != 5)) { 7; } else { 9; };"), Is.EqualTo(7));
        }

        [Test]
        public void TestIfLogicalAnd2()
        {
            Assert.That(CompileAndExecute("if((2 < 1) && (4 != 4)) { 7; } else { 9; };"), Is.EqualTo(9));
        }

        [Test]
        public void TestIfLogicalNot()
        {
            Assert.That(CompileAndExecute("if(! false) { 7; };"), Is.EqualTo(7));
        }

        [Test]
        public void TestIfLogicalOr1()
        {
            Assert.That(CompileAndExecute("if((2 < 1) || (4 != 5)) { 7; } else { 9; };"), Is.EqualTo(7));
        }

        [Test]
        public void TestIfLogicalOr2()
        {
            Assert.That(CompileAndExecute("if((2 < 1) || (4 != 4)) { 7; } else { 9; };"), Is.EqualTo(9));
        }

        [Test]
        public void TestLoop()
        {
            Assert.That(CompileAndExecute("num = 0; loop { num += 1; break if num == 17; }; num;"),
                        Is.EqualTo(17));
        }

        [Test]
        public void TestRetry()
        {
            Assert.That(CompileAndExecute("num = 0; for(i = 0; i < 5; i += 1) { num += 1; retry if num == 2; }; num;"),
                        Is.EqualTo(6));
        }

        [Test]
        public void TestSwitch()
        {
            Assert.That(CompileAndExecute("i = 10; switch(i) { case 10: { 'case 10!'; } };"),
                        Is.EqualTo("case 10!"));
        }

        [Test]
        public void TestSwitch2CB1()
        {
            Assert.That(CompileAndExecute("i = 5; switch(i) { case 5: { 'case 5!'; } case 10: { 'case 10!'; } };"),
                        Is.EqualTo("case 5!"));
        }

        [Test]
        public void TestSwitch2CB2()
        {
            Assert.That(CompileAndExecute("i = 10; switch(i) { case 5: { 'case 5!'; } case 10: { 'case 10!'; } };"),
                        Is.EqualTo("case 10!"));
        }

        [Test]
        public void TestSwitch2CBD()
        {
            Assert.That(
                        CompileAndExecute(
                                          "i = 0; switch(i) { case 5: { 'case 5!'; } case 10: { 'case 10!'; } default: { 'default block!'; } };"),
                        Is.EqualTo("default block!"));
        }

        [Test]
        public void TestSwitchDefault()
        {
            Assert.That(
                        CompileAndExecute("i = 5; switch(i) { case 10: { 'case 10!'; } default: { 'default block!'; } };"),
                        Is.EqualTo("default block!"));
        }

        [Test]
        public void TestUnlessConstruct()
        {
            Assert.That(CompileAndExecute("unless(3 > 2) { x = 1; } else { x = 2; };"), Is.EqualTo(2));
        }

        [Test]
        public void TestUnlessConstruct2()
        {
            Assert.That(CompileAndExecute("unless(2 > 3) { x = 2; } else { x = 1; };"), Is.EqualTo(2));
        }

        [Test]
        public void TestUnlessExpression()
        {
            Assert.That(CompileAndExecute("x = 2 unless 2 > 3;"), Is.EqualTo(2));
        }

        [Test]
        public void TestUnlessExpression2()
        {
            Assert.That(CompileAndExecute("x = 2; x += 1 unless 2 < 3; x;"), Is.EqualTo(2));
        }

        [Test]
        public void TestUntil()
        {
            Assert.That(CompileAndExecute("i = 10; x = 0; y = 2; until((i -= 1) == 0) { x = ((x + y) * 2); };"),
                        Is.EqualTo(2044));
        }

        [Test]
        public void TestUntilExpression()
        {
            Assert.That(CompileAndExecute("i = 10; x = 0; y = 2; x = ((x + y) * 2) until ((i -= 1) < 0);"),
                        Is.EqualTo(4092));
        }

        [Test]
        public void TestWhile()
        {
            Assert.That(CompileAndExecute("i = 10; x = 0; y = 2; while((i -= 1) > 0) { x = ((x + y) * 2); };"),
                        Is.EqualTo(2044));
        }

        [Test]
        public void TestWhileExpression()
        {
            Assert.That(CompileAndExecute("i = 10; x = 0; y = 2; x = ((x + y) * 2) while((i -= 1) >= 0);"),
                        Is.EqualTo(4092));
        }

        [Test]
        public void TestSwitchOperator()
        {
            Assert.That(CompileAndExecute("x = 5; x ?? { 1 => 2, 2 => 4, 3 => 8, 4 => 16, 5 => 32, 6 => 64 };"),
                        Is.EqualTo(32));
        }
    }
}