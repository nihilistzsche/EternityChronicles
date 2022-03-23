// RuntimeOperations.ExceptionSystem.cs in EternityChronicles/IronDragon
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
using System.Linq;
using System.Linq.Expressions;
using IronDragon.Builtins;
using IronDragon.Expressions;

namespace IronDragon.Runtime
{
    public static partial class RuntimeOperations
    {
        internal static dynamic Throw(Expression rawObjExpr, object rawScope)
        {
            var scope = rawScope as DragonScope ?? new DragonScope();
            var rawObj = CompilerServices.CompileExpression(rawObjExpr, scope);

            var obj = rawObj as DragonInstance;
            if (obj == null) throw new Exception();
            var instance = obj as DragonBoxedInstance;
            if (instance == null) throw new DragonSystemException(obj);
            var exc = instance.BoxedObject as Exception;
            if (exc != null) throw exc;
            throw new Exception();
        }

        internal static dynamic Begin(object rawTryExpression, List<Expression> rescueBlocksRaw,
                                      object rawEnsureBlock, object rawElseBlock, object rawScope)
        {
            var tryExpression = (Expression)rawTryExpression;
            var ensureBlock = (Expression)rawEnsureBlock;
            var elseBlock = (Expression)rawElseBlock;
            var scope = (DragonScope)rawScope;
            dynamic retVal = null;
            var exceptionRaised = false;
            var ensureRun = false;
            var rescueBlocks = new List<RescueExpression>();
            rescueBlocksRaw.ForEach(
                                    rawBlock =>
                                    {
                                        var block = rawBlock as RescueExpression;
                                        if (block != null) rescueBlocks.Add(block);
                                    });

            try
            {
                retVal = CompilerServices.CompileExpression(tryExpression, scope);
            }
            catch (Exception e)
            {
                var DragonException = e as DragonSystemException;
                var exType = DragonException != null ? DragonException.ExceptionClass.Name : e.GetType().Name;
                var found = false;
                exceptionRaised = true;
                foreach (var rescueBlock in rescueBlocks)
                {
                    var exceptionTypes = new List<string>();
                    if (!rescueBlock.IsWildcard)
                        foreach (var type in rescueBlock.ExceptionTypes)
                        {
                            var obj = Resolve(type, scope);
                            var instance = obj as DragonInstance;
                            if (instance != null)
                            {
                                exceptionTypes.Add(instance.Class.Name);
                            }
                            else
                            {
                                var @class = obj as DragonClass;
                                if (@class != null) exceptionTypes.Add(@class.Name);
                                var s = obj as string;
                                if (s != null) exceptionTypes.Add(s);
                                var ss = obj as DragonString;
                                if (ss != null) exceptionTypes.Add(ss);
                            }
                        }

                    var exMatches = rescueBlock.IsWildcard;
                    if (!exMatches)
                        if ((from type in exceptionTypes
                             select DragonTypeResolver.Resolve(type)
                             into _exType
                             where _exType != null
                             let __exType = DragonTypeResolver.Resolve(exType)
                             where (__exType != null && __exType.IsSubclassOf(_exType)) || __exType == _exType
                             select _exType).Any())
                            exMatches = true;
                    found = exMatches;
                    if (!found)
                    {
                        if (exceptionTypes.Contains(exType))
                            found = true;
                        else
                            continue;
                    }

                    var exception = e as DragonSystemException;
                    if (exception != null)
                        scope[rescueBlock.VarName] = exception.InnerObject;
                    else
                        scope[rescueBlock.VarName] = e;
                    try
                    {
                        retVal = CompilerServices.CompileExpression(rescueBlock.Body, scope);
                    }
                    catch (Exception)
                    {
                        if (ensureBlock == null) throw;
                        ensureRun = true;
                        CompilerServices.CompileExpression(ensureBlock, scope);
                        throw;
                    }

                    break;
                }

                if (!found) throw;
            }
            finally
            {
                if (!exceptionRaised && elseBlock != null) CompilerServices.CompileExpression(elseBlock, scope);
                if (!ensureRun && ensureBlock != null) CompilerServices.CompileExpression(ensureBlock, scope);
            }

            return retVal;
        }
    }
}