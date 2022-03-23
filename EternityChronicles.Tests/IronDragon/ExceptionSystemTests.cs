// ExceptionSystemTests.cs in EternityChronicles/EternityChronicles.Tests
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

using System;
using NUnit.Framework;

namespace EternityChronicles.Tests.IronDragon
{
    [TestFixture]
    public class ExceptionSystemTests : DragonAbstractTestFixture
    {
        [SetUp]
        public void Init()
        {
            CompileAndExecute("class DragonException < Exception { def new(msg = '') { @Dragonmessage = msg; }; };");
        }

        [Test]
        public void TestExceptionSystemThrow()
        {
            Assert.Throws<Exception>(() => CompileAndExecute("x = Exception(); throw x;"));
        }

        [Test]
        public void TestRescue1()
        {
            Assert.That(CompileAndExecute("x = 0; begin { throw Exception(); } rescue Exception => e { x = 10; }; x;"),
                        Is.EqualTo(10));
        }

        [Test]
        public void TestRescue2()
        {
            Assert.That(
                        CompileAndExecute(
                                          "x = 0; begin { throw DragonException(); } rescue DragonException1, DragonException => e { x = 10; }; x;"),
                        Is.EqualTo(10));
        }

        [Test]
        public void TestRescue3()
        {
            Assert.That(CompileAndExecute("x = 0; begin { throw Exception(); } rescue * => e { x = 10; }; x;"),
                        Is.EqualTo(10));
        }

        [Test]
        public void TestRescue4()
        {
            Assert.That(
                        CompileAndExecute(
                                          "x = 0; begin { throw Exception('test'); } rescue Exception => z { x = z.Message; }; x;"),
                        Is.EqualTo("test"));
        }

        [Test]
        public void TestRescue5()
        {
            Assert.That(
                        CompileAndExecute(
                                          "x = 0; begin { throw DragonException('test'); } rescue DragonException1, DragonException => z { x = z.Dragonmessage; }; x;"),
                        Is.EqualTo("test"));
        }

        [Test]
        public void TestRescue6()
        {
            Assert.That(
                        CompileAndExecute("x = 0; begin { throw Exception('test'); } rescue * => z { x = z.Message; }; x;"),
                        Is.EqualTo("test"));
        }

        [Test]
        public void TestRescue7()
        {
            Assert.That(
                        CompileAndExecute(
                                          "x = 0; exc = 'Exception'; begin { throw Exception('test'); } rescue exc => z { x = z.Message; }; x;"),
                        Is.EqualTo("test"));
        }

        [Test]
        public void TestRescueEnsure()
        {
            Assert.That(
                        CompileAndExecute(
                                          "x = 0; begin { throw Exception(); } rescue * => z { x = z; } ensure { x = 10; }; x;"),
                        Is.EqualTo(10));
        }

        [Test]
        public void TestRescueElse()
        {
            Assert.That(CompileAndExecute("x = 0; begin { y = 10; } rescue * => z { x = z; } else { x = 25; }; x;"),
                        Is.EqualTo(25));
        }

        [Test]
        public void TestRescueElseEnsure()
        {
            Assert.That(
                        CompileAndExecute(
                                          "x = 0; begin { y = 10; } rescue * => z { x = z; } else { x = 25; } ensure { x *= 2; }; x;"),
                        Is.EqualTo(50));
        }
    }
}