// DependencyResolverTests.cs in EternityChronicles/EternityChronicles.Tests
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
using NUnit.Framework.Legacy;

namespace EternityChronicles.Tests.ECX
{
    [TestFixture]
    public class DependencyResolverTests
    {
        [OneTimeSetUp]
        public void SetUp()
        {
            Directory.SetCurrentDirectory(new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName);
        }

        // ==
        [Test]
        public void TestDependencyResolveEqual()
        {
            var mc = new ModuleController();

            mc.SearchPath.Add($"data{Path.DirectorySeparatorChar}ecx-dr");

            // This will look for a specific version of ecx-dr-01b
            mc.LoadModule("ecx-dr-01a");

            // If it worked, we should have a domain for ecx-dr-01b
            Assert.That(mc.IsLoaded("ecx-dr-01a"));
            Assert.That(mc.IsLoaded("ecx-dr-01b"));
        }

        // !=
        [Test]
        public void TestDependencyResolveNotEqual()
        {
            var mc = new ModuleController();

            mc.SearchPath.Add($"data{Path.DirectorySeparatorChar}ecx-dr");

            // This will look for a specific version of ecx-dr-01b
            mc.LoadModule("ecx-dr-02a");

            // If it worked, we should have a domain for ecx-dr-01b
            Assert.That(mc.IsLoaded("ecx-dr-02a"));
            Assert.That(mc.IsLoaded("ecx-dr-02b"));
        }

        // <<
        [Test]
        public void TestDependencyResolveLessThan()
        {
            var mc = new ModuleController();

            mc.SearchPath.Add($"data{Path.DirectorySeparatorChar}ecx-dr");

            // This will look for a specific version of ecx-dr-01b
            mc.LoadModule("ecx-dr-03a");

            // If it worked, we should have a domain for ecx-dr-01b
            Assert.That(mc.IsLoaded("ecx-dr-03a"));
            Assert.That(mc.IsLoaded("ecx-dr-03b"));
        }

        // >>
        [Test]
        public void TestDependencyResolveGreaterThan()
        {
            var mc = new ModuleController();

            mc.SearchPath.Add($"data{Path.DirectorySeparatorChar}ecx-dr");

            // This will look for a specific version of ecx-dr-01b
            mc.LoadModule("ecx-dr-04a");

            // If it worked, we should have a domain for ecx-dr-01b
            Assert.That(mc.IsLoaded("ecx-dr-04a"));
            Assert.That(mc.IsLoaded("ecx-dr-04b"));
        }

        // <=
        [Test]
        public void TestDependencyResolveLessThanEqual()
        {
            var mc = new ModuleController();

            mc.SearchPath.Add($"data{Path.DirectorySeparatorChar}ecx-dr");

            // This will look for a specific version of ecx-dr-01b
            mc.LoadModule("ecx-dr-05a");

            // If it worked, we should have a domain for ecx-dr-01b
            Assert.That(mc.IsLoaded("ecx-dr-05a"));
            Assert.That(mc.IsLoaded("ecx-dr-05b"));
        }

        // >=
        [Test]
        public void TestDependencyResolveGreaterThanEqual()
        {
            var mc = new ModuleController();

            mc.SearchPath.Add($"data{Path.DirectorySeparatorChar}ecx-dr");

            // This will look for a specific version of ecx-dr-01b
            mc.LoadModule("ecx-dr-06a");

            // If it worked, we should have a domain for ecx-dr-01b
            Assert.That(mc.IsLoaded("ecx-dr-06a"));
            Assert.That(mc.IsLoaded("ecx-dr-06b"));
        }

        // ##
        [Test]
        public void TestDependencyResolveLoaded()
        {
            var mc = new ModuleController();

            mc.SearchPath.Add($"data{Path.DirectorySeparatorChar}ecx-dr");

            // This will look for a specific version of ecx-dr-01b
            mc.LoadModule("ecx-dr-07a");

            // If it worked, we should have a domain for ecx-dr-01b
            Assert.That(mc.IsLoaded("ecx-dr-07a"));
            Assert.That(mc.IsLoaded("ecx-dr-07b"));
        }

