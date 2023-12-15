// UnresolvedDependencyTests.cs in EternityChronicles/EternityChronicles.Tests
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

// ReSharper disable HeapView.DelegateAllocation
// ReSharper disable HeapView.BoxingAllocation

namespace EternityChronicles.Tests.ECX
{
    [TestFixture]
    public class UnresolvedDepencenyTests
    {
        [OneTimeSetUp]
        public void SetUp()
        {
            Directory.SetCurrentDirectory(new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName ??
                                          string.Empty);
        }

        // ==
        [Test]
        public void TestUnresolvedDependencyEqual()
        {
            var mc = new ModuleController();

            mc.SearchPath.Add($"data{Path.DirectorySeparatorChar}ecx-ur");

            Assert.Throws<UnresolvedDependencyException>(() =>
                                                         {
                                                             UnresolvedDependencyException e = null;
                                                             // This will look for a specific version of ecx-ur-01b
                                                             try
                                                             {
                                                                 mc.LoadModule("ecx-ur-01a");
                                                             }
                                                             catch (UnresolvedDependencyException exc)
                                                             {
                                                                 e = exc;
                                                             }

                                                             ClassicAssert.IsFalse(mc.IsLoaded("ecx-ur-01a"),
                                                                            "ecx-ur-01a is loaded, it should not be.");
                                                             ClassicAssert.IsFalse(mc.IsLoaded("ecx-ur-01b"),
                                                                            "ecx-ur-01b is loaded, it should not be.");
                                                             ClassicAssert.IsFalse(mc.IsLoaded("ecx-ur-01c"),
                                                                            "ecx-ur-01c is loaded, it should not be.");
                                                             ClassicAssert.IsFalse(mc.IsLoaded("ecx-ur-01d"),
                                                                            "ecx-ur-01d is loaded, it should not be.");
                                                             ClassicAssert.IsFalse(mc.IsLoaded("ecx-ur-01e"),
                                                                            "ecx-ur-01e is loaded, it should not be.");
                                                             ClassicAssert.IsFalse(mc.IsLoaded("ecx-ur-01f"),
                                                                            "ecx-ur-01f is loaded, it should not be.");
                                                             ClassicAssert.IsFalse(mc.IsLoaded("ecx-ur-01g"),
                                                                            "ecx-ur-01g is loaded, it should not be.");


                                                             if (e != null)
                                                                 throw e;
                                                         });
        }

        // !=
        [Test]
        public void TestUnresolvedDependencyNotEqual()
        {
            var mc = new ModuleController();

            mc.SearchPath.Add($"data{Path.DirectorySeparatorChar}ecx-ur");

            // This will look for a specific version of ecx-ur-01b
            Assert.Throws<UnresolvedDependencyException>(() => { mc.LoadModule("ecx-ur-02a"); });
        }

        // <<
        [Test]
        public void TestUnresolvedDependencyLessThan()
        {
            var mc = new ModuleController();

            mc.SearchPath.Add($"data{Path.DirectorySeparatorChar}ecx-ur");

            // This will look for a specific version of ecx-ur-01b
            Assert.Throws<UnresolvedDependencyException>(() => { mc.LoadModule("ecx-ur-03a"); });
        }

        // >>
        [Test]
        public void TestUnresolvedDependencyGreaterThan()
        {
            var mc = new ModuleController();

            mc.SearchPath.Add($"data{Path.DirectorySeparatorChar}ecx-ur");

            // This will look for a specific version of ecx-ur-01b
            Assert.Throws<UnresolvedDependencyException>(() => { mc.LoadModule("ecx-ur-04a"); });
        }

        // <=
        [Test]
        public void TestUnresolvedDependencyLessThanEqual()
        {
            var mc = new ModuleController();

            mc.SearchPath.Add($"data{Path.DirectorySeparatorChar}ecx-ur");

            // This will look for a specific version of ecx-ur-01b
            Assert.Throws<UnresolvedDependencyException>(() => { mc.LoadModule("ecx-ur-05a"); });
        }

        // >=
        [Test]
        public void TestUnresolvedDependencyGreaterThanEqual()
        {
            var mc = new ModuleController();

            mc.SearchPath.Add($"data{Path.DirectorySeparatorChar}ecx-ur");

            // This will look for a specific version of ecx-ur-01b
            Assert.Throws<UnresolvedDependencyException>(() => { mc.LoadModule("ecx-ur-06a"); });
        }

        // ##
        [Test]
        public void TestUnresolvedDependencyLoaded()
        {
            var mc = new ModuleController();

            mc.SearchPath.Add($"data{Path.DirectorySeparatorChar}ecx-ur");

            // This will look for a specific version of ecx-ur-01b
            Assert.Throws<UnresolvedDependencyException>(() => { mc.LoadModule("ecx-ur-07a"); });
        }

        // (&& (==) (>=))
        [Test]
        public void TestUnresolvedDependencyAndEqualGreaterThanEqual()
        {
            var mc = new ModuleController();

            mc.SearchPath.Add($"data{Path.DirectorySeparatorChar}ecx-ur");

            Assert.Throws<UnresolvedDependencyException>(() => { mc.LoadModule("ecx-ur-08a"); });
        }

        // (|| (>>) (<<))
        [Test]
        public void TestUnresolvedDependencyGreaterThanLessThan()
        {
            var mc = new ModuleController();

            mc.SearchPath.Add($"data{Path.DirectorySeparatorChar}ecx-ur");

            Assert.Throws<UnresolvedDependencyException>(() => { mc.LoadModule("ecx-ur-09a"); });
        }

        // (^^ (>>) (!=))
        [Test]
        public void TestUnresolvedDependencyXorGreaterThanNotEqual()
        {
            var mc = new ModuleController();

            mc.SearchPath.Add($"data{Path.DirectorySeparatorChar}ecx-ur");

            Assert.Throws<UnresolvedDependencyException>(() => { mc.LoadModule("ecx-ur-10a"); });
        }

        // (!#)
        [Test]
        public void TestUnresolvedDependencyNotLoaded()
        {
            var mc = new ModuleController();

            mc.SearchPath.Add($"data{Path.DirectorySeparatorChar}ecx-ur");

            mc.LoadModule("ecx-ur-11b");
            mc.LoadModule("ecx-ur-11b");
            Assert.Throws<UnresolvedDependencyException>(() => { mc.LoadModule("ecx-ur-11a"); });
        }

        // (>=)
        [Test]
        public void TestUnresolvedDependencyGreaterThanEqualWrongVersion()
        {
            var mc = new ModuleController();

            mc.SearchPath.Add($"data{Path.DirectorySeparatorChar}ecx-ur");

            Assert.Throws<UnresolvedDependencyException>(() => { mc.LoadModule("ecx-ur-12a"); });
        }

        // (&& (|| (==) (!=)) (?? (##)))
        [Test]
        public void TestUnresolvedDependencyAndOrEqualNotEqualOptionalLoaded()
        {
            var mc = new ModuleController();

            mc.SearchPath.Add($"data{Path.DirectorySeparatorChar}ecx-ur");

            Assert.Throws<UnresolvedDependencyException>(() => { mc.LoadModule("ecx-ur-13a"); });
        }

        // (|| (&& (##) (##)) (==))
        [Test]
        public void TestUnresolvedDependencyOrAndLoadedLoadedEqual()
        {
            var mc = new ModuleController();

            mc.SearchPath.Add($"data{Path.DirectorySeparatorChar}ecx-ur");

            Assert.Throws<UnresolvedDependencyException>(() => { mc.LoadModule("ecx-ur-14a"); });
        }
    }
}
