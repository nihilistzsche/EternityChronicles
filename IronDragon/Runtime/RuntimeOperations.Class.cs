// -----------------------------------------------------------------------
// <copyright file="RuntimeOperations.Class.cs" Company="Michael Tindal">
// Copyright 2011-2014 Michael Tindal
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
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using IronDragon.Expressions;
using IronDragon.Parser;

namespace IronDragon.Runtime {
    using E = ExpressionType;

    public class InstanceReference {
        public Expression LValue { get; set; }
        public string Key { get; set; }
    }

    public static partial class RuntimeOperations {
        private static bool _inClassDefine; // for nested class
        private static string _className;
        private static DragonScope _currentClassScope;

        private static readonly object _classDefineLock = new();

        internal static dynamic DefineClass(object rawName, object rawParent, List<Expression> contents, object rawScope) {
            lock (_classDefineLock) {
                if (Resolve(rawName, rawScope) != null) {
                    return DefineCategory(Resolve(rawName, rawScope), contents, rawScope);
                }
                var scope = (DragonScope) rawScope;
                var defineScope = _inClassDefine ? scope : scope.GlobalScope;
                _inClassDefine = true;
                DragonClass parent;
                if (rawParent == null) {
                    if (scope.GlobalScope["Object"] == null) {
                        scope.GlobalScope["Object"] = Dragon.Box(typeof (object));
                    }
                    parent = scope.GlobalScope["Object"];
                }
                else {
                    var dParent = Resolve(rawParent as string, scope);
                    if (dParent == null) {
                        _inClassDefine = false;
                        return null;
                    }
                    if (dParent is Type) {
                        parent = Dragon.Box(dParent);
                    }
                    else {
                        parent = dParent as DragonClass;
                    }
                    if (parent == null) {
                        _inClassDefine = false;
                        return null;
                    }
                }

                var name = (string) rawName;
                _className = name;

                var @class = new DragonClass {Name = _className, Parent = parent};
                var xScope = new DragonScope(scope);
                xScope["self"] = @class;
                xScope[_className] = @class;
                _currentClassScope = xScope;

                contents.ForEach(content => {
                    if (content is IncludeExpression) {
                        // We only include modules here so make sure this include references a module
                        var names = ((IncludeExpression) content).Names;

                        dynamic module = null;

                        var index = 0;
                        names.ForEach(mname => {
                            if ((module is DragonModule)) {
                                module = module.Context[mname];
                            }
                            else if (index == 0) {
                                module = scope[mname];
                            }
                            index = index + 1;
                        });

                        if (module != null) {
                            if (module is DragonModule) {
                                ((DragonModule) module).Contents.ForEach(mcon => {
                                    if (mcon is DragonFunction) {
                                        if ((mcon as DragonFunction).IsSingleton ||
                                            (mcon as DragonFunction).Name == "new") {
                                            DragonClass.AddMethod(@class.ClassMethods, mcon as DragonFunction);
                                        }
                                        else {
                                            DragonClass.AddMethod(@class.InstanceMethods, mcon as DragonFunction);
                                        }
                                        if (@class.RemovedMethods.Contains((mcon as DragonFunction).Name))
                                        {
                                            @class.RemovedMethods.Remove((mcon as DragonFunction).Name);
                                        }
                                        if (@class.UndefinedMethods.Contains((mcon as DragonFunction).Name))
                                        {
                                            @class.UndefinedMethods.Remove((mcon as DragonFunction).Name);
                                        }
                                    }
                                });

                                xScope.MergeWithScope(module.Context);
                            }
                            else if (module is DragonClass) {
                                xScope[((DragonClass) module).Name] = module;
                            }
                        }
                    }
                });

                contents.ForEach(content => {
                    if (!(content is IncludeExpression)) {
                        var result = CompilerServices.CompileExpression(content, xScope);
                        if (result is DragonFunction) {
                            if ((result as DragonFunction).IsSingleton || (result as DragonFunction).Name == "new") {
                                DragonClass.AddMethod(@class.ClassMethods, result as DragonFunction);
                            }
                            else {
                                DragonClass.AddMethod(@class.InstanceMethods, result as DragonFunction);
                            }
                            if (@class.RemovedMethods.Contains((result as DragonFunction).Name))
                            {
                                @class.RemovedMethods.Remove((result as DragonFunction).Name);
                            }
                            if (@class.UndefinedMethods.Contains((result as DragonFunction).Name))
                            {
                                @class.UndefinedMethods.Remove((result as DragonFunction).Name);
                            }
                        }
                    }
                });

                if (!@class.ClassMethods.ContainsKey("new")) {
                    DragonClass.AddMethod(@class.ClassMethods, new DragonFunction("new", new List<FunctionArgument>(),
                        DragonExpression.DragonBlock(
                            DragonExpression.Return(new List<FunctionArgument> {
                                new(null, DragonExpression.Variable(Expression.Constant("self")))
                            }),
                            Expression.Label(DragonParser.ReturnTarget, Expression.Constant(null, typeof (object)))),
                        new DragonScope()));
                }
                @class.Context = xScope;
                defineScope[@class.Name] = @class;
                _inClassDefine = false;
                return @class;
            }
        }


