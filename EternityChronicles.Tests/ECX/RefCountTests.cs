// RefCountTests.cs in EternityChronicles/EternityChronicles.Tests
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
    public class RefCountTests
    {
        [OneTimeSetUp]
        public void SetUp()
        {
            Directory.SetCurrentDirectory(new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName);
        }

        [Test]
        public void TestRefCountSingleLoad()
        {
            var _mc = new ModuleController();

            _mc.SearchPath.Add($"data{Path.DirectorySeparatorChar}ecx-rc");

            _mc.LoadModule("ecx-rc-01");

            Assert.IsTrue(_mc.RefCount("ecx-rc-01") == 1);
        }

        [Test]
        public void TestRefCountMultipleLoad()
        {
            var _mc = new ModuleController();

            _mc.SearchPath.Add($"data{Path.DirectorySeparatorChar}ecx-rc");

            _mc.LoadModule("ecx-rc-02");
            _mc.LoadModule("ecx-rc-02");

            Assert.IsTrue(_mc.RefCount("ecx-rc-02") == 2);
        }

        [Test]
        public void TestRefCountDependencies()
        {
            var _mc = new ModuleController();

            _mc.SearchPath.Add($"data{Path.DirectorySeparatorChar}ecx-rc");

            _mc.LoadModule("ecx-rc-03a");

            Assert.IsTrue(_mc.RefCount("ecx-rc-03c") == 3);
        }

        [Test]
        public void TestRefCountAfterLoadDependency()
        {
            var _mc = new ModuleController();

            _mc.SearchPath.Add($"data{Path.DirectorySeparatorChar}ecx-rc");

            _mc.LoadModule("ecx-rc-04a");
            _mc.LoadModule("ecx-rc-04b");

            Assert.IsTrue(_mc.RefCount("ecx-rc-04a") == 2);
            Assert.IsTrue(_mc.RefCount("ecx-rc-04b") == 1);
        }
    }
}