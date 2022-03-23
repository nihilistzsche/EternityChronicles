// DragonExpression.Factory.cs in EternityChronicles/IronDragon
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
using IronDragon.Runtime;

namespace IronDragon.Expressions
{
    /// <summary>
    ///     Methods for DragonExpression that provide factory expressions similar to how DLR Expression.* functions work.
    /// </summary>
    public partial class DragonExpression
    {
        /// <summary>
        ///     Creates a new comparison expression.
        /// </summary>
        /// <param name="left">The left side of the comparison.</param>
        /// <param name="right">The right side of the comparison.</param>
        /// <returns>A new CompareExpression with the given left and right operands.</returns>
        public static BinaryExpression Compare(Expression left, Expression right)
        {
            return new BinaryExpression(left, right, DragonExpressionType.Compare);
        }

        public static BinaryExpression OrButNotAlso(Expression left, Expression right)
        {
            return new BinaryExpression(left, right, DragonExpressionType.LogicalXor);
        }

        public static BinaryExpression Match(Expression left, Expression right)
        {
            return new BinaryExpression(left, right, DragonExpressionType.Match);
        }

        public static BinaryExpression NotMatch(Expression left, Expression right)
        {
            return new BinaryExpression(left, right, DragonExpressionType.NotMatch);
        }

        public static AssignmentExpression Assign(LeftHandValueExpression left, Expression right)
        {
            return Assign(left, right, ExpressionType.Assign);
        }

        public static AssignmentExpression Assign(LeftHandValueExpression left, Expression right, ExpressionType type)
        {
            return new AssignmentExpression(left, right, type);
        }

        public static ConditionalAssignmentExpression ConditionalAssign(LeftHandValueExpression left, Expression right,
                                                                        DragonExpressionType conditionalAssignmentType)
        {
            return new ConditionalAssignmentExpression(left, right, conditionalAssignmentType);
        }

        public static ParallelAssignmentExpression ParallelAssign(
            List<ParallelAssignmentExpression.ParallelAssignmentInfo> lvalues,
            List<ParallelAssignmentExpression.ParallelAssignmentInfo> rvalues)
        {
            return new ParallelAssignmentExpression(lvalues, rvalues);
        }

        public static SetAssignExpression SetAssign(Expression left, Expression right, ExpressionType type)
        {
            return new SetAssignExpression(left, right, type);
        }

        public static BinaryExpression Binary(Expression left, Expression right, ExpressionType type)
        {
            return new BinaryExpression(left, right, type);
        }

        public static UnaryExpression Unary(Expression expr, ExpressionType type)
        {
            return new UnaryExpression(expr, type);
        }

        public static BooleanExpression Boolean(Expression expr)
        {
            return new BooleanExpression(expr);
        }

        public new static IfExpression IfThen(Expression test, Expression ifTrue)
        {
            Expression ifFalse = Block(Default(ifTrue.Type));

            return IfElse(test, ifTrue, ifFalse);
        }

        public static IfExpression IfElse(Expression test, Expression ifTrue, Expression ifFalse)
        {
            return new IfExpression(test, ifTrue, ifFalse);
        }

        public static UnlessExpression UnlessThen(Expression test, Expression ifTrue)
        {
            Expression ifFalse = Block(Default(ifTrue.Type));

            return UnlessElse(test, ifTrue, ifFalse);
        }

        public static UnlessExpression UnlessElse(Expression test, Expression ifTrue, Expression ifFalse)
        {
            return new UnlessExpression(test, ifTrue, ifFalse);
        }

        public static WhileExpression While(Expression test, Expression body)
        {
            return new WhileExpression(test, body);
        }

        public static DoWhileExpression DoWhile(Expression test, Expression body)
        {
            return new DoWhileExpression(test, body);
        }

        public static ForExpression For(Expression init, Expression test, Expression step, Expression body)
        {
            return new ForExpression(init, test, step, body);
        }

        public static ForInExpression ForIn(string varName, Expression enumerator, Expression body)
        {
            return new ForInExpression(varName, enumerator, body);
        }

        public static UntilExpression Until(Expression test, Expression body)
        {
            return new UntilExpression(test, body);
        }

        public static DoUntilExpression DoUntil(Expression test, Expression body)
        {
            return new DoUntilExpression(test, body);
        }

        public static LoopExpression DragonLoop(Expression body)
        {
            return new LoopExpression(body);
        }

        public static TypeofExpression TypeOf(Expression expr)
        {
            return new TypeofExpression(expr);
        }

        public static SwitchExpression Switch(Expression test, List<SwitchCase> cases)
        {
            return Switch(test, null, cases);
        }

        public static SwitchExpression Switch(Expression test, Expression @default, List<SwitchCase> cases)
        {
            return new SwitchExpression(test, @default, cases);
        }

        public new static ConvertExpression Convert(Expression expr, Type type)
        {
            return new ConvertExpression(expr, type);
        }

        public static PutsExpression Puts(Expression expr)
        {
            return new PutsExpression(expr);
        }

        public static BlockExpression DragonBlock(params Expression[] body)
        {
            return new BlockExpression(body.ToList(), new DragonScope());
        }

        public static LeftHandValueExpression LeftHandValue(Expression e)
        {
            return new LeftHandValueExpression(e);
        }

        public static VariableExpression Variable(Expression name)
        {
            return new VariableExpression(name);
        }