        internal static dynamic DefineCategory(DragonClass @class, List<Expression> contents, object rawScope) {
            lock (_classDefineLock) {
                var scope = (DragonScope) rawScope;
                _inClassDefine = true;
                _className = @class.Name;

                scope["self"] = @class;
                scope[_className] = @class;
                _currentClassScope = scope;

                contents.ForEach(content => {
                    if (content is IncludeExpression) {
                        // We only include modules here so make sure this include references a module
                        var names = ((IncludeExpression) content).Names;

                        dynamic module = null;

                        var index = 0;
                        names.ForEach(mname => {
                            if (module != null && (module is DragonModule)) {
                                module = module.Context[mname];
                            }
                            else if (index == 0) {
                                module = scope[mname];
                            }
                            index = index + 1;
                        });

                        if (module != null) {
                            if (module is DragonModule) {
                                ((DragonModule) module).Contents.ForEach(mcon => {
                                    if (mcon is DragonFunction) {
                                        if ((mcon as DragonFunction).IsSingleton ||
                                            (mcon as DragonFunction).Name == "new") {
                                            DragonClass.AddMethod(@class.ClassMethods, mcon as DragonFunction);
                                        }
                                        else {
                                            DragonClass.AddMethod(@class.InstanceMethods, mcon as DragonFunction);
                                        }
                                        if (@class.RemovedMethods.Contains((mcon as DragonFunction).Name))
                                        {
                                            @class.RemovedMethods.Remove((mcon as DragonFunction).Name);
                                        }
                                        if (@class.UndefinedMethods.Contains((mcon as DragonFunction).Name))
                                        {
                                            @class.UndefinedMethods.Remove((mcon as DragonFunction).Name);
                                        }
                                    }
                                });

                                scope.MergeWithScope(module.Context);
                            }
                            else if (module is DragonClass) {
                                scope[((DragonClass) module).Name] = module;
                            }
                        }
                    }
                });

                contents.ForEach(content => {
                    if (!(content is IncludeExpression)) {
                        var result = CompilerServices.CompileExpression(content, scope);
                        if (result is DragonFunction) {
                            if ((result as DragonFunction).IsSingleton) {
                                DragonClass.AddMethod(@class.ClassMethods, result as DragonFunction);
                            }
                            else {
                                DragonClass.AddMethod(@class.InstanceMethods, result as DragonFunction);
                            }
                            if (@class.RemovedMethods.Contains((result as DragonFunction).Name))
                            {
                                @class.RemovedMethods.Remove((result as DragonFunction).Name);
                            }
                            if (@class.UndefinedMethods.Contains((result as DragonFunction).Name))
                            {
                                @class.UndefinedMethods.Remove((result as DragonFunction).Name);
                            }
                        }
                    }
                });

                @class.Context.MergeWithScope(scope);
                return @class;
            }
        }

        internal static dynamic DefineClassOpen(object rawValue, List<Expression> contents, object rawScope)
        {
            var scope = (DragonScope) rawScope;

            var value = CompilerServices.CompileExpression((Expression) rawValue, scope);

            if (value == null)
                return null;

            var @class = value as DragonClass;
            if (@class != null)
            {
                return DefineCategory(@class, contents, scope);
            }
            var instance = value as DragonInstance;
            if (instance != null)
            {
                return DefineCategory(instance.Class, contents, scope);
            }
            var newVal = Dragon.Box(value);
            return DefineCategory(((DragonInstance)newVal).Class, contents, scope);
        }
  
