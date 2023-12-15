// ClassTests.cs in EternityChronicles/EternityChronicles.Tests
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

using System;
using System.Collections.Generic;
using System.Text;
using IronDragon;
using IronDragon.Builtins;
using IronDragon.Runtime;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace EternityChronicles.Tests.IronDragon
{
    public class SVTestClass
    {
        protected int A;
        protected int B;

        public SVTestClass(int a, int b)
        {
            A = a;
            B = b;
        }

        public int Add(int c)
        {
            return A + B + c;
        }

        public int Mult()
        {
            return A * B;
        }

        [DragonExport("==")]
        public override bool Equals(object obj)
        {
            if (obj == this) return true;

            if (obj is not SVTestClass @class) return false;

            return A == @class.A && B == @class.B;
        }

        public override int GetHashCode()
        {
            return (A & B) ^ ((A | B) & (0xFA ^ ((A | B) & (A ^ B))));
        }
    }

    [TestFixture]
    public class ClassTests : DragonAbstractTestFixture
    {
        private class NativeHelper
        {
            internal readonly int FieldTest;

            public NativeHelper(int ft)
            {
                FieldTest = ft;
            }
        }

        [DragonExport("Export")]
        private class ExportHelper
        {
            [DragonExport("test_me")]
            public int TestMe(int x)
            {
                return x + 27;
            }

            [DragonDoNotExport]
            public int ShouldNotExport(int y)
            {
                return y - 10;
            }
        }

        [Test]
        public void TestArrayAdd()
        {
            var expect = new DragonArray { "hello" };

            Assert.That(CompileAndExecute("a = []; a << 'hello'; a;"), Is.EqualTo(expect));
        }

        [Test]
        public void TestBox()
        {
            var engine = GetRuntime().GetEngine("IronDragon");
            var defsource = engine.CreateScriptSourceFromString("ft.x = 15;");
            var defscope = engine.CreateScope();

            var ft = new NativeHelper(22);
            defscope.SetVariable("ft", ft);
            defsource.Execute(defscope);
            var ftx = Dragon.Box(ft);
            Assert.That(ftx.FieldTest + ftx.x, Is.EqualTo(37));
        }

        [Test]
        public void TestBoxAddInstanceMethod()
        {
            var ft = new NativeHelper(0);
            var ftx = Dragon.Box(ft);
            ftx.@class.add_me = CompileAndExecute("def add_me(num) { return num + @x; };");
            var ft2 = new NativeHelper(0);
            var ftx2 = Dragon.Box(ft2);
            ftx2.x = 15;
            Assert.That(ftx2.add_me(10), Is.EqualTo(25));
        }

        [Test]
        public void TestBoxAddSingleton()
        {
            var ft = new NativeHelper(0);
            var ftx = Dragon.Box(ft);
            ftx.y = CompileAndExecute("def y(num) { return num + @x; };");
            ftx.x = 15;
            Assert.That(ftx.y(10), Is.EqualTo(25));
        }

        [Test]
        public void TestBoxExtension()
        {
            DragonClass strb = Dragon.Box(typeof(StringBuilder));
            var engine = GetRuntime().GetEngine("IronDragon");
            var defsource =
                engine.CreateScriptSourceFromString(
                                                    "def AppendFromDragon() { self.Append('Appending from Dragon extension method!'); };");
            var defscope = engine.CreateScope();

            defsource.Execute(defscope);
            DragonClass.AddMethod(strb.InstanceMethods, defscope.GetVariable("AppendFromDragon"));

            var scope = engine.CreateScope();
            scope.SetVariable("StringBuilder", strb);
            var str = new StringBuilder();
            str.AppendFormat("{0} world! ", "Hello");
            var strx = Dragon.Box(str, new DragonScope(scope));
            strx.AppendFromDragon();
            Assert.That(str.ToString(),
                        Is.EqualTo("Hello world! Appending from Dragon extension method!"));
        }

        [Test]
        public void TestBoxExtensionIronRuby()
        {
            DragonClass strb = Dragon.Box(typeof(StringBuilder));
            var engine = GetRuntime().GetEngine("IronDragon");
            var defsource =
                engine.CreateScriptSourceFromString(
                                                    "def AppendFromDragon() { self.Append('Appending from Dragon extension method!'); };");
            var defscope = engine.CreateScope();

            defsource.Execute(defscope);
            DragonClass.AddMethod(strb.InstanceMethods, defscope.GetVariable("AppendFromDragon"));

            var scope = engine.CreateScope();
            scope.SetVariable("StringBuilder", strb);
            var str = new StringBuilder("Hello world! ");
            var strx = Dragon.Box(str, new DragonScope(scope));
            var rubyengine = GetRuntime().GetEngine("IronRuby");
            var rubysource =
                rubyengine.CreateScriptSourceFromString("strx.AppendFromDragon(); strx.ToString();");

            var rubyscope = rubyengine.CreateScope();
            rubyscope.SetVariable("strx", strx);
            Assert.That((string)rubysource.Execute(rubyscope),
                        Is.EqualTo("Hello world! Appending from Dragon extension method!"));
        }

        [Test]
        public void TestBoxInterface()
        {
            var iface = Dragon.Box(typeof(IComparable));
            var engine = GetRuntime().GetEngine("IronDragon");
            var source = engine.CreateScriptSourceFromString("x = IComparable();");
            var scope = engine.CreateScope();
            scope.SetVariable("IComparable", iface);
            object actual = source.Execute(scope);
            Assert.That(actual, Is.Null);
        }

        [Test]
        public void TestBoxUnderlyingObject()
        {
            var engine = GetRuntime().GetEngine("IronDragon");
            var defsource = engine.CreateScriptSourceFromString("ft.x = 15;");
            var defscope = engine.CreateScope();

            var ft = new NativeHelper(0);
            defscope.SetVariable("ft", ft);
            defsource.Execute(defscope);

            var ftx = Dragon.Box(ft);
            ftx.FieldTest = ftx.x + 10;
            Assert.That(ft.FieldTest, Is.EqualTo(25));
        }

        [Test]
        public void TestBoxWithoutDragonScope()
        {
            var ft = new NativeHelper(0);
            var ftx = Dragon.Box(ft);
            ftx.x = 15;
            ftx.FieldTest = ftx.x + 10;
            Assert.That(25, Is.EqualTo(ft.FieldTest));
        }

        [Test]
        public void TestCSharpClassCall()
        {
            var engine = GetRuntime().GetEngine("IronDragon");
            var source =
                engine.CreateScriptSourceFromString("str.AppendFormat('{0} world!','hello'); str.ToString();");

            var scope = engine.CreateScope();
            scope.SetVariable("str", new StringBuilder());
            Assert.That(source.Execute(scope), Is.EqualTo("hello world!"));
        }

        [Test]
        public void TestCSharpClassCallTwo()
        {
            var engine = GetRuntime().GetEngine("IronDragon");
            var source = engine.CreateScriptSourceFromString("str.AppendFormat('{0}','hello'); str;");

            var scope = engine.CreateScope();
            scope.SetVariable("str", new StringBuilder());
            var val = source.Execute(scope);
            val.Append(" world!");
            Assert.That(val.ToString(), Is.EqualTo("hello world!"));
        }

        [Test]
        public void TestCSharpClassExtend()
        {
            DragonClass strb = Dragon.Box(typeof(StringBuilder));
            var engine = GetRuntime().GetEngine("IronDragon");
            var defsource =
                engine.CreateScriptSourceFromString(
                                                    "def AppendFromDragon() { self.Append('Appending from Dragon extension method!'); };");
            var defscope = engine.CreateScope();

            defsource.Execute(defscope);
            DragonClass.AddMethod(strb.InstanceMethods, defscope.GetVariable("AppendFromDragon"));

            var source =
                engine.CreateScriptSourceFromString(
                                                    "str.AppendFormat('{0} world! ','hello'); str.AppendFromDragon(); str.ToString();");

            var scope = engine.CreateScope();
            scope.SetVariable("str", new StringBuilder());
            scope.SetVariable("StringBuilder", strb);
            Assert.That(source.Execute(scope),
                        Is.EqualTo("hello world! Appending from Dragon extension method!"));
        }

        [Test]
        public void TestCSharpContructorMap()
        {
            DragonClass strb = Dragon.Box(typeof(StringBuilder));
            var engine = GetRuntime().GetEngine("IronDragon");
            var defsource =
                engine.CreateScriptSourceFromString(
                                                    "str = StringBuilder('Hello '); str.Append('from Dragon!'); str.ToString();");
            var defscope = engine.CreateScope();
            defscope.SetVariable("StringBuilder", strb);
            Assert.That(defsource.Execute(defscope), Is.EqualTo("Hello from Dragon!"));
        }

        [Test]
        public void TestClassInheritCSharp()
        {
            var engine = GetRuntime().GetEngine("IronDragon");
            var source =
                engine.CreateScriptSourceFromString(
                                                    "class sv_subclass_cs < SVTestClass { }; x = sv_subclass_cs(15,10); x.Mult();");
            var scope = engine.CreateScope();
            scope.SetVariable("SVTestClass", Dragon.Box(typeof(SVTestClass)));
            Assert.That(source.Execute(scope), Is.EqualTo(150));
        }

        [Test]
        public void TestClassInheritCSharpTwo()
        {
            var engine = GetRuntime().GetEngine("IronDragon");
            var source =
                engine.CreateScriptSourceFromString(
                                                    "class sv_subclass_cs2 < SVTestClass { def new() { @A = 17; @B = 20; }; }; x = sv_subclass_cs2(); x.Mult();");
            var scope = engine.CreateScope();
            scope.SetVariable("SVTestClass", Dragon.Box(typeof(SVTestClass)));
            Assert.That(source.Execute(scope), Is.EqualTo(340));
        }

        [Test]
        public void TestClassInheritDragon()
        {
            Assert.That(
                        CompileAndExecute(
                                          "class sv_superclass { def print { return 'Hello world!'; }; }; class sv_subclass < sv_superclass { }; x = sv_subclass(); x.print();"),
                        Is.EqualTo("Hello world!"));
        }

        [Test]
        public void TestClassOverrideSimple()
        {
            Assert.That(
                        CompileAndExecute(
                                          "class sv_superclass0 { def print { return 'Hello world!'; }; }; class sv_subclass0 < sv_superclass0 { def print { return 'Hello you!'; }; }; x = sv_subclass0(); x.print();"),
                        Is.EqualTo("Hello you!"));
        }

        [Test]
        public void TestClassOverrideSuper()
        {
            Assert.That(
                        CompileAndExecute(
                                          "class sv_superclass1 { def print { return 'Hello me!'; }; }; class sv_subclass1 < sv_superclass1 { def print { return super.print(); }; }; x = sv_subclass1(); x.print();"),
                        Is.EqualTo("Hello me!"));
        }

        [Test]
        public void TestClassOverrideSuper2()
        {
            Assert.That(
                        CompileAndExecute(
                                          "class sv_superclass2 { def print0 { return 'Hello'; }; def print { return 'Goodbye'; }; }; class sv_subclass2 < sv_superclass2 { def print0 { return ' world!'; }; def print { x = ''; x << super.print0(); x << self.print0(); return x; }; }; x = sv_subclass2(); x.print();"),
                        Is.EqualTo("Hello world!"));
        }

        [Test]
        public void TestClassResolve()
        {
            Assert.That(CompileAndExecute("Math.Cos(90);"), Is.EqualTo(Math.Cos(90)));
        }

        [Test]
        public void TestClassResolveInclude()
        {
            Assert.That(
                        CompileAndExecute(
                                          "include System::Text; x = StringBuilder('hello'); x.Append(' world!'); x.ToString();"),
                        Is.EqualTo("hello world!"));
        }

        [Test]
        public void TestCustomOp()
        {
            var engine = GetRuntime().GetEngine("IronDragon");
            var source =
                engine.CreateScriptSourceFromString(
                                                    "def new(x) { @x = x; }; def <==>(other) { return ((@x + other.x) / (other.x/2)); };");

            var scope = engine.CreateScope();
            source.Execute(scope);

            var testClass = new DragonClass("Test", Dragon.Box(typeof(object)),
                                            new List<DragonFunction> { scope.GetVariable("new") },
                                            new List<DragonFunction> { scope.GetVariable("<==>") });
            scope.SetVariable("Test", testClass);
            scope.RemoveVariable("new");
            scope.RemoveVariable("<==>");
            source = engine.CreateScriptSourceFromString("x = Test(8); y = Test(8); x <==> y;");
            Assert.That(source.Execute(scope), Is.EqualTo(4));
        }

        [Test]
        public void TestDynamicIVarGetFromWrap()
        {
            var engine = GetRuntime().GetEngine("IronDragon");
            var defsource = engine.CreateScriptSourceFromString("ft.x = 15;");
            var defscope = engine.CreateScope();

            var ft = new NativeHelper(22);
            defscope.SetVariable("ft", ft);
            defsource.Execute(defscope);
            var ftx = Dragon.Box(ft);
            Assert.That(ft.FieldTest + ftx.x, Is.EqualTo(37));
        }

        [Test]
        public void TestDynamicIVarPersistance()
        {
            var engine = GetRuntime().GetEngine("IronDragon");
            var defsource = engine.CreateScriptSourceFromString("ft._fieldTest = 17; ft.x = 15;");
            var defscope = engine.CreateScope();

            defscope.SetVariable("ft", new NativeHelper(0));
            defsource.Execute(defscope);
            var ft = defscope.GetVariable("ft");
            ClassicAssert.IsInstanceOf(typeof(NativeHelper), ft);
            var source = engine.CreateScriptSourceFromString("ft.x + ft._fieldTest;");
            var scope = engine.CreateScope();
            scope.SetVariable("ft", ft);
            Assert.That(source.Execute(scope), Is.EqualTo(32));
        }

        [Test]
        public void TestDynamicWrapPartialFunction()
        {
            var x = new SVTestClass(10, 20);

            var dx = Dragon.Box(x);

            var add = dx.Add();

            Assert.That((int)add(10), Is.EqualTo(40));
        }

        [Test]
        public void TestInstanceVar()
        {
            var engine = GetRuntime().GetEngine("IronDragon");
            var defsource =
                engine.CreateScriptSourceFromString("ft._fieldTest = 17; ft.x = 10; ft.x + ft._fieldTest;");
            var defscope = engine.CreateScope();

            defscope.SetVariable("ft", new NativeHelper(0));
            Assert.That(defsource.Execute(defscope), Is.EqualTo(27));
        }

        [Test]
        public void TestInvokeMember()
        {
            var engine = GetRuntime().GetEngine("IronDragon");
            var source = engine.CreateScriptSourceFromString("def test(x) { return x + 10; };");

            var scope = engine.CreateScope();
            source.Execute(scope);

            var testClass = new DragonClass("Test", Dragon.Box(typeof(object)), new List<DragonFunction>(),
                                            new List<DragonFunction> { scope.GetVariable("test") });

            var nscope = engine.CreateScope();
            nscope.SetVariable("Test", testClass);

            var nsource = engine.CreateScriptSourceFromString("x = Test();  x.test(17);");
            Assert.That(nsource.Execute(nscope), Is.EqualTo(27));
        }

        [Test]
        public void TestInvokeMemberCSharp()
        {
            var engine = GetRuntime().GetEngine("IronDragon");
            var source = engine.CreateScriptSourceFromString("def test(x) { return x + 10; };");

            var scope = engine.CreateScope();
            source.Execute(scope);

            dynamic testClass = new DragonClass("Test", Dragon.Box(typeof(object)),
                                                new List<DragonFunction>(),
                                                new List<DragonFunction> { scope.GetVariable("test") });

            var x = testClass();

            Assert.That(x.test(17), Is.EqualTo(27));
        }

        [Test]
        public void TestInvokeMemberIronRuby()
        {
            var engine = GetRuntime().GetEngine("IronDragon");
            var source = engine.CreateScriptSourceFromString("def test(x) { return x + 10; };");

            var scope = engine.CreateScope();
            source.Execute(scope);

            dynamic testClass = new DragonClass("Test", Dragon.Box(typeof(object)),
                                                new List<DragonFunction>(),
                                                new List<DragonFunction> { scope.GetVariable("test") });

            scope.RemoveVariable("test");
            scope.SetVariable("TestClass", testClass);

            var rubyengine = GetRuntime().GetEngine("IronRuby");
            var rubysource = rubyengine.CreateScriptSourceFromString("x = dragon.TestClass; x.test(17);");

            var rubyscope = rubyengine.CreateScope();
            rubyscope.SetVariable("dragon", scope);

            Assert.That(rubysource.Execute(rubyscope), Is.EqualTo(27));
        }

        [Test]
        public void TestInvokeMemberIronRubyWithNewArg()
        {
            var engine = GetRuntime().GetEngine("IronDragon");
            var source =
                engine.CreateScriptSourceFromString("def new(z) { @y = z; }; def test(x) { return x + @y; };");

            var scope = engine.CreateScope();
            source.Execute(scope);

            dynamic testClass = new DragonClass("Test", Dragon.Box(typeof(object)),
                                                new List<DragonFunction> { scope.GetVariable("new") },
                                                new List<DragonFunction> { scope.GetVariable("test") });

            scope.RemoveVariable("new");
            scope.RemoveVariable("test");
            scope.SetVariable("TestClass", testClass);

            var rubyengine = GetRuntime().GetEngine("IronRuby");
            var rubysource = rubyengine.CreateScriptSourceFromString("x = dragon.TestClass(22); x.test(17);");

            var rubyscope = rubyengine.CreateScope();
            rubyscope.SetVariable("dragon", scope);

            Assert.That(rubysource.Execute(rubyscope), Is.EqualTo(39));
        }

        [Test]
        public void TestInvokeMemberMethodTable()
        {
            var engine = GetRuntime().GetEngine("IronDragon");
            var source = engine.CreateScriptSourceFromString("def test(x) { return x + 10; };");
            var source2 = engine.CreateScriptSourceFromString("def test(x,y) { return x + y; };");

            var scope = engine.CreateScope();
            source.Execute(scope);
            var scope2 = engine.CreateScope();
            source2.Execute(scope2);

            var testClass = new DragonClass("Test", Dragon.Box(typeof(object)), new List<DragonFunction>(),
                                            new List<DragonFunction>
                                            {
                                                scope.GetVariable("test"),
                                                scope2.GetVariable("test")
                                            });

            var nscope = engine.CreateScope();
            nscope.SetVariable("Test", testClass);

            var nsource = engine.CreateScriptSourceFromString("x = Test();  x.test(17) + x.test(5,10);");
            Assert.That(nsource.Execute(nscope), Is.EqualTo(42));
        }

        [Test]
        public void TestInvokeMemberPartialFunction()
        {
            var engine = GetRuntime().GetEngine("IronDragon");
            var source = engine.CreateScriptSourceFromString("def test(x,y) { return x + y; };");

            var scope = engine.CreateScope();
            source.Execute(scope);

            var testClass = new DragonClass("Test", Dragon.Box(typeof(object)), new List<DragonFunction>(),
                                            new List<DragonFunction> { scope.GetVariable("test") });

            var nscope = engine.CreateScope();
            nscope.SetVariable("Test", testClass);

            var nsource = engine.CreateScriptSourceFromString("x = Test();  pinv = x.test(10); pinv(17);");
            Assert.That(nsource.Execute(nscope), Is.EqualTo(27));
        }

        [Test]
        public void TestInvokeMemberPartialFunctionIVarTest()
        {
            var engine = GetRuntime().GetEngine("IronDragon");
            var source =
                engine.CreateScriptSourceFromString("def new(z) { @z = z; }; def test(x,y) { return x + y + @z; };");

            var scope = engine.CreateScope();
            source.Execute(scope);

            var testClass = new DragonClass("Test", Dragon.Box(typeof(object)),
                                            new List<DragonFunction> { scope.GetVariable("new") },
                                            new List<DragonFunction> { scope.GetVariable("test") });

            var nscope = engine.CreateScope();
            nscope.SetVariable("Test", testClass);

            var nsource = engine.CreateScriptSourceFromString("x = Test(10);  pinv = x.test(10); pinv(17);");
            Assert.That(nsource.Execute(nscope), Is.EqualTo(37));
        }

        [Test]
        public void TestInvokeMemberPartialFunctionIVarTestCSharp()
        {
            var engine = GetRuntime().GetEngine("IronDragon");
            var source =
                engine.CreateScriptSourceFromString("def new(z) { @z = z; }; def test(x,y) { return x + y + @z; };");

            var scope = engine.CreateScope();
            source.Execute(scope);

            dynamic testClass = new DragonClass("Test", Dragon.Box(typeof(object)),
                                                new List<DragonFunction> { scope.GetVariable("new") },
                                                new List<DragonFunction> { scope.GetVariable("test") });

            scope.RemoveVariable("new");
            scope.RemoveVariable("test");

            var x = testClass(10);
            var px = x.test(10);

            Assert.That(px(17), Is.EqualTo(37));
        }

        [Test]
        public void TestInvokeMemberPipe()
        {
            var engine = GetRuntime().GetEngine("IronDragon");
            var source = engine.CreateScriptSourceFromString("def test(x) { return x + 10; };");

            var scope = engine.CreateScope();
            source.Execute(scope);

            var testClass = new DragonClass("Test", Dragon.Box(typeof(object)), new List<DragonFunction>(),
                                            new List<DragonFunction> { scope.GetVariable("test") });

            var nscope = engine.CreateScope();
            nscope.SetVariable("Test", testClass);

            var nsource = engine.CreateScriptSourceFromString("x = Test();  17 |> x.test;");
            Assert.That(nsource.Execute(nscope), Is.EqualTo(27));
        }

        [Test]
        public void TestInvokeMemberSuper()
        {
            var engine = GetRuntime().GetEngine("IronDragon");
            var source = engine.CreateScriptSourceFromString("def test(x) { return x + 10; };");
            var source2 = engine.CreateScriptSourceFromString("def test(x) { super.test(x) + 15; };");

            var scope = engine.CreateScope();
            var scope2 = engine.CreateScope();
            source.Execute(scope);
            source2.Execute(scope2);

            var testClassBase = new DragonClass("TestBase", Dragon.Box(typeof(object)),
                                                new List<DragonFunction>(),
                                                new List<DragonFunction> { scope.GetVariable("test") });

            var testClass = new DragonClass("Test", testClassBase, new List<DragonFunction>(),
                                            new List<DragonFunction> { scope2.GetVariable("test") });

            var nscope = engine.CreateScope();
            nscope.SetVariable("TestBase", testClassBase);
            nscope.SetVariable("Test", testClass);

            var nsource = engine.CreateScriptSourceFromString("x = Test();  x.test(10);");
            Assert.That(nsource.Execute(nscope), Is.EqualTo(35));
        }

        [Test]
        public void TestInvokeMemberSuperCSharp()
        {
            var engine = GetRuntime().GetEngine("IronDragon");
            var source = engine.CreateScriptSourceFromString("def test(x) { return x + 10; };");
            var source2 = engine.CreateScriptSourceFromString("def test(x) { super.test(x) + 15; };");

            var scope = engine.CreateScope();
            var scope2 = engine.CreateScope();
            source.Execute(scope);
            source2.Execute(scope2);

            var testClassBase = new DragonClass("TestBase", Dragon.Box(typeof(object)),
                                                new List<DragonFunction>(),
                                                new List<DragonFunction> { scope.GetVariable("test") });

            dynamic testClass = new DragonClass("Test", testClassBase, new List<DragonFunction>(),
                                                new List<DragonFunction> { scope2.GetVariable("test") });

            var x = testClass();

            Assert.That((int)x.test(10), Is.EqualTo(35));
        }

        [Test]
        public void TestInvokeMemberSuperIronRuby()
        {
            var engine = GetRuntime().GetEngine("IronDragon");
            var source = engine.CreateScriptSourceFromString("def test(x) { return x + 10; };");
            var source2 = engine.CreateScriptSourceFromString("def test(x) { super.test(x) + 15; };");

            var scope = engine.CreateScope();
            var scope2 = engine.CreateScope();
            source.Execute(scope);
            source2.Execute(scope2);

            var testClassBase = new DragonClass("TestBase", Dragon.Box(typeof(object)),
                                                new List<DragonFunction>(),
                                                new List<DragonFunction> { scope.GetVariable("test") });

            dynamic testClass = new DragonClass("Test", testClassBase, new List<DragonFunction>(),
                                                new List<DragonFunction> { scope2.GetVariable("test") });

            var scope3 = engine.CreateScope();
            scope3.SetVariable("TestBase", testClassBase);
            scope3.SetVariable("Test", testClass);

            var rubyengine = GetRuntime().GetEngine("IronRuby");
            var rubysource = rubyengine.CreateScriptSourceFromString("x = dragon.Test; x.test(10);");

            var rubyscope = rubyengine.CreateScope();
            rubyscope.SetVariable("dragon", scope3);
            Assert.That((int)rubysource.Execute(rubyscope), Is.EqualTo(35));
        }

        [Test]
        public void TestInvokeMemberTwo()
        {
            var engine = GetRuntime().GetEngine("IronDragon");
            var source = engine.CreateScriptSourceFromString("def test(x) { return x + 10; };");

            var scope = engine.CreateScope();
            source.Execute(scope);

            var testClass = new DragonClass("Test", Dragon.Box(typeof(object)), new List<DragonFunction>(),
                                            new List<DragonFunction> { scope.GetVariable("test") });

            var nscope = engine.CreateScope();
            nscope.SetVariable("Test", testClass);

            var nsource = engine.CreateScriptSourceFromString("x = Test();  x.test(17);");
            Assert.That(nsource.Execute(nscope), Is.EqualTo(27));
        }

        [Test]
        public void TestModule()
        {
            Assert.That(
                        CompileAndExecute(
                                          "module TestModule { def testModuleFunc(x) { return x * 2; }; }; class ModuleTestClass { include TestModule; }; x = ModuleTestClass(); x.testModuleFunc(10);"),
                        Is.EqualTo(20));
        }

        [Test]
        public void TestSetGetMember()
        {
            var engine = GetRuntime().GetEngine("IronDragon");
            var source = engine.CreateScriptSourceFromString("def test(x) { return x + 10; };");

            var scope = engine.CreateScope();
            source.Execute(scope);

            var testClass = new DragonClass("Test", Dragon.Box(typeof(object)), new List<DragonFunction>(),
                                            new List<DragonFunction> { scope.GetVariable("test") });

            var nscope = engine.CreateScope();
            nscope.SetVariable("Test", testClass);

            var nsource = engine.CreateScriptSourceFromString("x = Test();  x.y = 10; x.test(x.y);");
            Assert.That(nsource.Execute(nscope), Is.EqualTo(20));
        }

        [Test]
        public void TestSetGetMemberInBody()
        {
            var engine = GetRuntime().GetEngine("IronDragon");
            var source =
                engine.CreateScriptSourceFromString("def test() { return @x + @y; }; def new() { @x = 12; @y = 10; };");

            var scope = engine.CreateScope();
            source.Execute(scope);

            var testClass = new DragonClass("Test", Dragon.Box(typeof(object)),
                                            new List<DragonFunction> { scope.GetVariable("new") },
                                            new List<DragonFunction> { scope.GetVariable("test") });

            var nscope = engine.CreateScope();
            nscope.SetVariable("Test", testClass);

            var nsource = engine.CreateScriptSourceFromString("x = Test();  x.test;");
            Assert.That(nsource.Execute(nscope), Is.EqualTo(22));
        }

        [Test]
        public void TestDragonDoNotExportAttribute()
        {
            DragonClass klass = Dragon.Box(typeof(ExportHelper));
            var engine = GetRuntime().GetEngine("IronDragon");
            var source = engine.CreateScriptSourceFromString("x = Export(); x.ShouldNotExport(10);");
            var scope = engine.CreateScope();
            scope.SetVariable(klass.Name, klass);

            object actual = source.Execute(scope);
            Assert.That(actual, Is.Null);
        }

        [Test]
        public void TestDragonExportAttribute()
        {
            DragonClass klass = Dragon.Box(typeof(ExportHelper));
            var engine = GetRuntime().GetEngine("IronDragon");
            var source = engine.CreateScriptSourceFromString("x = Export(); x.test_me(13);");
            var scope = engine.CreateScope();
            scope.SetVariable(klass.Name, klass);
            Assert.That(source.Execute(scope), Is.EqualTo(40));
        }

        [Test]
        public void TestDragonObjectBinary()
        {
            var engine = GetRuntime().GetEngine("IronDragon");
            var source = engine.CreateScriptSourceFromString("def +(x) { return @x + x; };");

            var scope = engine.CreateScope();
            source.Execute(scope);

            var testClass = new DragonClass("Test", Dragon.Box(typeof(object)), new List<DragonFunction>(),
                                            new List<DragonFunction> { scope.GetVariable("+") });

            var nscope = engine.CreateScope();
            nscope.SetVariable("Test", testClass);

            var nsource = engine.CreateScriptSourceFromString("x = Test();  x.x = 17; x + 10;");
            Assert.That(nsource.Execute(nscope), Is.EqualTo(27));
        }

        [Test]
        public void TestDragonObjectBinaryCSharp()
        {
            var engine = GetRuntime().GetEngine("IronDragon");
            var source = engine.CreateScriptSourceFromString("def +(x) { return @x + x; };");

            var scope = engine.CreateScope();
            source.Execute(scope);

            dynamic testClass = new DragonClass("Test", Dragon.Box(typeof(object)), new List<DragonFunction>(),
                                                new List<DragonFunction> { scope.GetVariable("+") });

            var x = testClass();
            x.x = 17;
            Assert.That(27, Is.EqualTo(x + 10));
        }

        [Test]
        public void TestDragonObjectBinaryIronRuby()
        {
            var engine = GetRuntime().GetEngine("IronDragon");
            var source = engine.CreateScriptSourceFromString("def +(x) { return @x + x; };");

            var scope = engine.CreateScope();
            source.Execute(scope);

            dynamic testClass = new DragonClass("Test", Dragon.Box(typeof(object)), new List<DragonFunction>(),
                                                new List<DragonFunction> { scope.GetVariable("+") });

            scope.RemoveVariable("+");
            scope.SetVariable("TestClass", testClass);

            var rubyengine = GetRuntime().GetEngine("IronRuby");
            var rubysource = rubyengine.CreateScriptSourceFromString("x = dragon.TestClass; x.x = 17; x + 10;");

            var rubyscope = rubyengine.CreateScope();
            rubyscope.SetVariable("dragon", scope);

            Assert.That(rubysource.Execute(rubyscope), Is.EqualTo(27));
        }

        [Test]
        public void TestSimpleClassDefinition()
        {
            Assert.That(
                        CompileAndExecute(
                                          "class sv_string { def print { return 'Hello world!'; }; }; x = sv_string(); x.print();"),
                        Is.EqualTo("Hello world!"));
        }

        [Test]
        public void TestSingleton()
        {
            var expect = new DragonArray { null, 5 };
            var engine = GetRuntime().GetEngine("IronDragon");
            var source = engine.CreateScriptSourceFromString("def test(x) { return x + 10; };");

            var scope = engine.CreateScope();
            source.Execute(scope);

            var testClass = new DragonClass("Test", Dragon.Box(typeof(object)), new List<DragonFunction>(),
                                            new List<DragonFunction> { scope.GetVariable("test") });

            var nscope = engine.CreateScope();
            nscope.SetVariable("Test", testClass);

            var nsource =
                engine.CreateScriptSourceFromString(
                                                    "x = Test();  y = Test(); def x.single(y) { return y - 5; }; [y.single(10),x.single(10)];");
            Assert.That(nsource.Execute(nscope), Is.EqualTo(expect));
        }

        [Test]
        public void TestSubModule()
        {
            var expect = new DragonArray { 25, 10 };
            Assert.That(
                        CompileAndExecute(
                                          "module TestModule2 { class ModuleClass2 { def testFunc(x) { return x * 5; }; }; class ModuleClass3 { def testFunc2(x) { return x/5; }; }; }; class ModuleTestClass2 { include TestModule2::ModuleClass2;  def new() { tmp = ModuleClass2(); @x = tmp.testFunc(5); @y = 0; begin { @y = ModuleClass3(); } rescue Exception => ex { @y = 10; }; }; }; x = ModuleTestClass2(); [x.x, x.y];"),
                        Is.EqualTo(expect));
        }

        [Test]
        public void TestUnaryCustomOp()
        {
            var expect = new DragonArray { 490, 10 };
            Assert.That(
                        CompileAndExecute(
                                          "class sv_custom_op_test { def new(x) { @x = x; }; def =!= { if (__postfix) { return @x * 7; } else { return @x / 7; }; }; }; x = sv_custom_op_test(70); [x =!=, =!= x];"),
                        Is.EqualTo(expect));
        }

        [Test]
        public void TestClassOpening()
        {
            Assert.That(
                        CompileAndExecute("foobar = []; class << foobar { def test { return 127; }; }; t = []; t.test;"),
                        Is.EqualTo(127));
        }

        [Test]
        public void TestCodeEval()
        {
            Assert.That(
                        CompileAndExecute(
                                          "def evalTest(s) { code = <<-CODE def test() { return 7; }; CODE; s.class_eval(code); }; class PropTest { evalTest(self()); }; x = PropTest(); x.test;"),
                        Is.EqualTo(7));
        }

        [Test]
        public void TestClassEval()
        {
            Assert.That(
                        CompileAndExecute(
                                          "class TestClassEval { def test() { return 7; }; }; x = TestClassEval(); x.class_eval('def test2() { return 25; };'); x.test2;"),
                        Is.EqualTo(25));
        }

        [Test]
        public void TestInstanceEval()
        {
            Assert.That(
                        CompileAndExecute(
                                          "class TestClassEval2 { def test() { @x = 17; }; }; x = TestClassEval2(); x.test; x.instance_eval('@x;');"),
                        Is.EqualTo(17));
        }

        [Test]
        public void TestUndefMethod1()
        {
            Assert.That(
                        CompileAndExecute(
                                          "class UndefMethodTest { def test { return 25; }; undef_method :test; }; x = UndefMethodTest(); begin { x.test; } rescue * { nil; };"),
                        Is.Null);
        }

        [Test]
        public void TestUndefMethod2()
        {
            Assert.That(
                        CompileAndExecute(
                                          "class UndefMethodTest2 { def test { return 25; }; }; class UndefMethodTest2 { undef_method :test; }; x = UndefMethodTest2(); begin { x.test; } rescue * { nil; };"),
                        Is.Null);
        }

        [Test]
        public void TestUndefMethod3()
        {
            Assert.That(
                        CompileAndExecute(
                                          "class UndefMethodSuperTest { def test { return 25; }; }; class UndefMethodSubTest < UndefMethodSuperTest { def test { return 50; }; undef_method :test; }; x = UndefMethodSubTest(); begin { x.test; } rescue * { nil; };"),
                        Is.Null);
        }

        [Test]
        public void TestRemoveMethod1()
        {
            Assert.That(
                        CompileAndExecute(
                                          "class RemoveMethodSuperTest { def test { return 25; }; }; class RemoveMethodSubTest < RemoveMethodSuperTest { def test { return 50; }; remove_method :test; }; x = RemoveMethodSubTest(); x.test;"),
                        Is.EqualTo(25));
        }

        [Test]
        public void TestRemoveMethod2()
        {
            Assert.That(
                        CompileAndExecute(
                                          "class RemoveMethodTest { def test { return 25; }; remove_method :test; }; x = RemoveMethodTest(); begin { x.test; } rescue * { nil; };"),
                        Is.Null);
        }

        [Test]
        public void TestRemoveMethod3()
        {
            Assert.That(
                        CompileAndExecute(
                                          "class RemoveMethodTest2 { def test { return 25; }; }; class RemoveMethodTest2 { remove_method :test; }; x = RemoveMethodTest2(); begin { x.test; } rescue * { nil; };"),
                        Is.Null);
        }

        [Test]
        public void TestUndefMethodRedef()
        {
            Assert.That(
                        CompileAndExecute(
                                          "class UndefMethodRedefTest { def test { return 10; }; undef_method :test; def test { return 25; }; }; x = UndefMethodRedefTest(); x.test;"),
                        Is.EqualTo(25)
                       );
        }

        [Test]
        public void TestRemoveMethodRedef()
        {
            Assert.That(
                        CompileAndExecute(
                                          "class RemoveMethodRedefTest { def test { return 10; }; remove_method :test; def test { return 25; }; }; x = RemoveMethodRedefTest(); x.test;"),
                        Is.EqualTo(25));
        }

        [Test]
        public void TestUndefMethodRedefRemove()
        {
            Assert.That(
                        CompileAndExecute(
                                          "class UndefMethodRedefSuperTest { def test { return 25; }; }; class UndefMethodRedefSubTest < UndefMethodRedefSuperTest { def test { return 50; }; undef_method :test; def test { return 10; }; remove_method :test; }; x = UndefMethodRedefSubTest(); x.test;"),
                        Is.EqualTo(25));
        }

        [Test]
        public void TestClassSingletonUndefMethod()
        {
            Assert.That(
                        CompileAndExecute(
                                          "class UndefSingletonTest { def test { return 25; }; }; UndefSingletonTest.undef_method :test; x = UndefSingletonTest(); begin { x.test; } rescue * { nil; };"),
                        Is.Null);
        }

        [Test]
        public void TestInstanceSingletonRemoveMethod()
        {
            var expect = new DragonArray { 25, null };

            Assert.That(
                        CompileAndExecute(
                                          "class RemoveMethodSingletonSuperTest { def test { return nil; }; }; class RemoveMethodSingletonSubTest < RemoveMethodSingletonSuperTest { def test { return 25; }; }; x = RemoveMethodSingletonSubTest(); y = RemoveMethodSingletonSubTest(); y.remove_method :test; [x.test, y.test];"),
                        Is.EqualTo(expect));
        }

        [Test]
        public void TestAnonymousType()
        {
            Assert.That(
                        CompileAndExecute("x = ({ def new { @x = 10; }; def add(num) { return @x + num; }; })(); x.add(20);"),
                        Is.EqualTo(30));
        }

        [Test]
        public void TestClassMacro()
        {
            Assert.That(
                        CompileAndExecute(
                                          "def funcMacro(n, r) { code = <<-CODE def testFunc#{n} { return '#{r}'; }; CODE; code; }; class MacroTest { self.class_eval(funcMacro('Test', 'Hello world!')); }; x = MacroTest(); x.testFuncTest();"),
                        Is.EqualTo("Hello world!"));
        }
    }
}
