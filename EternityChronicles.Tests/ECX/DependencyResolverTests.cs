//
// ecx-dr.cs
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