        internal static dynamic DefineModule(object rawName, List<Expression> contents, object rawScope) {
            lock (_classDefineLock) {
                var scope = (DragonScope) rawScope;
                var defineScope = _inClassDefine ? scope : scope.GlobalScope;

                var name = (string) rawName;

                var xScope = new DragonScope(scope);

                var module = new DragonModule {Name = name, Context = scope};

                contents.ForEach(content => module.Contents.Add(CompilerServices.CompileExpression(content, xScope)));

                defineScope[module.Name] = module;

                return module;
            }
        }

        internal static dynamic InstanceRef(Expression lvalue, object key) {
            // Try to compile lvalue to check for type
            return new InstanceReference {LValue = lvalue, Key = (string) key};
        }

        internal static dynamic Include(List<string> names) {
            // this has a different meaning when were defining classes
            if (!_inClassDefine) {
                var s = new StringBuilder();
                names.ForEach(name => s.AppendFormat("{0}.", name));
                s.Remove(s.Length - 1, 1);
                DragonTypeResolver.Include(s.ToString());
                return null;
            }
            return null;
        }

        internal static dynamic Sync(object rawName, object rawBlock, object rawScope)
        {
            var varName = (string) rawName;
            var block = (Expression) rawBlock;
            var scope = (DragonScope) rawScope;

            var var = scope[varName];

            dynamic retVal = null;

            lock (var)
            {
                retVal = CompilerServices.CompileExpression(block, scope);
            }

            return retVal;
        }

        internal static dynamic ObjectMethodChange(object rawSelf, object rawName, bool isRemove, object rawScope)
        {
            var scope = (DragonScope) rawScope;
            var name = (string)rawName;

            var self = CompilerServices.CompileExpression((Expression) rawSelf, scope);

            var @class = self as DragonClass;
            if (@class != null)
            {
                if (isRemove)
                {
                    @class.RemovedMethods.Add(name);
                }
                else
                {
                    @class.UndefinedMethods.Add(name);
                }
            }
            else
            {
                var instance = self as DragonInstance;
                if (instance != null)
                {
                    if (isRemove)
                    {
                        instance.RemovedMethods.Add(name);
                    }
                    else
                    {
                        instance.UndefinedMethods.Add(name);
                    }
                }
                else
                {
                    var newVal = Dragon.Box(self);
                    if (isRemove)
                    {
                        ((DragonInstance)newVal).RemovedMethods.Add(name);
                    }
                    else
                    {
                        ((DragonInstance)newVal).UndefinedMethods.Add(name);
                    }
                }
            }


            return null;
        }

        internal static dynamic MethodChange(object rawName, bool isRemove)
        {
            if (!_inClassDefine)
            {
                return null;
            }

            var name = (string)rawName;


            DragonClass @class = _currentClassScope[_className];
            if (isRemove)
            {
                @class.RemovedMethods.Add(name);
            }
            else
            {
                @class.UndefinedMethods.Add(name);
            }

            return null;
        }

        internal static dynamic Switch(object rawTest, List<SwitchCase> cases, object rawDefaultBlock, object rawScope)
        {
            var test = (Expression) rawTest;
            var defaultBlock = rawDefaultBlock != null
                ? (Expression) rawDefaultBlock
                : DragonExpression.DragonBlock(Expression.Default(cases.First().Body.Type));
            var scope = (DragonScope) rawScope;

            dynamic retVal = null;

            var found = false;
            var tval = CompilerServices.CompileExpression(test, scope);
            foreach (var @case in cases.Where(@case => @case.TestValues.Select(testValue => CompilerServices.CompileExpression(testValue, scope))
                .Any(xval => Binary(tval, xval, E.Equal, scope))))
            {
                found = true;
                retVal = CompilerServices.CompileExpression(@case.Body, scope);
            }

            if (!found)
            {
                retVal = CompilerServices.CompileExpression(defaultBlock, scope);
            }

            return retVal;
        }
    }
}