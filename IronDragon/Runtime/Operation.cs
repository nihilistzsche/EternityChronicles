// Operation.cs in EternityChronicles/IronDragon
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
using System.Linq.Expressions;

namespace IronDragon.Runtime
{
    /// <summary>
    ///     TODO: Update summary.
    /// </summary>
    public static class Operation
    {
        public static Expression Resolve(Type expectedType, params Expression[] args)
        {
            return RuntimeOperations.Op("Resolve", expectedType, args);
        }

        public static Expression ResolveSymbol(Type expectedType, params Expression[] args)
        {
            return RuntimeOperations.Op("ResolveSymbol", expectedType, args);
        }

        public static Expression Assign(Type expectedType, params Expression[] args)
        {
            return RuntimeOperations.Op("Assign", expectedType, args);
        }

        public static Expression ConditionalAssign(Type expectedType, params Expression[] args)
        {
            return RuntimeOperations.Op("ConditionalAssign", expectedType, args);
        }

        public static Expression ParallelAssign(Type expectedType, params Expression[] args)
        {
            return RuntimeOperations.Op("ParallelAssign", expectedType, args);
        }

        public static Expression Binary(Type expectedType, params Expression[] args)
        {
            return RuntimeOperations.Op("Binary", expectedType, args);
        }

        public static Expression Compare(Type expectedType, params Expression[] args)
        {
            return RuntimeOperations.Op("Compare", expectedType, args);
        }

        public static Expression Match(Type expectedType, params Expression[] args)
        {
            return RuntimeOperations.Op("Match", expectedType, args);
        }

        public static Expression Unary(Type expectedType, params Expression[] args)
        {
            return RuntimeOperations.Op("Unary", expectedType, args);
        }

        public static Expression Boolean(Type expectedType, params Expression[] args)
        {
            return RuntimeOperations.Op("Boolean", expectedType, args);
        }

        public static Expression Convert(Type expectedType, params Expression[] args)
        {
            return RuntimeOperations.Op("Convert", expectedType, args);
        }

        public static Expression Access(Type expectedType, params Expression[] args)
        {
            return RuntimeOperations.Op("Access", expectedType, args);
        }

        public static Expression AccessSet(Type expectedType, params Expression[] args)
        {
            return RuntimeOperations.Op("AccessSet", expectedType, args);
        }

        public static Expression ConditionalAccessSet(Type expectedType, params Expression[] args)
        {
            return RuntimeOperations.Op("ConditionalAccessSet", expectedType, args);
        }

        public static Expression CreateArray(Type expectedType, params Expression[] args)
        {
            return RuntimeOperations.Op("CreateArray", expectedType, args);
        }

        public static Expression CreateDictionary(Type expectedType, params Expression[] args)
        {
            return RuntimeOperations.Op("CreateDictionary", expectedType, args);
        }

        public static Expression KeyValuePair(Type expectedType, params Expression[] args)
        {
            return RuntimeOperations.Op("KeyValuePair", expectedType, args);
        }

        public static Expression Define(Type expectedType, params Expression[] args)
        {
            return RuntimeOperations.Op("Define", expectedType, args);
        }

        public static Expression SingletonDefine(Type expectedType, params Expression[] args)
        {
            return RuntimeOperations.Op("SingletonDefine", expectedType, args);
        }

        public static Expression Call(Type expectedType, params Expression[] args)
        {
            return RuntimeOperations.Op("Call", expectedType, args);
        }

        public static Expression Yield(Type expectedType, params Expression[] args)
        {
            return RuntimeOperations.Op("Yield", expectedType, args);
        }

        public static Expression String(Type expectedType, params Expression[] args)
        {
            return RuntimeOperations.Op("String", expectedType, args);
        }

        public static Expression Regex(Type expectedType, params Expression[] args)
        {
            return RuntimeOperations.Op("Regex", expectedType, args);
        }

        public static Expression Number(Type expectedType, params Expression[] args)
        {
            return RuntimeOperations.Op("Number", expectedType, args);
        }

        public static Expression Invoke(Type expectedType, params Expression[] args)
        {
            return RuntimeOperations.Op("Invoke", expectedType, args);
        }

        public static Expression Eval(Type expectedType, params Expression[] args)
        {
            return RuntimeOperations.Op("Eval", expectedType, args);
        }

        public static Expression Alias(Type expectedType, params Expression[] args)
        {
            return RuntimeOperations.Op("Alias", expectedType, args);
        }

        public static Expression InstanceRef(Type expectedType, params Expression[] args)
        {
            return RuntimeOperations.Op("InstanceRef", expectedType, args);
        }

        public static Expression Typeof(Type expectedType, params Expression[] args)
        {
            return RuntimeOperations.Op("Typeof", expectedType, args);
        }

        public static Expression Include(Type expectedType, params Expression[] args)
        {
            return RuntimeOperations.Op("Include", expectedType, args);
        }

        public static Expression DefineClass(Type expectedType, params Expression[] args)
        {
            return RuntimeOperations.Op("DefineClass", expectedType, args);
        }

        public static Expression DefineClassOpen(Type expectedType, params Expression[] args)
        {
            return RuntimeOperations.Op("DefineClassOpen", expectedType, args);
        }

        public static Expression DefineModule(Type expectedType, params Expression[] args)
        {
            return RuntimeOperations.Op("DefineModule", expectedType, args);
        }

        public static Expression Throw(Type expectedType, params Expression[] args)
        {
            return RuntimeOperations.Op("Throw", expectedType, args);
        }

        public static Expression Begin(Type expectedType, params Expression[] args)
        {
            return RuntimeOperations.Op("Begin", expectedType, args);
        }

        public static Expression Sync(Type expectedType, params Expression[] args)
        {
            return RuntimeOperations.Op("Sync", expectedType, args);
        }

        public static Expression MethodChange(Type expectedType, params Expression[] args)
        {
            return RuntimeOperations.Op("MethodChange", expectedType, args);
        }

        public static Expression ObjectMethodChange(Type expectedType, params Expression[] args)
        {
            return RuntimeOperations.Op("ObjectMethodChange", expectedType, args);
        }

        public static Expression ConvertIfNumber(Type expectedType, params Expression[] args)
        {
            return RuntimeOperations.Op("ConvertIfNumber", expectedType, args);
        }

        public static Expression Switch(Type expectedType, params Expression[] args)
        {
            return RuntimeOperations.Op("Switch", expectedType, args);
        }

        public static Expression Range(Type expectedType, params Expression[] args)
        {
            return RuntimeOperations.Op("Range", expectedType, args);
        }

        public static Expression Require(Type expectedType, params Expression[] args)
        {
            return RuntimeOperations.Op("Require", expectedType, args);
        }
    }
}