        // (&& (==) (>=))
        [Test]
        public void TestDependencyResolveAndEqualGreaterThanEqual()
        {
            var mc = new ModuleController();

            mc.SearchPath.Add($"data{Path.DirectorySeparatorChar}ecx-dr");

            mc.LoadModule("ecx-dr-08a");

            Assert.That(mc.IsLoaded("ecx-dr-08a"));
            Assert.That(mc.IsLoaded("ecx-dr-08b"));
            Assert.That(mc.IsLoaded("ecx-dr-08c"));
        }

        // (|| (>>) (<<))
        [Test]
        public void TestDependencyResolveOrGreaterThanLessThan()
        {
            var mc = new ModuleController();

            mc.SearchPath.Add($"data{Path.DirectorySeparatorChar}ecx-dr");

            mc.LoadModule("ecx-dr-09a");

            Assert.That(mc.IsLoaded("ecx-dr-09a"));
            ClassicAssert.IsFalse(mc.IsLoaded("ecx-dr-09b"));
            Assert.That(mc.IsLoaded("ecx-dr-09c"));
        }

        // (^^ (>>) (!=))
        [Test]
        public void TestDependencyResolveXorGreaterThanNotEqual()
        {
            var mc = new ModuleController();

            mc.SearchPath.Add($"data{Path.DirectorySeparatorChar}ecx-dr");

            mc.LoadModule("ecx-dr-10a");

            Assert.That(mc.IsLoaded("ecx-dr-10a"));
            Assert.That(mc.IsLoaded("ecx-dr-10b"));
            ClassicAssert.IsFalse(mc.IsLoaded("ecx-dr-10c"));
        }

        // (!#))
        [Test]
        public void TestDependencyResolveNotLoaded()
        {
            var mc = new ModuleController();

            mc.SearchPath.Add($"data{Path.DirectorySeparatorChar}ecx-dr");

            mc.LoadModule("ecx-dr-11a");

            Assert.That(mc.IsLoaded("ecx-dr-11a"));
            ClassicAssert.IsFalse(mc.IsLoaded("ecx-dr-11b"));
        }

        // (?? (>=))
        [Test]
        public void TestDependencyResolveOptionalGreaterThanEqual()
        {
            var mc = new ModuleController();

            mc.SearchPath.Add($"data{Path.DirectorySeparatorChar}ecx-dr");

            mc.LoadModule("ecx-dr-12a");

            Assert.That(mc.IsLoaded("ecx-dr-12a"));
            Assert.That(mc.IsLoaded("ecx-dr-12b"));
        }

        // (&& (|| (==) (!=)) (?? (##)))
        [Test]
        public void TestDependencyResolveAndOrEqualNotEqualOptionalLoaded()
        {
            var mc = new ModuleController();

            mc.SearchPath.Add($"data{Path.DirectorySeparatorChar}ecx-dr");

            mc.LoadModule("ecx-dr-13a");

            Assert.That(mc.IsLoaded("ecx-dr-13a"));
            Assert.That(mc.IsLoaded("ecx-dr-13b"));
            ClassicAssert.IsFalse(mc.IsLoaded("ecx-dr-13c"));
            ClassicAssert.IsFalse(mc.IsLoaded("ecx-dr-13d"));
        }

        // (|| (&& (##) (##)) (==)))
        [Test]
        public void TestDependencyResolveOrAndLoadedLoadedEqual()
        {
            var mc = new ModuleController();

            mc.SearchPath.Add($"data{Path.DirectorySeparatorChar}ecx-dr");

            mc.LoadModule("ecx-dr-14a");

            Assert.That(mc.IsLoaded("ecx-dr-14a"));
            Assert.That(mc.IsLoaded("ecx-dr-14b"));
            Assert.That(mc.IsLoaded("ecx-dr-14c"));
            ClassicAssert.IsFalse(mc.IsLoaded("ecx-dr-14d"));
        }
    }
}
