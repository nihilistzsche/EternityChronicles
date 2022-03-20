// ModuleLoaderTests.cs in EternityChronicles/EternityChronicles.Tests
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
using NUnit.Framework;

namespace EternityChronicles.Tests.ECX
{
    [TestFixture]
    public class ModuleLoaderTests
    {
        [OneTimeSetUp]
        public void SetUp()
        {
            Directory.SetCurrentDirectory(new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName);
        }

        // ecx-ld-01 - Loading with no dependencies, single dir search path.
        [Test]
        public void TestLoadingSingleDir()
        {
            var _mc = new ModuleController();

            _mc.SearchPath.Add($"data{Path.DirectorySeparatorChar}ecx-ld");

            _mc.LoadModule("ecx-ld-01");

            Assert.IsTrue(_mc.IsLoaded("ecx-ld-01"));
        }

        // ecx-ld-02 - Loading with no dependencies, single dir search path, not found.
        [Test]
        public void TestLoadingSingleDirNotFound()
        {
            var _mc = new ModuleController();

            _mc.SearchPath.Add($"data{Path.DirectorySeparatorChar}ecx-ld");

            Assert.Throws<ModuleNotFoundException>(() => { _mc.LoadModule("ecx-ld-02"); });
        }

        // ecx-ld-03 - Loading with no dependencies, multiple dir search path, first dir.
        [Test]
        public void TestLoadingMultipleDirFirstDir()
        {
            var _mc = new ModuleController();

            _mc.SearchPath.Add($"data{Path.DirectorySeparatorChar}ecx-ld");
            _mc.SearchPath.Add($"data{Path.DirectorySeparatorChar}ecx-ld-2");
            _mc.SearchPath.Add($"data{Path.DirectorySeparatorChar}ecx-ld-3");

            _mc.LoadModule("ecx-ld-03");

            Assert.IsTrue(_mc.IsLoaded("ecx-ld-03"));
        }

        // ecx-ld-04 - Loading with no dependencies, multiple dir search path, middle dir.
        [Test]
        public void TestLoadingMultipleDirMiddleDir()
        {
            var _mc = new ModuleController();

            _mc.SearchPath.Add($"data{Path.DirectorySeparatorChar}ecx-ld-2");
            _mc.SearchPath.Add($"data{Path.DirectorySeparatorChar}ecx-ld");
            _mc.SearchPath.Add($"data{Path.DirectorySeparatorChar}ecx-ld-3");

            _mc.LoadModule("ecx-ld-04");
            Assert.IsTrue(_mc.IsLoaded("ecx-ld-04"));
        }

        // ecx-ld-05 - Loading with no dependencies, multiple dir search path, last dir.
        [Test]
        public void TestLoadingMultipleDirLastDir()
        {
            var _mc = new ModuleController();

            _mc.SearchPath.Add($"data{Path.DirectorySeparatorChar}ecx-ld-2");
            _mc.SearchPath.Add($"data{Path.DirectorySeparatorChar}ecx-ld-3");
            _mc.SearchPath.Add($"data{Path.DirectorySeparatorChar}ecx-ld");

            _mc.LoadModule("ecx-ld-05");
            Assert.IsTrue(_mc.IsLoaded("ecx-ld-05"));
        }

        // ecx-ld-06 - Loading with no dependencies, multiple dir search path, not found.
        [Test]
        public void TestLoadingMultipleDirNotFound()
        {
            var _mc = new ModuleController();

            _mc.SearchPath.Add($"data{Path.DirectorySeparatorChar}ecx-ld");
            _mc.SearchPath.Add($"data{Path.DirectorySeparatorChar}ecx-ld-2");
            _mc.SearchPath.Add($"data{Path.DirectorySeparatorChar}ecx-ld-3");

            Assert.Throws<ModuleNotFoundException>(() => _mc.LoadModule("ecx-ld-06"));
        }
    }
}