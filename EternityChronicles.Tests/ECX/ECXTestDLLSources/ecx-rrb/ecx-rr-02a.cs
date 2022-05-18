//
// ecx-rr-02a.cs
//
// Author:
//     Michael Tindal <urilith@gentoo.org>
//
// Copyright (C) 2005 Michael Tindal and the individuals listed on
// the ChangeLog entries.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Reflection;
using System.Collections;

using ECX.Core.Loader;
using ECX.Core;
using ECX.Core.Module;
using ECX.Core.Dependency;

using ECX.Core.Dependency.Resolver;

[assembly: AssemblyVersion ("1.0.*")]
[assembly: ModuleRole ("IECXTestRole1")]
[assembly: ModuleDependency("(## ecx-rr-02b)")]

namespace EternityChronicles.Tests.ECX.ecx_rr {
	public class ecx_rr_02a : IModule, IECXTestRole1 {
		public void ModuleEntry (object rawController) {
			ModuleController controller = (ModuleController)rawController;

			controller.RegisterNewRole("IECXTestRole2", typeof(IECXTestRole2), RegisterIECXTestRole2, UnregisterIECXTestRole2);
		}

		public void ModuleExit (object rawController) {
			ModuleController controller = (ModuleController)rawController;

			controller.UnregisterRole("IECXTestRole2");
		}

		protected IECXTestRole2 i;
		
		private static ecx_rr_02a _initiated;

		private static void ChangeInitiated(ecx_rr_02a init) {
			if(_initiated != null) {
				if(_initiated.i != null) {
					init.i = _initiated.i;
				}
			}
			_initiated = init;
		}

		public static void RegisterIECXTestRole2 (Assembly asm, Type type)
		{
			_initiated.i = (IECXTestRole2)asm.CreateInstance (type.ToString ());
		}
		
		public ecx_rr_02a() {
			ChangeInitiated(this);
		}

		public string CallIECXTestRole2 ()
		{
			return i.Message ();
		}
		
		public static void UnregisterIECXTestRole2 (Assembly asm)
		{
			return;
		}

		public string WriteMyself() {
			return CallIECXTestRole2 ();
		}
	}
}
