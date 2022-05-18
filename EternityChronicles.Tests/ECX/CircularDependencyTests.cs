// CircularDependencyTests.cs in EternityChronicles/EternityChronicles.Tests
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
using ECX.Core.Dependency.Resolver;
using ECX.Core.Loader;
using NUnit.Framework;

namespace EternityChronicles.Tests.ECX
{
    [TestFixture]
    public class CircularDependencyTests
    {
        [OneTimeSetUp]
        public void SetUp()
        {
            Directory.SetCurrentDirectory(new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName);
        }

        [Test]
        public void TestCircularDependencyAonBonA()
        {
            var mc = new ModuleController();

            mc.SearchPath.Add($"data{Path.DirectorySeparatorChar}ecx-cr");

            Assert.Throws<CircularDependencyException>(() => { mc.LoadModule("ecx-cr-01a"); });
        }

        [Test]
        public void TestCircularDependencyAonBonConA()
        {
            var mc = new ModuleController();

            mc.SearchPath.Add($"data{Path.DirectorySeparatorChar}ecx-cr");

            Assert.Throws<CircularDependencyException>(() => { mc.LoadModule("ecx-cr-02a"); });
        }

        [Test]
        public void TestCircularDependencyOnSelf()
        {
            var mc = new ModuleController();

            mc.SearchPath.Add($"data{Path.DirectorySeparatorChar}ecx-cr");

            Assert.Throws<CircularDependencyException>(() => { mc.LoadModule("ecx-cr-03a"); });
        }
    }
}