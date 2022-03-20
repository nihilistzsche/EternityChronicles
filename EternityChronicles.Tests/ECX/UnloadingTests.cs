// UnloadingTests.cs in EternityChronicles/EternityChronicles.Tests
// 
// Copyright (C) 2022 Michael Tindal (nihilistzsche AT gmail DOT com)
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//    http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.IO;
using System.Reflection;
using ECX.Core.Loader;
using ECX.Core.Module;
using NUnit.Framework;

namespace EternityChronicles.Tests.ECX
{
    [TestFixture]
    public class UnloadingTests
    {
        [OneTimeSetUp]
        public void SetUp()
        {
            Directory.SetCurrentDirectory(new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName);
        }

        // ecx-ul-01 - Unloading with no dependencies
        [Test]
        public void TestUnloading()
        {
            var _mc = new ModuleController();

            _mc.SearchPath.Add($"data{Path.DirectorySeparatorChar}ecx-ul");

            _mc.LoadModule("ecx-ul-01");

            _mc.UnloadModule("ecx-ul-01");

            Assert.IsFalse(_mc.IsLoaded("ecx-ul-01"));
        }

        // ecx-ul-02 - Unloading with ref count > 1
        [Test]
        public void TestDomainStillReferencedUnloading()
        {
            var _mc = new ModuleController();

            _mc.SearchPath.Add($"data{Path.DirectorySeparatorChar}ecx-ul");

            _mc.LoadModule("ecx-ul-02");

            _mc.LoadModule("ecx-ul-02");

            Assert.Throws<DomainStillReferencedException>(() => { _mc.UnloadModule("ecx-ul-02"); });
        }
    }
}