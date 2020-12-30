//
// ecx-cr.cs
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
using Antlr.Runtime;
using ECX.Core.Dependency.Resolver;
using NUnit.Framework;
using ECX.Core.Loader;

namespace EternityChronicles.Tests.ECX
{
    [TestFixture]
	public class CircularDependencyTests {
        [OneTimeSetUp]
        public void SetUp()
        {
            Directory.SetCurrentDirectory(new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName);
        }

		[Test]
		public void TestCircularDependencyAonBonA () {
            ModuleController _mc = new ModuleController ();

			_mc.SearchPath.Add ("data" + Path.DirectorySeparatorChar + "ecx-cr");

			Assert.Throws<CircularDependencyException>(() => { _mc.LoadModule("ecx-cr-01a"); });
		}

		[Test]
		public void TestCircularDependencyAonBonConA () {
			ModuleController _mc = new ModuleController ();

			_mc.SearchPath.Add ("data" + Path.DirectorySeparatorChar + "ecx-cr");

            Assert.Throws<CircularDependencyException>(() => { _mc.LoadModule("ecx-cr-02a"); });
		}

		[Test]
		public void TestCircularDependencyOnSelf () {
			ModuleController _mc = new ModuleController ();

			_mc.SearchPath.Add ("data" + Path.DirectorySeparatorChar + "ecx-cr");

            Assert.Throws<CircularDependencyException>(() => { _mc.LoadModule("ecx-cr-03a"); });
		}
	}
}