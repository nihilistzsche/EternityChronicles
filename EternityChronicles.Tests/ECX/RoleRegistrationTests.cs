//
// ecx-rr.cs
//
// Author:
//     Michael Tindal <mj.tindal@icloud.com>
//
// Copyright (C) 2005-2013 Michael Tindal and the individuals listed on
// the ChangeLog entries.
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
//

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

        protected IECXTestRole1 i;

        protected ModuleController controller;

        protected dynamic di;

        public void RegisterIECXTestRole1(Assembly asm, Type type)
        {
            i = (IECXTestRole1)asm.CreateInstance(type.ToString());
        }

        public string CallIECXTestRole1()
        {
            return i.WriteMyself();
        }

        public void UnregisterIECXTestRole1(Assembly asm)
        {
        }

        public void GenericRegister(Assembly asm, Type type)
        {
            di = typeof(RoleRegisterTest<Dummy>).CreateGenericInstance(new[] { type },
            new[] { asm.CreateInstance(type.ToString()) });
        }

        public void GenericUnregister(Assembly asm)
        {
        }

        [Test]
        public void TestAppRegisteredRole()
        {
            var _mc = new ModuleController();

            _mc.SearchPath.Add($"data{Path.DirectorySeparatorChar}ecx-rr");

            controller = _mc;

            _mc.RegisterNewRole("IECXTestRole1", typeof(IECXTestRole1), RegisterIECXTestRole1, UnregisterIECXTestRole1);

            _mc.LoadModule("ecx-rr-01a");

            Assert.AreEqual("I am an instantiated ecx_rr_01a, go me!", CallIECXTestRole1());
        }

        [Test]
        public void TestModuleRegisteredRole()
        {
            var _mc = new ModuleController();

            _mc.SearchPath.Add($"data{Path.DirectorySeparatorChar}ecx-rr");

            _mc.RegisterNewRole("IECXTestRole1", typeof(IECXTestRole1), RegisterIECXTestRole1, UnregisterIECXTestRole1);

            _mc.LoadModule("ecx-rr-02a");

            Assert.AreEqual("Module initiated role!", CallIECXTestRole1());
        }

        [Test]
        public void TestGenericRole()
        {
            var _mc = new ModuleController();

            _mc.SearchPath.Add($"data{Path.DirectorySeparatorChar}ecx-rr");

            _mc.RegisterNewRole("IECXTestRole1", typeof(IECXTestRole1), GenericRegister, GenericUnregister);

            _mc.LoadModule("ecx-rr-01a");

            Assert.AreEqual("I am an instantiated ecx_rr_01a, go me!", di.GetMessage());
        }

        [Test]
        public void TestSearchForModule()
        {
            var _mc = new ModuleController();

            _mc.SearchPath.Add($"data{Path.DirectorySeparatorChar}ecx-rs");

            _mc.RegisterNewRole("IECXTestRole1", typeof(IECXTestRole1), RegisterIECXTestRole1, UnregisterIECXTestRole1);

            var m = _mc.SearchForModulesForRole("IECXTestRole1");

            Assert.AreEqual(new List<string> { "ecx-rs-01a", "ecx-rs-01c" }, m);

            foreach (var _m in m) Assert.AreEqual(false, _mc.IsLoaded(_m));
        }
    }
}