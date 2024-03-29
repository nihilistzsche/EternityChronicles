// RoleRegistrationTests.cs in EternityChronicles/EternityChronicles.Tests
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
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using ECX.Core;
using ECX.Core.Loader;
using NUnit.Framework;

namespace EternityChronicles.Tests.ECX
{
    internal class RoleRegisterTest<TArg> where TArg : IECXTestRole1
    {
        private readonly TArg _member;

        public RoleRegisterTest(TArg member)
        {
            _member = member;
        }

        public string GetMessage()
        {
            return _member.WriteMyself();
        }
    }

    internal class Dummy : IECXTestRole1
    {
        public string WriteMyself()
        {
            throw new NotImplementedException();
        }
    }

    // TODO: Build a wrapper type that has a default constructor that creates an instance of Wonder and implements the members of IWonder by
    // calling the methods on the backing object.
    [TestFixture]
    public class RoleRegistrationTests
    {
        [OneTimeSetUp]
        public void SetUp()
        {
            Directory.SetCurrentDirectory(new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName);
        }

        protected IECXTestRole1 I;

        protected ModuleController Controller;

        protected dynamic Di;

        public void RegisterIecxTestRole1(Assembly asm, Type type)
        {
            I = (IECXTestRole1)asm.CreateInstance(type.ToString());
        }

        public string CallIecxTestRole1()
        {
            return I.WriteMyself();
        }

        public void UnregisterIecxTestRole1(Assembly asm)
        {
        }

        public void GenericRegister(Assembly asm, Type type)
        {
            Di = typeof(RoleRegisterTest<Dummy>).CreateGenericInstance(new[] { type },
                                                                       new[] { asm.CreateInstance(type.ToString()) });
        }

        public void GenericUnregister(Assembly asm)
        {
        }

        [Test]
        public void TestAppRegisteredRole()
        {
            var mc = new ModuleController();

            mc.SearchPath.Add($"data{Path.DirectorySeparatorChar}ecx-rr");

            Controller = mc;

            mc.RegisterNewRole("IECXTestRole1", typeof(IECXTestRole1), RegisterIecxTestRole1, UnregisterIecxTestRole1);

            mc.LoadModule("ecx-rr-01a");

            Assert.AreEqual("I am an instantiated ecx_rr_01a, go me!", CallIecxTestRole1());
        }

        [Test]
        public void TestModuleRegisteredRole()
        {
            var mc = new ModuleController();

            mc.SearchPath.Add($"data{Path.DirectorySeparatorChar}ecx-rr");

            mc.RegisterNewRole("IECXTestRole1", typeof(IECXTestRole1), RegisterIecxTestRole1, UnregisterIecxTestRole1);

            mc.LoadModule("ecx-rr-02a");

            Assert.AreEqual("Module initiated role!", CallIecxTestRole1());
        }

        [Test]
        public void TestGenericRole()
        {
            var mc = new ModuleController();

            mc.SearchPath.Add($"data{Path.DirectorySeparatorChar}ecx-rr");

            mc.RegisterNewRole("IECXTestRole1", typeof(IECXTestRole1), GenericRegister, GenericUnregister);

            mc.LoadModule("ecx-rr-01a");

            Assert.AreEqual("I am an instantiated ecx_rr_01a, go me!", Di.GetMessage());
        }

        [Test]
        public void TestSearchForModule()
        {
            var mc = new ModuleController();

            mc.SearchPath.Add($"data{Path.DirectorySeparatorChar}ecx-rs");

            mc.RegisterNewRole("IECXTestRole1", typeof(IECXTestRole1), RegisterIecxTestRole1, UnregisterIecxTestRole1);

            var m = mc.SearchForModulesForRole("IECXTestRole1");

            Assert.AreEqual(new List<string> { "ecx-rs-01a", "ecx-rs-01c" }, m);

            foreach (var n in m) Assert.AreEqual(false, mc.IsLoaded(n));
        }
    }
}