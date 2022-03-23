// ModuleLoaderTests.cs in EternityChronicles/EternityChronicles.Tests
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