        public static VariableExpression Variable(Symbol sym)
        {
            return new VariableExpression(sym);
        }

        public static CreateArrayExpression CreateArray(List<Expression> values)
        {
            return new CreateArrayExpression(values);
        }

        public static CreateDictionaryExpression CreateDictionary(List<Expression> values)
        {
            return new CreateDictionaryExpression(values);
        }

        public static KeyValuePairExpression KeyValuePair(Expression key, Expression value)
        {
            return new KeyValuePairExpression(key, value);
        }

        public static AccessExpression Access(Expression container, List<FunctionArgument> args)
        {
            return new AccessExpression(container, args);
        }

        public static AccessSetExpression AccessSet(Expression container, List<FunctionArgument> args, Expression value)
        {
            return AccessSet(container, args, value, ExpressionType.Assign);
        }

        public static AccessSetExpression AccessSet(Expression container, List<FunctionArgument> args, Expression value,
                                                    ExpressionType extra)
        {
            return new AccessSetExpression(container, args, value, extra);
        }

        public static ConditionalAccessSetExpression ConditionalAccessSet(Expression container,
                                                                          List<FunctionArgument> args, Expression value,
                                                                          DragonExpressionType
                                                                              conditionalAssignmentType)
        {
            return new ConditionalAccessSetExpression(container, args, value, conditionalAssignmentType);
        }

        public static FunctionDefinitionExpression FunctionDefinition(string name, List<FunctionArgument> args,
                                                                      Expression body)
        {
            return new FunctionDefinitionExpression(name, args, body);
        }

        public static SingletonDefinitionExpression SingletonDefinition(Expression singleton, string name,
                                                                        List<FunctionArgument> args, Expression body)
        {
            return new SingletonDefinitionExpression(singleton, name, args, body);
        }

        public static Expression Call(Expression func, List<FunctionArgument> args)
        {
            return new FunctionCallExpression(func, args);
        }

        public static Expression CallWithPipe(Expression func, List<FunctionArgument> args,
                                              DragonExpressionType pipeType)
        {
            return new FunctionCallExpression(func, args, pipeType);
        }

        public static Expression CallUnaryOp(Expression func, bool isPostfix)
        {
            return new FunctionCallExpression(func, new List<FunctionArgument>(), true, isPostfix);
        }

        public static Expression Return(List<FunctionArgument> args)
        {
            return new ReturnExpression(args);
        }

        public static Expression Yield(List<FunctionArgument> args)
        {
            return new YieldExpression(args);
        }

        public static Expression String(string value)
        {
            return new StringExpression(value);
        }

        public static Expression Regex(string value)
        {
            return new RegexExpression(value);
        }

        public static Expression Number(object value)
        {
            return new NumberExpression(value);
        }

        public static Expression Invoke(Expression type, Expression method, List<FunctionArgument> args)
        {
            return new InvokeExpression(type, method, args);
        }

        public static Expression Alias(Expression from, Expression to)
        {
            return new AliasExpression(from, to);
        }

        public static Expression InstanceRef(Expression lvalue, Expression key)
        {
            return new InstanceReferenceExpression(lvalue, key);
        }

        public static Expression Include(List<string> names)
        {
            return new IncludeExpression(names);
        }

        public static Expression DefineClass(string name, string parent, List<Expression> contents)
        {
            return new ClassDefinitionExpression(name, parent, contents);
        }

        public static Expression ClassOpen(Expression name, List<Expression> contents)
        {
            return new ClassOpenExpression(name, contents);
        }

        public static Expression DefineModule(string name, List<Expression> contents)
        {
            return new ModuleDefinitionExpression(name, contents);
        }

        public static Expression Rescue(List<string> exceptionTypes, Expression body, string varName = "$#")
        {
            return new RescueExpression(exceptionTypes, body, varName);
        }

        public static Expression Begin(Expression tryBlock, List<Expression> rescueBlocks, Expression ensureBlock,
                                       Expression elseBlock)
        {
            return new BeginExpression(tryBlock, rescueBlocks, ensureBlock, elseBlock);
        }

        public new static Expression Throw(Expression exceptionObject)
        {
            return new ThrowExpression(exceptionObject);
        }

        public static Expression Sync(string varName, Expression body)
        {
            return new SyncExpression(varName, body);
        }

        public static Expression MethodChange(string varName, bool isRemove)
        {
            return new MethodChangeExpression(varName, isRemove);
        }

        public static Expression ObjectMethodChange(Expression lvalue, string varName, bool isRemove)
        {
            return new ObjectMethodChangeExpression(lvalue, varName, isRemove);
        }

        public static Expression SwitchOp(Expression test, Expression pairs)
        {
            var _pairs = (CreateDictionaryExpression)pairs;

            var _kvpList = _pairs.Values;

            var caseBlocks =
                (from KeyValuePairExpression _kvp in _kvpList select SwitchCase(Block(_kvp.Value), _kvp.Key)).ToList();

            var @default = Block(Default(caseBlocks.First().Body.Type));

            return Switch(Convert(test, caseBlocks.First().TestValues.First().Type), @default, caseBlocks);
        }

        public static Expression Range(Expression start, Expression end, bool inclusive)
        {
            return new RangeExpression(start, end, inclusive);
        }

        public static Expression Require(Expression value)
        {
            return new RequireExpression(value);
        }
    }
}