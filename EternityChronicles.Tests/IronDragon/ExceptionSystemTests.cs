// -----------------------------------------------------------------------
// <copyright file="ExceptionSystemTests.cs" Company="Michael Tindal">
// Copyright 2011-2014 Michael Tindal
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

using System;
using IronDragon;
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