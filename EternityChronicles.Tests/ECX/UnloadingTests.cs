// UnloadingTests.cs in EternityChronicles/EternityChronicles.Tests
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
using ECX.Core.Module;
using NUnit.Framework;
using NUnit.Framework.Legacy;

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
            var mc = new ModuleController();

            mc.SearchPath.Add($"data{Path.DirectorySeparatorChar}ecx-ul");

            mc.LoadModule("ecx-ul-01");

            mc.UnloadModule("ecx-ul-01");

            ClassicAssert.IsFalse(mc.IsLoaded("ecx-ul-01"));
        }

        // ecx-ul-02 - Unloading with ref count > 1
        [Test]
        public void TestDomainStillReferencedUnloading()
        {
            var mc = new ModuleController();

            mc.SearchPath.Add($"data{Path.DirectorySeparatorChar}ecx-ul");

            mc.LoadModule("ecx-ul-02");

            mc.LoadModule("ecx-ul-02");

            Assert.Throws<DomainStillReferencedException>(() => { mc.UnloadModule("ecx-ul-02"); });
        }
    }
}
