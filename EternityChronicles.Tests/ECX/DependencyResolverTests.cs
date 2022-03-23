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
            var _mc = new ModuleController();

            _mc.SearchPath.Add($"data{Path.DirectorySeparatorChar}ecx-dr");

            // This will look for a specific version of ecx-dr-01b
            _mc.LoadModule("ecx-dr-01a");

            // If it worked, we should have a domain for ecx-dr-01b
            Assert.IsTrue(_mc.IsLoaded("ecx-dr-01a"));
            Assert.IsTrue(_mc.IsLoaded("ecx-dr-01b"));
        }

        // !=
        [Test]
        public void TestDependencyResolveNotEqual()
        {
            var _mc = new ModuleController();

            _mc.SearchPath.Add($"data{Path.DirectorySeparatorChar}ecx-dr");

            // This will look for a specific version of ecx-dr-01b
            _mc.LoadModule("ecx-dr-02a");

            // If it worked, we should have a domain for ecx-dr-01b
            Assert.IsTrue(_mc.IsLoaded("ecx-dr-02a"));
            Assert.IsTrue(_mc.IsLoaded("ecx-dr-02b"));
        }

        // <<
        [Test]
        public void TestDependencyResolveLessThan()
        {
            var _mc = new ModuleController();

            _mc.SearchPath.Add($"data{Path.DirectorySeparatorChar}ecx-dr");

            // This will look for a specific version of ecx-dr-01b
            _mc.LoadModule("ecx-dr-03a");

            // If it worked, we should have a domain for ecx-dr-01b
            Assert.IsTrue(_mc.IsLoaded("ecx-dr-03a"));
            Assert.IsTrue(_mc.IsLoaded("ecx-dr-03b"));
        }

        // >>
        [Test]
        public void TestDependencyResolveGreaterThan()
        {
            var _mc = new ModuleController();

            _mc.SearchPath.Add($"data{Path.DirectorySeparatorChar}ecx-dr");

            // This will look for a specific version of ecx-dr-01b
            _mc.LoadModule("ecx-dr-04a");

            // If it worked, we should have a domain for ecx-dr-01b
            Assert.IsTrue(_mc.IsLoaded("ecx-dr-04a"));
            Assert.IsTrue(_mc.IsLoaded("ecx-dr-04b"));
        }

        // <=
        [Test]
        public void TestDependencyResolveLessThanEqual()
        {
            var _mc = new ModuleController();

            _mc.SearchPath.Add($"data{Path.DirectorySeparatorChar}ecx-dr");

            // This will look for a specific version of ecx-dr-01b
            _mc.LoadModule("ecx-dr-05a");

            // If it worked, we should have a domain for ecx-dr-01b
            Assert.IsTrue(_mc.IsLoaded("ecx-dr-05a"));
            Assert.IsTrue(_mc.IsLoaded("ecx-dr-05b"));
        }

        // >=
        [Test]
        public void TestDependencyResolveGreaterThanEqual()
        {
            var _mc = new ModuleController();

            _mc.SearchPath.Add($"data{Path.DirectorySeparatorChar}ecx-dr");

            // This will look for a specific version of ecx-dr-01b
            _mc.LoadModule("ecx-dr-06a");

            // If it worked, we should have a domain for ecx-dr-01b
            Assert.IsTrue(_mc.IsLoaded("ecx-dr-06a"));
            Assert.IsTrue(_mc.IsLoaded("ecx-dr-06b"));
        }

        // ##
        [Test]
        public void TestDependencyResolveLoaded()
        {
            var _mc = new ModuleController();

            _mc.SearchPath.Add($"data{Path.DirectorySeparatorChar}ecx-dr");

            // This will look for a specific version of ecx-dr-01b
            _mc.LoadModule("ecx-dr-07a");

            // If it worked, we should have a domain for ecx-dr-01b
            Assert.IsTrue(_mc.IsLoaded("ecx-dr-07a"));
            Assert.IsTrue(_mc.IsLoaded("ecx-dr-07b"));
        }

        // (&& (==) (>=))
        [Test]
        public void TestDependencyResolveAndEqualGreaterThanEqual()
        {
            var _mc = new ModuleController();

            _mc.SearchPath.Add($"data{Path.DirectorySeparatorChar}ecx-dr");

            _mc.LoadModule("ecx-dr-08a");

            Assert.IsTrue(_mc.IsLoaded("ecx-dr-08a"));
            Assert.IsTrue(_mc.IsLoaded("ecx-dr-08b"));
            Assert.IsTrue(_mc.IsLoaded("ecx-dr-08c"));
        }

        // (|| (>>) (<<))
        [Test]
        public void TestDependencyResolveOrGreaterThanLessThan()
        {
            var _mc = new ModuleController();

            _mc.SearchPath.Add($"data{Path.DirectorySeparatorChar}ecx-dr");

            _mc.LoadModule("ecx-dr-09a");

            Assert.IsTrue(_mc.IsLoaded("ecx-dr-09a"));
            Assert.IsFalse(_mc.IsLoaded("ecx-dr-09b"));
            Assert.IsTrue(_mc.IsLoaded("ecx-dr-09c"));
        }

        // (^^ (>>) (!=))
        [Test]
        public void TestDependencyResolveXorGreaterThanNotEqual()
        {
            var _mc = new ModuleController();

            _mc.SearchPath.Add($"data{Path.DirectorySeparatorChar}ecx-dr");

            _mc.LoadModule("ecx-dr-10a");

            Assert.IsTrue(_mc.IsLoaded("ecx-dr-10a"));
            Assert.IsTrue(_mc.IsLoaded("ecx-dr-10b"));
            Assert.IsFalse(_mc.IsLoaded("ecx-dr-10c"));
        }

        // (!#))
        [Test]
        public void TestDependencyResolveNotLoaded()
        {
            var _mc = new ModuleController();

            _mc.SearchPath.Add($"data{Path.DirectorySeparatorChar}ecx-dr");

            _mc.LoadModule("ecx-dr-11a");

            Assert.IsTrue(_mc.IsLoaded("ecx-dr-11a"));
            Assert.IsFalse(_mc.IsLoaded("ecx-dr-11b"));
        }

        // (?? (>=))
        [Test]
        public void TestDependencyResolveOptionalGreaterThanEqual()
        {
            var _mc = new ModuleController();

            _mc.SearchPath.Add($"data{Path.DirectorySeparatorChar}ecx-dr");

            _mc.LoadModule("ecx-dr-12a");

            Assert.IsTrue(_mc.IsLoaded("ecx-dr-12a"));
            Assert.IsTrue(_mc.IsLoaded("ecx-dr-12b"));
        }

        // (&& (|| (==) (!=)) (?? (##)))
        [Test]
        public void TestDependencyResolveAndOrEqualNotEqualOptionalLoaded()
        {
            var _mc = new ModuleController();

            _mc.SearchPath.Add($"data{Path.DirectorySeparatorChar}ecx-dr");

            _mc.LoadModule("ecx-dr-13a");

            Assert.IsTrue(_mc.IsLoaded("ecx-dr-13a"));
            Assert.IsTrue(_mc.IsLoaded("ecx-dr-13b"));
            Assert.IsFalse(_mc.IsLoaded("ecx-dr-13c"));
            Assert.IsFalse(_mc.IsLoaded("ecx-dr-13d"));
        }

        // (|| (&& (##) (##)) (==)))
        [Test]
        public void TestDependencyResolveOrAndLoadedLoadedEqual()
        {
            var _mc = new ModuleController();

            _mc.SearchPath.Add($"data{Path.DirectorySeparatorChar}ecx-dr");

            _mc.LoadModule("ecx-dr-14a");

            Assert.IsTrue(_mc.IsLoaded("ecx-dr-14a"));
            Assert.IsTrue(_mc.IsLoaded("ecx-dr-14b"));
            Assert.IsTrue(_mc.IsLoaded("ecx-dr-14c"));
            Assert.IsFalse(_mc.IsLoaded("ecx-dr-14d"));
        }
    }
}