//
// ecx-rc.cs
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