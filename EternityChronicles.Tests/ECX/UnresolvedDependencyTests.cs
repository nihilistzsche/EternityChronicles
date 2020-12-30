//
// nm_ur.cs
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
	public class UnresolvedDepencenyTests
	{
        [OneTimeSetUp]
        public void SetUp()
        {
            Directory.SetCurrentDirectory(new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName);
		}

        // ==
        [Test]
		public void TestUnresolvedDependencyEqual ()
		{
			ModuleController _mc = new ModuleController ();

			_mc.SearchPath.Add ("data" + Path.DirectorySeparatorChar + "ecx-ur");

            Assert.Throws<UnresolvedDependencyException>(() =>
            {
                UnresolvedDependencyException e = null;
                // This will look for a specific version of ecx-ur-01b
                try
                {
                    _mc.LoadModule("ecx-ur-01a");
                }
                catch (UnresolvedDependencyException exc)
                {
                    e = exc;
                }

                Assert.IsFalse(_mc.IsLoaded("ecx-ur-01a"), "ecx-ur-01a is loaded, it should not be.");
                Assert.IsFalse(_mc.IsLoaded("ecx-ur-01b"), "ecx-ur-01b is loaded, it should not be.");
                Assert.IsFalse(_mc.IsLoaded("ecx-ur-01c"), "ecx-ur-01c is loaded, it should not be.");
                Assert.IsFalse(_mc.IsLoaded("ecx-ur-01d"), "ecx-ur-01d is loaded, it should not be.");
                Assert.IsFalse(_mc.IsLoaded("ecx-ur-01e"), "ecx-ur-01e is loaded, it should not be.");
                Assert.IsFalse(_mc.IsLoaded("ecx-ur-01f"), "ecx-ur-01f is loaded, it should not be.");
                Assert.IsFalse(_mc.IsLoaded("ecx-ur-01g"), "ecx-ur-01g is loaded, it should not be.");


                if (e != null)
                    throw e;
            });
		}

		// !=
		[Test]
		public void TestUnresolvedDependencyNotEqual ()
		{
			ModuleController _mc = new ModuleController ();

			_mc.SearchPath.Add ("data" + Path.DirectorySeparatorChar + "ecx-ur");

			// This will look for a specific version of ecx-ur-01b
			Assert.Throws<UnresolvedDependencyException>(() => { _mc.LoadModule("ecx-ur-02a"); });
		}

		// <<
		[Test]
		public void TestUnresolvedDependencyLessThan ()
		{
			ModuleController _mc = new ModuleController ();

			_mc.SearchPath.Add ("data" + Path.DirectorySeparatorChar + "ecx-ur");

			// This will look for a specific version of ecx-ur-01b
			Assert.Throws<UnresolvedDependencyException>(() => { _mc.LoadModule("ecx-ur-03a"); });
		}

		// >>
		[Test]
		public void TestUnresolvedDependencyGreaterThan ()
		{
			ModuleController _mc = new ModuleController ();

			_mc.SearchPath.Add ("data" + Path.DirectorySeparatorChar + "ecx-ur");

			// This will look for a specific version of ecx-ur-01b
			Assert.Throws<UnresolvedDependencyException>(() => { _mc.LoadModule("ecx-ur-04a"); });
		}

		// <=
		[Test]
		public void TestUnresolvedDependencyLessThanEqual ()
		{
			ModuleController _mc = new ModuleController ();

			_mc.SearchPath.Add ("data" + Path.DirectorySeparatorChar + "ecx-ur");

			// This will look for a specific version of ecx-ur-01b
			Assert.Throws<UnresolvedDependencyException>(() => { _mc.LoadModule("ecx-ur-05a"); });
		}

		// >=
		[Test]
		public void TestUnresolvedDependencyGreaterThanEqual ()
		{
			ModuleController _mc = new ModuleController ();

			_mc.SearchPath.Add ("data" + Path.DirectorySeparatorChar + "ecx-ur");

			// This will look for a specific version of ecx-ur-01b
			Assert.Throws<UnresolvedDependencyException>(() => { _mc.LoadModule("ecx-ur-06a"); });
		}

		// ##
		[Test]
		public void TestUnresolvedDependencyLoaded ()
		{
			ModuleController _mc = new ModuleController ();

			_mc.SearchPath.Add ("data" + Path.DirectorySeparatorChar + "ecx-ur");

			// This will look for a specific version of ecx-ur-01b
			Assert.Throws<UnresolvedDependencyException>(() => { _mc.LoadModule("ecx-ur-07a"); });
		}

		// (&& (==) (>=))
		[Test]
		public void TestUnresolvedDependencyAndEqualGreaterThanEqual ()
		{
			ModuleController _mc = new ModuleController ();

			_mc.SearchPath.Add ("data" + Path.DirectorySeparatorChar + "ecx-ur");

			Assert.Throws<UnresolvedDependencyException>(() => { _mc.LoadModule("ecx-ur-08a"); });
		}

		// (|| (>>) (<<))
		[Test]
		public void TestUnresolvedDependencyGreaterThanLessThan ()
		{
			ModuleController _mc = new ModuleController ();

			_mc.SearchPath.Add ("data" + Path.DirectorySeparatorChar + "ecx-ur");

			Assert.Throws<UnresolvedDependencyException>(() => { _mc.LoadModule("ecx-ur-09a"); });
		}

		// (^^ (>>) (!=))
		[Test]
		public void TestUnresolvedDependencyXorGreaterThanNotEqual ()
		{
			ModuleController _mc = new ModuleController ();

			_mc.SearchPath.Add ("data" + Path.DirectorySeparatorChar + "ecx-ur");

			Assert.Throws<UnresolvedDependencyException>(() => { _mc.LoadModule("ecx-ur-10a"); });
		}

		// (!#)
		[Test]
		public void TestUnresolvedDependencyNotLoaded ()
		{
			ModuleController _mc = new ModuleController ();

			_mc.SearchPath.Add ("data" + Path.DirectorySeparatorChar + "ecx-ur");

			_mc.LoadModule("ecx-ur-11b");
            _mc.LoadModule("ecx-ur-11b");
            Assert.Throws<UnresolvedDependencyException>(() => { _mc.LoadModule("ecx-ur-11a"); });
		}

		// (>=)
		[Test]
		public void TestUnresolvedDependencyGreaterThanEqualWrongVersion ()
		{
			ModuleController _mc = new ModuleController ();

			_mc.SearchPath.Add ("data" + Path.DirectorySeparatorChar + "ecx-ur");

			Assert.Throws<UnresolvedDependencyException>(() => { _mc.LoadModule("ecx-ur-12a"); });
		}

		// (&& (|| (==) (!=)) (?? (##)))
		[Test]
		public void TestUnresolvedDependencyAndOrEqualNotEqualOptionalLoaded ()
		{
			ModuleController _mc = new ModuleController ();

			_mc.SearchPath.Add ("data" + Path.DirectorySeparatorChar + "ecx-ur");

			Assert.Throws<UnresolvedDependencyException>(() => { _mc.LoadModule("ecx-ur-13a"); });
		}

		// (|| (&& (##) (##)) (==))
		[Test]
		public void TestUnresolvedDependencyOrAndLoadedLoadedEqual ()
		{
			ModuleController _mc = new ModuleController ();

			_mc.SearchPath.Add ("data" + Path.DirectorySeparatorChar + "ecx-ur");

			Assert.Throws<UnresolvedDependencyException>(() => { _mc.LoadModule("ecx-ur-14a"); });
		}
	}
}
