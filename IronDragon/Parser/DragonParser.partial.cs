using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using IronDragon.Expressions;
using IronDragon.Lexer;
using IronDragon.Runtime;
using Microsoft.Scripting;
using Optional;
using static IronDragon.Expressions.DragonExpression;
using static IronDragon.Expressions.ParallelAssignmentExpression;
using static System.Linq.Expressions.Expression;
using BlockExpression = IronDragon.Expressions.BlockExpression;

namespace IronDragon.Parser
{
    public partial class DragonParser
    {
        public static readonly LabelTarget ContinueTarget = Label("<dragon_continue>");

        public static readonly LabelTarget BreakTarget = Label("<dragon_break>");

        public static readonly LabelTarget RetryTarget = Label("<dragon_retry>");

        public static readonly LabelTarget ReturnTarget = Label(typeof(object), "<dragon_return>");
        internal static int NumberOfAnonymous;
        internal static int NumberOfAnonymousTypes;
        internal string[] Lines;

        internal string FileName { get; private set; }

        public bool IsLiteral { get; set; }

        private void ClearAnon()
        {
            NumberOfAnonymous = 0;
            NumberOfAnonymousTypes = 0;
        }

        public static DragonToken<T> S<T>(IToken token)
        {
            return token as DragonToken<T>;
        }

        public static BlockExpression CreateBlock(object body)
        {
            List<Expression> expressions;

            if (body is List<Expression>)
                expressions = (List<Expression>)body;
            else
                expressions = new List<Expression> { (Expression)body };

            if (expressions.Last().Type == typeof(void)) expressions.Add(Constant(null, typeof(object)));

            return new BlockExpression(expressions, new DragonScope());
        }

        public static BlockExpression EmptyBlock()
        {
            var expressions = new List<Expression> { Constant(null) };

            return new BlockExpression(expressions, new DragonScope());
        }

        private SyntaxErrorException BuildException(string message, DragonToken token, int? errorCode,
                                                    Severity severity)
        {
            var completeMessage = new StringBuilder();
            completeMessage.AppendFormat("{0}:", message);
            completeMessage.AppendLine();
            completeMessage.AppendFormat("  {0}", Lines[token.Span.Start.Line - 1]);
            completeMessage.AppendLine();
            if (token.Span.Start.Line == token.Span.End.Line)
            {
                var cs = token.Span.Start.Column - 1;
                var ce = token.Span.End.Column - 1;
                for (var i = 0; i < cs + 2; i++) completeMessage.Append(" ");

                for (var i = 0; i < ce - cs; i++) completeMessage.Append("~");

                completeMessage.AppendLine();
            }

            return new SyntaxErrorException(completeMessage.ToString(), FileName, token.Text,
                                            Lines[token.Span.Start.Line - 1], token.Span, errorCode ?? default,
                                            severity);
        }

        private static T VisitIf<T, U>(U value, Func<U, T> func)
        {
            return value != null ? func(value) : default;
        }


        private static object ChooseNode(params Option<object>[] chkMap)
        {
            return chkMap.First(o => o.HasValue).ValueOr(() => null);
        }

        private static Option<object> M<T, U>(U value, Func<U, T> func)
        {
            return value != null ? Option.Some<object>(func(value)) : Option.None<object>();
        }

        private static Expression ChooseLValue(Lvalue_variableContext variableLvalue,
                                               Lvalue_instance_refContext instanceRefLvalue,
                                               Lvalue_accessContext accessLvalue)
        {
            if (variableLvalue != null)
                return new LvalueVariableVisitor().VisitLvalue_variable(variableLvalue);
            if (instanceRefLvalue != null)
                return new LvalueInstanceRefVisitor().VisitLvalue_instance_ref(instanceRefLvalue);
            return accessLvalue != null ? new LvalueAccessVisitor().VisitLvalue_access(accessLvalue) : null;
        }

        private static BlockExpression B(params Expression[] expr)
        {
            return DragonBlock(expr);
        }

        private static T CV<T>(ITerminalNode node)
        {
            return ((DragonToken<T>)node.Symbol).Value;
        }

        private static double D(ITerminalNode node)
        {
            return CV<double>(node);
        }

        private static int I(ITerminalNode node)
        {
            return CV<int>(node);
        }

        private static string S(ITerminalNode node)
        {
            return CV<string>(node);
        }

        private static FunctionArgument DA(string name, Expression defaultValue, bool isVarArg, bool isFunction,
                                           bool isLiteral)
        {
            return new FunctionArgument(name, 0)
                   {
                       DefaultValue = defaultValue,
                       HasDefault = defaultValue != null,
                       IsVarArg = isVarArg,
                       IsFunction = isFunction,
                       IsLiteral = isLiteral
                   };
        }

        public static Expression[] Parse(string source, SourceUnit sourceUnit = null)
        {
            var fileName = "";

            switch (sourceUnit?.Kind)
            {
            case SourceCodeKind.File:
                fileName = sourceUnit.Path;
                break;
            case SourceCodeKind.InteractiveCode:
                fileName = "source string";
                break;
            case SourceCodeKind.Unspecified:
                fileName = "unspecified";
                break;
            case SourceCodeKind.Expression:
                fileName = "expression";
                break;
            case SourceCodeKind.Statements:
                fileName = "statements";
                break;
            case SourceCodeKind.SingleStatement:
                fileName = "statement";
                break;
            case SourceCodeKind.AutoDetect:
                fileName = sourceUnit.HasPath ? sourceUnit.Path : "source string";
                break;
            case null:
                fileName = "unknown";
                break;
            default:
                fileName = "source string";
                break;
            }

            var lexer = new DragonLexer(fileName, source);
            lexer.Scan();
            var stream = new CommonTokenStream(lexer.Queue);
            var parser = new DragonParser(stream);
            lexer.SetLines(parser);

            var errorListener = SyntaxErrorListener.Instance;
            errorListener.Parser = parser;

            parser.FileName = fileName;
            parser.RemoveErrorListeners();
            parser.AddErrorListener(errorListener);

            parser.ClearAnon();

            try
            {
                return new ProgramVisitor().VisitProgram(parser.program());
            }
            catch (RecognitionException e)
            {
                throw parser.BuildException(e.Message, (DragonToken)e.OffendingToken, e.OffendingState, Severity.Error);
            }
        }

        private sealed class SyntaxErrorListener : BaseErrorListener
        {
            public static readonly SyntaxErrorListener Instance = new();

            internal DragonParser Parser { private get; set; }

            public override void SyntaxError(IRecognizer recognizer, IToken offendingSymbol, int line,
                                             int charPositionInLine, string msg,
                                             RecognitionException e)
            {
                if (!(offendingSymbol is DragonToken))
                    throw e;
                throw Parser.BuildException(msg, (DragonToken)offendingSymbol, e?.OffendingState, Severity.Error);
            }
        }

        private sealed class ProgramVisitor : DragonParserBaseVisitor<Expression[]>
        {
            public override Expression[] VisitProgram(ProgramContext context)
            {
                if (context.top_level_statement() != null)
                    return new[]
                           { new TopLevelStatementVisitor().VisitTop_level_statement(context.top_level_statement()) };

                return new
                    BlockContentsVisitor().VisitBlock_contents(context.block_contents()).ToArray();
            }
        }

        private sealed class BlockContentsVisitor : DragonParserBaseVisitor<List<Expression>>
        {
            public override List<Expression> VisitBlock_contents(Block_contentsContext context)
            {
                var tlsv = new TopLevelStatementVisitor();

                return context.term_top_level_statement()
                              .Select(tls => tlsv.VisitTop_level_statement(tls.top_level_statement())).ToList();
            }
        }

        private sealed class TopLevelStatementVisitor : DragonParserBaseVisitor<Expression>
        {
            public override Expression VisitTop_level_statement(Top_level_statementContext context)
            {
                if (context.conditional_statement() != null)
                    return new ConditionalStatementVisitor()
                        .VisitConditional_statement(context.conditional_statement());

                return new StatementVisitor().VisitStatement(context.statement());
            }
        }

        private sealed class ConditionalStatementVisitor : DragonParserBaseVisitor<Expression>
        {
            public override Expression VisitConditional_statement(Conditional_statementContext context)
            {
                if (context.expression() == null)
                    return SwitchOp(new StatementVisitor().VisitStatement(context.statement()),
                                    new HashVisitor().VisitHash(context.hash()));
                var body = B(new StatementVisitor().VisitStatement(context.statement()));
                var test = new ExpressionVisitor().VisitExpression(context.expression());
                return (Expression)ChooseNode(
                                              M(context.IF(), o => IfThen(test, body)),
                                              M(context.WHILE(), o => While(test, body)),
                                              M(context.UNLESS(), o => UnlessThen(test, body)),
                                              M(context.UNTIL(), o => Until(test, body))
                                             );
            }
        }

        private sealed class StatementVisitor : DragonParserBaseVisitor<Expression>
        {
            public override Expression VisitStatement(StatementContext context)
            {
                if (context.parallel_assign_left() != null)
                    return ParallelAssign(
                                          new ParallelAssignLeftVisitor()
                                              .VisitParallel_assign_left(context.parallel_assign_left()),
                                          new ParallelAssignRightVisitor()
                                              .VisitParallel_assign_right(context.parallel_assign_right()));

                if (context.BREAK() != null) return Break(BreakTarget);

                if (context.RETRY() != null) return Continue(RetryTarget);

                if (context.CONTINUE() != null) return Continue(ContinueTarget);

                if (context.THROW() != null)
                    return Throw(new ExpressionVisitor().VisitExpression(context.expression()));

                return (Expression)ChooseNode(
                                              M(context.loop_construct(),
                                                o => new LoopConstructVisitor().VisitLoop_construct(o)),
                                              M(context.class_declaration(),
                                                o => new ClassDeclarationVisitor().VisitClass_declaration(o)),
                                              M(context.module_declaration(),
                                                o => new ModuleDeclarationVisitor().VisitModule_declaration(o)),
                                              M(context.switch_construct(),
                                                o => new SwitchConstructVisitor().VisitSwitch_construct(o)),
                                              M(context.for_construct(),
                                                o => new ForConstructVisitor().VisitFor_construct(o)),
                                              M(context.for_in_construct(),
                                                o => new ForInConstructVisitor().VisitFor_in_construct(o)),
                                              M(context.until_construct(),
                                                o => new UntilConstructVisitor().VisitUntil_construct(o)),
                                              M(context.do_until_construct(),
                                                o => new DoUntilConstructVisitor().VisitDo_until_construct(o)),
                                              M(context.while_construct(),
                                                o => new WhileConstructVisitor().VisitWhile_construct(o)),
                                              M(context.do_while_construct(),
                                                o => new DoWhileConstructVisitor().VisitDo_while_construct(o)),
                                              M(context.unless_construct(),
                                                o => new UnlessConstructVisitor().VisitUnless_construct(o)),
                                              M(context.if_else_construct(),
                                                o => new IfElseConstructVisitor().VisitIf_else_construct(o)),
                                              M(context.begin_construct(),
                                                o => new BeginConstructVisitor().VisitBegin_construct(o)),
                                              M(context.sync_construct(),
                                                o => new SyncConstructVisitor().VisitSync_construct(o)),
                                              M(context.puts_construct(),
                                                o => new PutsConstructVisitor().VisitPuts_construct(o)),
                                              M(context.alias(), o => new AliasVisitor().VisitAlias(o)),
                                              M(context.include(), o => new IncludeVisitor().VisitInclude(o)),
                                              M(context.return_expression(),
                                                o => new ReturnExpressionVisitor().VisitReturn_expression(o)),
                                              M(context.function_definition(),
                                                o => new FunctionDefinitionVisitor().VisitFunction_definition(o)),
                                              M(context.expression(), o => new ExpressionVisitor().VisitExpression(o))
                                             );
            }
        }

        private sealed class BlockVisitor : DragonParserBaseVisitor<BlockExpression>
        {
            public override BlockExpression VisitBlock(BlockContext context)
            {
                return B(
                         new BlockContentsVisitor().VisitBlock_contents(context.block_contents()).ToArray());
            }
        }

        private sealed class SwitchConstructVisitor : DragonParserBaseVisitor<Expression>
        {
            public override Expression VisitSwitch_construct(Switch_constructContext context)
            {
                var cbv = new CaseBlockVisitor();
                var caseBlocks = context.case_block().Select(cb => cbv.VisitCase_block(cb)).ToList();
                var defaultBlock = VisitIf(context.default_block(),
                                           o => new DefaultBlockVisitor().VisitDefault_block(o));

                return Switch(new ExpressionVisitor().VisitExpression(context.expression()),
                              defaultBlock, caseBlocks);
            }
        }

        private sealed class CaseBlockVisitor : DragonParserBaseVisitor<SwitchCase>
        {
            public override SwitchCase VisitCase_block(Case_blockContext context)
            {
                var ev = new ExpressionVisitor();
                var cases = context.expression().Select(e => ev.VisitExpression(e)).ToList();
                var body = new BlockVisitor().VisitBlock(context.block());

                return SwitchCase(body, cases);
            }
        }

        private sealed class DefaultBlockVisitor : DragonParserBaseVisitor<Expression>
        {
            public override Expression VisitDefault_block(Default_blockContext context)
            {
                return new BlockVisitor().VisitBlock(context.block());
            }
        }

        private sealed class LoopConstructVisitor : DragonParserBaseVisitor<Expression>
        {
            public override Expression VisitLoop_construct(Loop_constructContext context)
            {
                return DragonLoop(new BlockVisitor().VisitBlock(context.block()));
            }
        }

        private sealed class ForConstructVisitor : DragonParserBaseVisitor<Expression>
        {
            public override Expression VisitFor_construct(For_constructContext context)
            {
                var ev = new ExpressionVisitor();
                var init = ev.VisitExpression(context.expression(0));
                var test = ev.VisitExpression(context.expression(1));
                var step = ev.VisitExpression(context.expression(2));
                var body = VisitIf(context.block(), o => new BlockVisitor().VisitBlock(o));

                return For(init, test, step, body);
            }
        }

        private sealed class ForInConstructVisitor : DragonParserBaseVisitor<Expression>
        {
            public override Expression VisitFor_in_construct(For_in_constructContext context)
            {
                var identifier = context.IDENTIFIER().GetText();
                var range = new ExpressionVisitor().VisitExpression(context.expression());
                var body = new BlockVisitor().VisitBlock(context.block());

                return ForIn(identifier, range, body);
            }
        }

        private sealed class UntilConstructVisitor : DragonParserBaseVisitor<Expression>
        {
            public override Expression VisitUntil_construct(Until_constructContext context)
            {
                var test = new ExpressionVisitor().VisitExpression(context.expression());
                var body = VisitIf(context.block(), o => new BlockVisitor().VisitBlock(o));

                return Until(test, body);
            }
        }

        private sealed class DoUntilConstructVisitor : DragonParserBaseVisitor<Expression>
        {
            public override Expression VisitDo_until_construct(Do_until_constructContext context)
            {
                var test = new ExpressionVisitor().VisitExpression(context.expression());
                var body = VisitIf(context.block(), o => new BlockVisitor().VisitBlock(o));

                return DoUntil(test, body);
            }
        }

        private sealed class WhileConstructVisitor : DragonParserBaseVisitor<Expression>
        {
            public override Expression VisitWhile_construct(While_constructContext context)
            {
                var test = new ExpressionVisitor().VisitExpression(context.expression());
                var body = VisitIf(context.block(), o => new BlockVisitor().VisitBlock(o));

                return While(test, body);
            }
        }

        private sealed class DoWhileConstructVisitor : DragonParserBaseVisitor<Expression>
        {
            public override Expression VisitDo_while_construct(Do_while_constructContext context)
            {
                var test = new ExpressionVisitor().VisitExpression(context.expression());
                var body = VisitIf(context.block(), o => new BlockVisitor().VisitBlock(o));

                return DoWhile(test, body);
            }
        }

        private sealed class UnlessConstructVisitor : DragonParserBaseVisitor<Expression>
        {
            public override Expression VisitUnless_construct(Unless_constructContext context)
            {
                var test = new ExpressionVisitor().VisitExpression(context.expression());
                var body = new BlockVisitor().VisitBlock(context.block());
                var elsePart = VisitIf(context.else_part(), o => new ElsePartVisitor().VisitElse_part(o));

                return UnlessElse(test, body, elsePart);
            }
        }

        private sealed class IfElseConstructVisitor : DragonParserBaseVisitor<Expression>
        {
            public override Expression VisitIf_else_construct(If_else_constructContext context)
            {
                var test = new ExpressionVisitor().VisitExpression(context.expression());
                var body = new BlockVisitor().VisitBlock(context.block());
                var elsePart = VisitIf(context.else_part(), o => new ElsePartVisitor().VisitElse_part(o));

                return IfElse(test, body, elsePart);
            }
        }

        private sealed class ElsePartVisitor : DragonParserBaseVisitor<Expression>
        {
            public override Expression VisitElse_part(Else_partContext context)
            {
                return (Expression)ChooseNode(
                                              M(context.if_else_construct(),
                                                o => new IfElseConstructVisitor().VisitIf_else_construct(o)),
                                              M(context.unless_construct(),
                                                o => new UnlessConstructVisitor().VisitUnless_construct(o)),
                                              M(context.block(), o => new BlockVisitor().VisitBlock(o))
                                             );
            }
        }

        private sealed class ReturnExpressionVisitor : DragonParserBaseVisitor<Expression>
        {
            public override Expression VisitReturn_expression(Return_expressionContext context)
            {
                return Return(VisitIf(context.call_args(),
                                      o => new CallArgsVisitor().VisitCall_args(o)));
            }
        }

        private sealed class ExpressionVisitor : DragonParserBaseVisitor<Expression>
        {
            public override Expression VisitExpression(ExpressionContext context)
            {
                return (Expression)ChooseNode(
                                              M(context.assignment(), o => new AssignmentVisitor().VisitAssignment(o)),
                                              M(context.arg(), o => new ArgVisitor().VisitArg(o))
                                             );
            }
        }

        private sealed class AssignmentVisitor : DragonParserBaseVisitor<Expression>
        {
            public override Expression VisitAssignment(AssignmentContext context)
            {
                if (context.CONST() != null)
                {
                    var lvalue = LeftHandValue(new LvalueVisitor().VisitLvalue(context.lvalue()));
                    if (lvalue.Value is not VariableExpression)
                        throw new InvalidOperationException("const can only be used with variable lvalues.");
                    var value = new ExpressionVisitor().VisitExpression(context.expression());
                    var res = Assign(lvalue, value);
                    res.IsConst = true;
                    return res;
                }

                if (context.prefix_increment() != null || context.postfix_increment() != null)
                    return (Expression)ChooseNode(
                                                  M(context.prefix_increment(),
                                                    o => new PrefixIncrementVisitor().VisitPrefix_increment(o)),
                                                  M(context.postfix_increment(),
                                                    o => new PostfixIncrementVisitor().VisitPostfix_increment(o))
                                                 );

                var myType = (Enum)ChooseNode(
                                              M(context.ASSIGN(), o => ExpressionType.Assign),
                                              M(context.ADDASSIGN(), o => ExpressionType.AddAssign),
                                              M(context.SUBASSIGN(), o => ExpressionType.SubtractAssign),
                                              M(context.MULASSIGN(), o => ExpressionType.MultiplyAssign),
                                              M(context.DIVASSIGN(), o => ExpressionType.DivideAssign),
                                              M(context.MODASSIGN(), o => ExpressionType.ModuloAssign),
                                              M(context.SHLASSIGN(), o => ExpressionType.LeftShiftAssign),
                                              M(context.SHRASSIGN(), o => ExpressionType.RightShiftAssign),
                                              M(context.ANDASSIGN(), o => ExpressionType.AndAssign),
                                              M(context.ORASSIGN(), o => ExpressionType.OrAssign),
                                              M(context.XORASSIGN(), o => ExpressionType.ExclusiveOrAssign),
                                              M(context.EXPASSIGN(), o => ExpressionType.PowerAssign),
                                              M(context.CONDASSIGNO(), o => DragonExpressionType.IfNullAssign),
                                              M(context.CONDASSIGNA(), o => DragonExpressionType.IfNotNullAssign)
                                             );

                var lval = new LvalueVisitor().VisitLvalue(context.lvalue());
                var val = new ExpressionVisitor().VisitExpression(context.expression());

                switch (myType)
                {
                case DragonExpressionType _:
                    return ConditionalAssign(LeftHandValue(lval), val, (DragonExpressionType)myType);
                case ExpressionType _:
                    if (context.SET() != null)
                        return SetAssign(LeftHandValue(lval), val, (ExpressionType)myType);

                    return Assign(LeftHandValue(lval), val, (ExpressionType)myType);
                }

                return null;
            }
        }

        private sealed class PrefixIncrementVisitor : DragonParserBaseVisitor<Expression>
        {
            public override Expression VisitPrefix_increment(Prefix_incrementContext context)
            {
                var lvalue = new LvalueVisitor().VisitLvalue(context.lvalue());

                return (Expression)ChooseNode(
                                              M(context.INCREMENT(),
                                                o => Assign(LeftHandValue(lvalue), null,
                                                            ExpressionType.PreIncrementAssign)),
                                              M(context.DECREMENT(),
                                                o => Assign(LeftHandValue(lvalue), null,
                                                            ExpressionType.PreDecrementAssign))
                                             );
            }
        }

        private sealed class PostfixIncrementVisitor : DragonParserBaseVisitor<Expression>
        {
            public override Expression VisitPostfix_increment(Postfix_incrementContext context)
            {
                var lvalue = new LvalueVisitor().VisitLvalue(context.lvalue());

                return (Expression)ChooseNode(
                                              M(context.INCREMENT(),
                                                o => Assign(LeftHandValue(lvalue), null,
                                                            ExpressionType.PostIncrementAssign)),
                                              M(context.DECREMENT(),
                                                o => Assign(LeftHandValue(lvalue), null,
                                                            ExpressionType.PostDecrementAssign))
                                             );
            }
        }

        private sealed class ArgVisitor : DragonParserBaseVisitor<Expression>
        {
            public override Expression VisitArg(ArgContext context)
            {
                return (Expression)ChooseNode(
                                              M(context.prefix_op(), o => new PrefixOpVisitor().VisitPrefix_op(o)),
                                              M(context.postfix_op(), o => new PostfixOpVisitor().VisitPostfix_op(o)),
                                              M(context.op_expression(),
                                                o => new OpExpressionVisitor().VisitOp_expression(o))
                                             );
            }
        }

        private sealed class PrefixOpVisitor : DragonParserBaseVisitor<Expression>
        {
            public override Expression VisitPrefix_op(Prefix_opContext context)
            {
                return CallUnaryOp(Variable(
                                            InstanceRef(LeftHandValue(new OpExpressionVisitor().VisitOp_expression(context.op_expression())),
                                                        Constant(S(context.OP())))), false);
            }
        }

        private sealed class PostfixOpVisitor : DragonParserBaseVisitor<Expression>
        {
            public override Expression VisitPostfix_op(Postfix_opContext context)
            {
                return CallUnaryOp(Variable(
                                            InstanceRef(LeftHandValue(new OpExpressionVisitor().VisitOp_expression(context.op_expression())),
                                                        Constant(S(context.OP())))), true);
            }
        }

        private sealed class OpExpressionVisitor : DragonParserBaseVisitor<Expression>
        {
            public override Expression VisitOp_expression(Op_expressionContext context)
            {
                var pev = new PipeExpressionVisitor();
                var result = pev.VisitPipe_expression(context.pipe_expression(0));

                var i = 0;
                while (i < context.OP().Length)
                {
                    result = Call(
                                  Variable(InstanceRef(LeftHandValue(result), Constant(S(context.OP(i))))),
                                  new List<FunctionArgument>
                                  {
                                      new(null, pev.VisitPipe_expression(context.pipe_expression(i + 1)))
                                  });
                    i++;
                }

                return result;
            }
        }

        private sealed class PipeOpVisitor : DragonParserBaseVisitor<DragonExpressionType>
        {
            public override DragonExpressionType VisitPipe_op(Pipe_opContext context)
            {
                return (DragonExpressionType)ChooseNode(
                                                        M(context.BACKWARDPIPE(),
                                                          o => DragonExpressionType.BackwardPipe),
                                                        M(context.FORWARDPIPE(), o => DragonExpressionType.ForwardPipe)
                                                       );
            }
        }

        private sealed class PipeExpressionVisitor : DragonParserBaseVisitor<Expression>
        {
            public override Expression VisitPipe_expression(Pipe_expressionContext context)
            {
                var rev = new RangeExpressionVisitor();
                var pov = new PipeOpVisitor();
                var result = rev.VisitRange_expression(context.range_expression(0));

                var i = 0;
                while (i < context.pipe_op().Length)
                {
                    var pipeOp = pov.VisitPipe_op(context.pipe_op(i));
                    var right = rev.VisitRange_expression(context.range_expression(i + 1));
                    if (pipeOp == DragonExpressionType.BackwardPipe)
                        result = CallWithPipe(result, new List<FunctionArgument> { new(null, right) },
                                              pipeOp);
                    else
                        result = CallWithPipe(right, new List<FunctionArgument> { new(null, result) },
                                              pipeOp);
                    i++;
                }

                return result;
            }
        }

        private sealed class RangeOpVisitor : DragonParserBaseVisitor<bool>
        {
            public override bool VisitRange_op(Range_opContext context)
            {
                return context.IRANGE() != null ? true : false;
            }
        }

        private sealed class RangeExpressionVisitor : DragonParserBaseVisitor<Expression>
        {
            public override Expression VisitRange_expression(Range_expressionContext context)
            {
                var loev = new LogicalOrExpressionVisitor();
                var rov = new RangeOpVisitor();
                var result = loev.VisitLogical_or_expression(context.logical_or_expression(0));

                var i = 0;
                while (i < context.range_op().Length)
                {
                    var inclusive = rov.VisitRange_op(context.range_op(i));
                    var right = loev.VisitLogical_or_expression(context.logical_or_expression(i + 1));
                    result = Range(result, right, inclusive);
                    i++;
                }

                return result;
            }
        }

        private sealed class LogicalOrExpressionVisitor : DragonParserBaseVisitor<Expression>
        {
            public override Expression VisitLogical_or_expression(Logical_or_expressionContext context)
            {
                var lxev = new LogicalXorExpressionVisitor();
                var result = lxev.VisitLogical_xor_expression(context.logical_xor_expression(0));

                var i = 0;
                while (i < context.LOGICALOR().Length)
                {
                    result = Binary(result,
                                    lxev.VisitLogical_xor_expression(context.logical_xor_expression(i + 1)),
                                    ExpressionType.OrElse);
                    i++;
                }

                return result;
            }
        }

        private sealed class LogicalXorExpressionVisitor : DragonParserBaseVisitor<Expression>
        {
            public override Expression VisitLogical_xor_expression(Logical_xor_expressionContext context)
            {
                var laev = new LogicalAndExpressionVisitor();
                var result = laev.VisitLogical_and_expression(context.logical_and_expression(0));

                var i = 0;
                while (i < context.LOGICALXOR().Length)
                {
                    result = OrButNotAlso(result,
                                          laev.VisitLogical_and_expression(context.logical_and_expression(i + 1)));
                    i++;
                }

                return result;
            }
        }

        private sealed class LogicalAndExpressionVisitor : DragonParserBaseVisitor<Expression>
        {
            public override Expression VisitLogical_and_expression(Logical_and_expressionContext context)
            {
                var eev = new EqualityExpressionVisitor();
                var result = eev.VisitEquality_expression(context.equality_expression(0));

                var i = 0;
                while (i < context.LOGICALAND().Length)
                {
                    result = Binary(result,
                                    eev.VisitEquality_expression(context.equality_expression(i + 1)),
                                    ExpressionType.AndAlso);
                    i++;
                }

                return result;
            }
        }

        private sealed class EqualityOpEqNeqVisitor : DragonParserBaseVisitor<Enum>
        {
            public override Enum VisitEquality_op_eq_neq(Equality_op_eq_neqContext context)
            {
                return (Enum)ChooseNode(
                                        M(context.EQUAL(), o => ExpressionType.Equal),
                                        M(context.NOTEQUAL(), o => ExpressionType.NotEqual)
                                       );
            }
        }

        private sealed class EqualityOpVisitor : DragonParserBaseVisitor<Enum>
        {
            public override Enum VisitEquality_op(Equality_opContext context)
            {
                return (Enum)ChooseNode(
                                        M(context.equality_op_eq_neq(),
                                          o => new EqualityOpEqNeqVisitor().VisitEquality_op_eq_neq(o)),
                                        M(context.COMPARE(), o => DragonExpressionType.Compare),
                                        M(context.REGEXMATCH(), o => DragonExpressionType.Match),
                                        M(context.REGEXNOMATCH(), o => DragonExpressionType.NotMatch)
                                       );
            }
        }

        private sealed class EqualityExpressionVisitor : DragonParserBaseVisitor<Expression>
        {
            public override Expression VisitEquality_expression(Equality_expressionContext context)
            {
                var cev = new ComparisonExpressionVisitor();
                var eov = new EqualityOpVisitor();
                var result = cev.VisitComparison_expression(context.comparison_expression(0));

                var i = 0;
                while (i < context.equality_op().Length)
                {
                    var myType = eov.VisitEquality_op(context.equality_op(i));
                    switch (myType)
                    {
                    case ExpressionType e:
                        result = Binary(result,
                                        cev.VisitComparison_expression(context.comparison_expression(i + 1)), e);
                        break;
                    case DragonExpressionType n:
                        switch (n)
                        {
                        case DragonExpressionType.Compare:
                            result = Compare(result,
                                             cev.VisitComparison_expression(context.comparison_expression(i + 1)));
                            break;
                        case DragonExpressionType.Match:
                            result = DragonExpression.Match(result,
                                                            cev.VisitComparison_expression(context
                                                                .comparison_expression(i + 1)));
                            break;
                        case DragonExpressionType.NotMatch:
                            result = NotMatch(result,
                                              cev.VisitComparison_expression(context.comparison_expression(i + 1)));
                            break;
                        default:
                            throw new Exception($"unknown expression type {myType}");
                        }

                        break;
                    }

                    i++;
                }

                return result;
            }
        }

        private sealed class ComparisonOpVisitor : DragonParserBaseVisitor<ExpressionType>
        {
            public override ExpressionType VisitComparison_op(Comparison_opContext context)
            {
                return (ExpressionType)ChooseNode(
                                                  M(context.LESSTHANEQUAL(), o => ExpressionType.LessThanOrEqual),
                                                  M(context.LESSTHAN(), o => ExpressionType.LessThan),
                                                  M(context.GREATERTHANEQUAL(), o => ExpressionType.GreaterThanOrEqual),
                                                  M(context.GREATERTHAN(), o => ExpressionType.GreaterThan)
                                                 );
            }
        }

        private sealed class ComparisonExpressionVisitor : DragonParserBaseVisitor<Expression>
        {
            public override Expression VisitComparison_expression(Comparison_expressionContext context)
            {
                var boev = new BitwiseOrExpressionVisitor();
                var cov = new ComparisonOpVisitor();
                var result = boev.VisitBitwise_or_expression(context.bitwise_or_expression(0));

                var i = 0;
                while (i < context.comparison_op().Length)
                {
                    result = Binary(result,
                                    boev.VisitBitwise_or_expression(context.bitwise_or_expression(i + 1)),
                                    cov.VisitComparison_op(context.comparison_op(i)));
                    i++;
                }

                return result;
            }
        }

        private sealed class BitwiseOrOpVisitor : DragonParserBaseVisitor<ExpressionType>
        {
            public override ExpressionType VisitBitwise_or_op(Bitwise_or_opContext context)
            {
                return (ExpressionType)ChooseNode(M(context.BITWISEOR(), o => ExpressionType.Or),
                                                  M(context.BITWISEXOR(), o => ExpressionType.ExclusiveOr));
            }
        }

        private sealed class BitwiseOrExpressionVisitor : DragonParserBaseVisitor<Expression>
        {
            public override Expression VisitBitwise_or_expression(Bitwise_or_expressionContext context)
            {
                var baev = new BitwiseAndExpressionVisitor();
                var boov = new BitwiseOrOpVisitor();
                var result = baev.VisitBitwise_and_expression(context.bitwise_and_expression(0));

                var i = 0;
                while (i < context.bitwise_or_op().Length)
                {
                    result = Binary(result,
                                    baev.VisitBitwise_and_expression(context.bitwise_and_expression(i + 1)),
                                    boov.VisitBitwise_or_op(context.bitwise_or_op(i)));
                    i++;
                }

                return result;
            }
        }

        private sealed class BitwiseAndExpressionVisitor : DragonParserBaseVisitor<Expression>
        {
            public override Expression VisitBitwise_and_expression(Bitwise_and_expressionContext context)
            {
                var sev = new ShiftExpressionVisitor();
                var result = sev.VisitShift_expression(context.shift_expression(0));

                var i = 0;
                while (i < context.BITWISEAND().Length)
                {
                    result = Binary(result, sev.VisitShift_expression(context.shift_expression(i + 1)),
                                    ExpressionType.And);
                    i++;
                }

                return result;
            }
        }

        private sealed class ShiftOpVisitor : DragonParserBaseVisitor<ExpressionType>
        {
            public override ExpressionType VisitShift_op(Shift_opContext context)
            {
                return (ExpressionType)ChooseNode(
                                                  M(context.SHIFTLEFT(), o => ExpressionType.LeftShift),
                                                  M(context.SHIFTRIGHT(), o => ExpressionType.RightShift)
                                                 );
            }
        }

        private sealed class ShiftExpressionVisitor : DragonParserBaseVisitor<Expression>
        {
            public override Expression VisitShift_expression(Shift_expressionContext context)
            {
                var aev = new AdditiveExpressionVisitor();
                var sov = new ShiftOpVisitor();
                var result = aev.VisitAdditive_expression(context.additive_expression(0));

                var i = 0;
                while (i < context.shift_op().Length)
                {
                    result = Binary(result,
                                    aev.VisitAdditive_expression(context.additive_expression(i + 1)),
                                    sov.VisitShift_op(context.shift_op(i)));
                    i++;
                }

                return result;
            }
        }

        private sealed class AdditiveOpVisitor : DragonParserBaseVisitor<ExpressionType>
        {
            public override ExpressionType VisitAdditive_op(Additive_opContext context)
            {
                return (ExpressionType)ChooseNode(
                                                  M(context.PLUS(), o => ExpressionType.Add),
                                                  M(context.MINUS(), o => ExpressionType.Subtract)
                                                 );
            }
        }

        private sealed class AdditiveExpressionVisitor : DragonParserBaseVisitor<Expression>
        {
            public override Expression VisitAdditive_expression(Additive_expressionContext context)
            {
                var mev = new MultiplicativeExpressionVisitor();
                var aov = new AdditiveOpVisitor();
                var result = mev.VisitMultiplicative_expression(context.multiplicative_expression(0));

                if (context.multiplicative_expression().Length > 1)
                {
                    var i = 0;
                    while (i < context.additive_op().Length)
                    {
                        result = Binary(result,
                                        mev.VisitMultiplicative_expression(context.multiplicative_expression(i + 1)),
                                        aov.VisitAdditive_op(context.additive_op(i)));
                        i++;
                    }

                    return result;
                }

                if (context.additive_op(0) == null || context.additive_op().Length == 0) return result;
                {
                    var i = 0;
                    while (i < context.additive_op().Length)
                    {
                        var op = aov.VisitAdditive_op(context.additive_op(i));
                        if (op == ExpressionType.Subtract) result = Unary(result, ExpressionType.Negate);
                        i++;
                    }
                }


                return result;
            }
        }

        private sealed class MultiplicativeOpVisitor : DragonParserBaseVisitor<ExpressionType>
        {
            public override ExpressionType VisitMultiplicative_op(Multiplicative_opContext context)
            {
                return (ExpressionType)ChooseNode(M(context.MULTIPLY(), o => ExpressionType.Multiply),
                                                  M(context.DIVIDE(), o => ExpressionType.Divide),
                                                  M(context.MODULO(), o => ExpressionType.Modulo));
            }
        }

        private sealed class MultiplicativeExpressionVisitor : DragonParserBaseVisitor<Expression>
        {
            public override Expression VisitMultiplicative_expression(Multiplicative_expressionContext context)
            {
                var uv = new UnaryExpressionVisitor();
                var mov = new MultiplicativeOpVisitor();
                var result = uv.VisitUnary_expression(context.unary_expression(0));

                var i = 0;
                while (i < context.multiplicative_op().Length)
                {
                    result = Binary(result, uv.VisitUnary_expression(context.unary_expression(i + 1)),
                                    mov.VisitMultiplicative_op(context.multiplicative_op(i)));
                    i++;
                }

                return result;
            }
        }

        private sealed class UnaryExpressionVisitor : DragonParserBaseVisitor<Expression>
        {
            public override Expression VisitUnary_expression(Unary_expressionContext context)
            {
                var uv = new UnaryExpressionVisitor();
                if (context.NOT() != null && context.NOT().Length > 0)
                {
                    var result = uv.VisitUnary_expression(context.unary_expression());
                    for (var i = 0; i < context.NOT().Length; ++i) result = Unary(result, ExpressionType.Not);
                    return result;
                }

                if (context.BITWISEINVERSE() != null && context.BITWISEINVERSE().Length > 0)
                {
                    var result = uv.VisitUnary_expression(context.unary_expression());
                    for (var i = 0; i < context.BITWISEINVERSE().Length; ++i)
                        result = Unary(result, ExpressionType.OnesComplement);
                    return result;
                }

                string ChooseName(Unary_expressionContext _context)
                {
                    if (_context.symbol() != null)
                        return new SymbolVisitor().VisitSymbol(context.symbol()).Name;
                    return S(context.STRING());
                }

                return (Expression)ChooseNode(
                                              M(context.TYPEOF(),
                                                o => TypeOf(uv.VisitUnary_expression(context.unary_expression()))),
                                              M(context.YIELD(),
                                                o => Yield(context.call_args() != null
                                                               ? new CallArgsVisitor()
                                                                   .VisitCall_args(context.call_args())
                                                               : new List<FunctionArgument>())),
                                              M(context.UNDEF(), o => MethodChange(ChooseName(context), false)),
                                              M(context.REMOVE(), o => MethodChange(ChooseName(context), true)),
                                              M(context.power_expression(),
                                                o => new PowerExpressionVisitor().VisitPower_expression(o))
                                             );
            }
        }

        private sealed class PowerExpressionVisitor : DragonParserBaseVisitor<Expression>
        {
            public override Expression VisitPower_expression(Power_expressionContext context)
            {
                var atom = new AtomVisitor().VisitAtom(context.atom());
                return context.EXPONENT() != null
                           ? Binary(atom,
                                    new PowerExpressionVisitor().VisitPower_expression(context.power_expression()),
                                    ExpressionType.Power)
                           : atom;
            }
        }

        private sealed class AtomVisitor : DragonParserBaseVisitor<Expression>
        {
            public override Expression VisitAtom(AtomContext context)
            {
                return (Expression)ChooseNode(
                                              M(context.expression(), o => new ExpressionVisitor().VisitExpression(o)),
                                              M(context.primary(), o => new PrimaryVisitor().VisitPrimary(o))
                                             );
            }
        }

        private sealed class PrimaryRightSideParensVisitor : DragonParserBaseVisitor<List<FunctionArgument>>
        {
            public override List<FunctionArgument> VisitPrimary_right_side_parens(
                Primary_right_side_parensContext context)
            {
                var args = context.call_args() == null
                               ? new List<FunctionArgument>()
                               : new CallArgsVisitor().VisitCall_args(context.call_args());
                if (context.yield_block() != null)
                    args.Add(new FunctionArgument("__yieldBlock",
                                                  new YieldBlockVisitor().VisitYield_block(context.yield_block())));
                return args;
            }
        }

        private sealed class PrimaryRightSideAccessVisitor : DragonParserBaseVisitor<List<FunctionArgument>>
        {
            public override List<FunctionArgument> VisitPrimary_right_side_access(
                Primary_right_side_accessContext context)
            {
                return new CallArgsVisitor().VisitCall_args(context.call_args());
            }
        }

        private sealed class PrimaryRightSideYieldBlockVisitor : DragonParserBaseVisitor<FunctionArgument>
        {
            public override FunctionArgument VisitPrimary_right_side_yield_block(
                Primary_right_side_yield_blockContext context)
            {
                return new FunctionArgument("__yieldBlock",
                                            (Expression)
                                            ChooseNode(M(context.yield_block(), o => new YieldBlockVisitor().VisitYield_block(o)),
                                                       M(context.do_yield_block(),
                                                         o => new DoYieldBlockVisitor().VisitDo_yield_block(o))));
            }
        }

        private sealed class PrimaryRightSideParensAccessVisitor : DragonParserBaseVisitor<Expression>
        {
            internal Expression FunctionLValue { private get; set; }

            public override Expression VisitPrimary_right_side_parens_access(
                Primary_right_side_parens_accessContext context)
            {
                if (FunctionLValue == null)
                    throw new ArgumentNullException(nameof(FunctionLValue));

                if (context.primary_right_side_parens() != null)
                {
                    var args = new PrimaryRightSideParensVisitor().VisitPrimary_right_side_parens(context
                        .primary_right_side_parens());
                    return Call(FunctionLValue, args);
                }

                if (context.primary_right_side_access() != null)
                {
                    var args = new PrimaryRightSideAccessVisitor().VisitPrimary_right_side_access(context
                        .primary_right_side_access());
                    return Access(FunctionLValue, args);
                }

                return null;
            }
        }

        private sealed class PrimaryFunctionCallVisitor : DragonParserBaseVisitor<Expression>
        {
            internal Expression FunctionLValue { private get; set; }

            public override Expression VisitPrimary_function_call(Primary_function_callContext context)
            {
                if (FunctionLValue == null)
                    throw new ArgumentNullException(nameof(FunctionLValue));

                var result = FunctionLValue;

                if (context.primary_right_side_parens_access() == null) return result;
                var prspav = new PrimaryRightSideParensAccessVisitor();
                foreach (var prspa in context.primary_right_side_parens_access())
                {
                    prspav.FunctionLValue = result;
                    result = prspav.VisitPrimary_right_side_parens_access(prspa);
                }

                if (context.primary_right_side_yield_block() != null)
                {
                    var funcArgs = new List<FunctionArgument>
                                   {
                                       new PrimaryRightSideYieldBlockVisitor().VisitPrimary_right_side_yield_block(
                                        context.primary_right_side_yield_block())
                                   };
                    result = Call(result, funcArgs);
                }

                return result;
            }
        }

        private sealed class PrimaryLeftSideVisitor : DragonParserBaseVisitor<Expression>
        {
            public override Expression VisitPrimary_left_side(Primary_left_sideContext context)
            {
                return (Expression)ChooseNode(
                                              M(context.literal(), o => new LiteralVisitor().VisitLiteral(o)),
                                              M(context.lvalue_method_change(),
                                                o => new LvalueMethodChangeVisitor().VisitLvalue_method_change(o)),
                                              M(context.lvalue(), o => new LvalueVisitor().VisitLvalue(o)),
                                              M(context.array(), o => new ArrayVisitor().VisitArray(o)),
                                              M(context.hash(), o => new HashVisitor().VisitHash(o)),
                                              M(context.anonymous_function(),
                                                o => new AnonymousFunctionVisitor().VisitAnonymous_function(o)),
                                              M(context.anonymous_class_declaration(),
                                                o => new AnonymousClassDeclarationVisitor()
                                                    .VisitAnonymous_class_declaration(o))
                                             );
            }
        }

        private sealed class PrimaryVisitor : DragonParserBaseVisitor<Expression>
        {
            public override Expression VisitPrimary(PrimaryContext context)
            {
                var result = new PrimaryLeftSideVisitor().VisitPrimary_left_side(context.primary_left_side());

                return context.primary_function_call() == null
                           ? result
                           : new PrimaryFunctionCallVisitor { FunctionLValue = result }.VisitPrimary_function_call(
                            context.primary_function_call());
            }
        }

        private sealed class HashVisitor : DragonParserBaseVisitor<CreateDictionaryExpression>
        {
            public override CreateDictionaryExpression VisitHash(HashContext context)
            {
                if (context.COLON() != null)
                    return CreateDictionary(null);

                var hkv = new HashKeyVisitor();
                var av = new ArgVisitor();
                var keyValues = new List<Expression>();
                var i = 0;

                while (i < context.hash_key().Length)
                {
                    var key = hkv.VisitHash_key(context.hash_key(i));
                    var value = av.VisitArg(context.arg(i));
                    keyValues.Add(KeyValuePair(key, value));
                    ++i;
                }

                return CreateDictionary(keyValues);
            }
        }

        private sealed class SingleHashKeyVisitor : DragonParserBaseVisitor<Expression>
        {
            public override Expression VisitSingle_hash_key(Single_hash_keyContext context)
            {
                if (context.IDENTIFIER() != null)
                    return Constant(S(context.IDENTIFIER()));
                if (context.STRING() != null)
                    return Constant(S(context.STRING()));
                return context.symbol() != null ? Constant(new SymbolVisitor().VisitSymbol(context.symbol())) : null;
            }
        }

        private sealed class HashKeyVisitor : DragonParserBaseVisitor<Expression>
        {
            public override Expression VisitHash_key(Hash_keyContext context)
            {
                return new ArgVisitor().VisitArg(context.arg());
            }
        }

        private sealed class ArrayVisitor : DragonParserBaseVisitor<Expression>
        {
            public override Expression VisitArray(ArrayContext context)
            {
                if (context.BRACKETS() != null)
                    return CreateArray(new List<Expression>());

                var av = new ArgVisitor();
                return CreateArray(context.arg().Select(arg => av.VisitArg(arg)).ToList());
            }
        }

        private sealed class LiteralVisitor : DragonParserBaseVisitor<Expression>
        {
            public override Expression VisitLiteral(LiteralContext context)
            {
                Func<object, Expression> C = Constant;

                return (Expression)ChooseNode(
                                              M(context.NUMBER(), o => Number(D(o))),
                                              M(context.INTEGER(), o => Number(I(o))),
                                              M(context.STRING(), o => String(S(o))),
                                              M(context.REGEX(), o => Regex(S(o))),
                                              M(context.NIL(), o => C(null)),
                                              M(context.TRUE(), o => C(true)),
                                              M(context.FALSE(), o => C(false)),
                                              M(context.symbol(), o => Constant(new SymbolVisitor().VisitSymbol(o)))
                                             );
            }
        }

        private sealed class SymbolVisitor : DragonParserBaseVisitor<Symbol>
        {
            public override Symbol VisitSymbol(SymbolContext context)
            {
                string myName = null;
                if (context.IDENTIFIER() != null)
                    myName = S(context.IDENTIFIER());
                else if (context.STRING() != null) myName = S(context.STRING());
                if (myName == null)
                    throw new ArgumentNullException("Symbol name not provided.");

                return Symbol.NewSymbol(myName);
            }
        }

        private sealed class LvalueVisitor : DragonParserBaseVisitor<Expression>
        {
            public override Expression VisitLvalue(LvalueContext context)
            {
                return ChooseLValue(context.lvalue_variable(), context.lvalue_instance_ref(), context.lvalue_access());
            }
        }

        private sealed class LvalueVariableVisitor : DragonParserBaseVisitor<Expression>
        {
            public override Expression VisitLvalue_variable(Lvalue_variableContext context)
            {
                return new VariableVisitor().VisitVariable(context.variable());
            }
        }

        private sealed class LvalueInstanceRefOptVisitor : DragonParserBaseVisitor<Expression>
        {
            public override Expression VisitLvalue_instance_ref_opt(Lvalue_instance_ref_optContext context)
            {
                return (Expression)ChooseNode(
                                              M(context.CLASS(), o => Constant("class")),
                                              M(context.function_name(),
                                                o => Constant(new FunctionNameVisitor().VisitFunction_name(o)))
                                             );
            }
        }

        private sealed class LvalueInstanceRefVisitor : DragonParserBaseVisitor<Expression>
        {
            public override Expression VisitLvalue_instance_ref(Lvalue_instance_refContext context)
            {
                var lirov = new LvalueInstanceRefOptVisitor();
                var result = new VariableVisitor().VisitVariable(context.variable());

                return context.lvalue_instance_ref_opt().Aggregate(result,
                                                                   (current, opt) =>
                                                                       Variable(InstanceRef(current,
                                                                                    lirov
                                                                                        .VisitLvalue_instance_ref_opt(opt))));
            }
        }

        private sealed class LvalueMethodChangeVisitor : DragonParserBaseVisitor<Expression>
        {
            public override Expression VisitLvalue_method_change(Lvalue_method_changeContext context)
            {
                var name = "";
                if (context.symbol() != null)
                    name = new SymbolVisitor().VisitSymbol(context.symbol()).Name;
                else
                    name = S(context.STRING());
                return ObjectMethodChange(new VariableVisitor().VisitVariable(context.variable()),
                                          name, context.REMOVE() != null);
            }
        }

        private sealed class LvalueAccessVisitor : DragonParserBaseVisitor<Expression>
        {
            public override Expression VisitLvalue_access(Lvalue_accessContext context)
            {
                var cav = new CallArgsVisitor();
                var result = new VariableVisitor().VisitVariable(context.variable());

                return context.call_args()
                              .Aggregate(result, (current, cactx) => Access(current, cav.VisitCall_args(cactx)));
            }
        }

        private sealed class VariableVisitor : DragonParserBaseVisitor<Expression>
        {
            public override Expression VisitVariable(VariableContext context)
            {
                return (Expression)ChooseNode(
                                              M(context.IDENTIFIER(), o => Variable(Constant(S(o)))),
                                              M(context.SELF(), o => Variable(Constant("self"))),
                                              M(context.SUPER(), o => Variable(Constant("super"))),
                                              M(context.CONTEXT(), o => Variable(Constant("$:")))
                                             );
            }
        }

        private sealed class CallArgsVisitor : DragonParserBaseVisitor<List<FunctionArgument>>
        {
            public override List<FunctionArgument> VisitCall_args(Call_argsContext context)
            {
                var cav = new CallArgVisitor();

                return context.call_arg() == null
                           ? new List<FunctionArgument>()
                           : context.call_arg().Select(x => cav.VisitCall_arg(x)).ToList();
            }
        }

        private sealed class CallArgSingleHashVisitor : DragonParserBaseVisitor<FunctionArgument>
        {
            public override FunctionArgument VisitCall_arg_single_hash(Call_arg_single_hashContext context)
            {
                var key = new SingleHashKeyVisitor().VisitSingle_hash_key(context.single_hash_key());
                var value = new ArgVisitor().VisitArg(context.arg());
                var kvp = KeyValuePair(key, value);
                var cd = CreateDictionary(new List<Expression> { kvp });
                return new FunctionArgument(null, cd);
            }
        }

        private sealed class CallArgVisitor : DragonParserBaseVisitor<FunctionArgument>
        {
            public override FunctionArgument VisitCall_arg(Call_argContext context)
            {
                if (context.call_arg_single_hash() != null)
                    return new CallArgSingleHashVisitor().VisitCall_arg_single_hash(context.call_arg_single_hash());

                var argumentName = context.IDENTIFIER() != null ? S(context.IDENTIFIER()) : null;
                var value = new ArgVisitor().VisitArg(context.arg());

                return new FunctionArgument(argumentName, value);
            }
        }

        private sealed class FirstArgVisitor : DragonParserBaseVisitor<FunctionArgument>
        {
            public override FunctionArgument VisitFirst_arg(First_argContext context)
            {
                var isLiteral = context.COLON() != null;
                var argumentName = S(context.IDENTIFIER());
                var defaultValue = context.arg() != null ? new ArgVisitor().VisitArg(context.arg()) : null;

                if (isLiteral && defaultValue != null)
                    throw new SyntaxErrorException(
                                                   $"Argument {argumentName} is declared both literal and with a default value.");
                return DA(argumentName, defaultValue, false, false, isLiteral);
            }
        }

        private sealed class FirstVarArgVisitor : DragonParserBaseVisitor<FunctionArgument>
        {
            public override FunctionArgument VisitFirst_var_arg(First_var_argContext context)
            {
                return DA(S(context.IDENTIFIER()), null, true, false, false);
            }
        }

        private sealed class FirstBlockArgVisitor : DragonParserBaseVisitor<FunctionArgument>
        {
            public override FunctionArgument VisitFirst_block_arg(First_block_argContext context)
            {
                return DA(S(context.IDENTIFIER()), null, false, true, false);
            }
        }

        private sealed class EndVarArgVisitor : DragonParserBaseVisitor<FunctionArgument>
        {
            public override FunctionArgument VisitEnd_var_arg(End_var_argContext context)
            {
                return DA(S(context.IDENTIFIER()), null, true, false, false);
            }
        }

        private sealed class EndBlockArgVisitor : DragonParserBaseVisitor<FunctionArgument>
        {
            public override FunctionArgument VisitEnd_block_arg(End_block_argContext context)
            {
                return DA(S(context.IDENTIFIER()), null, false, true, false);
            }
        }

        private sealed class NextArgVisitor : DragonParserBaseVisitor<FunctionArgument>
        {
            public override FunctionArgument VisitNext_arg(Next_argContext context)
            {
                var isLiteral = context.COLON() != null;
                var argumentName = S(context.IDENTIFIER());
                var defaultValue = context.arg() != null ? new ArgVisitor().VisitArg(context.arg()) : null;

                if (isLiteral && defaultValue != null)
                    throw new SyntaxErrorException(
                                                   $"Argument {argumentName} is declared both literal and with a default value.");

                return DA(argumentName, defaultValue, false, false, isLiteral);
            }
        }

        private sealed class DefinitionArgumentListNoParenVisitor : DragonParserBaseVisitor<List<FunctionArgument>>
        {
            public override List<FunctionArgument> VisitDefinition_argument_list_no_paren(
                Definition_argument_list_no_parenContext context)
            {
                var args = new List<FunctionArgument>();

                if (context.first_var_arg() != null)
                {
                    args.Add(new FirstVarArgVisitor().VisitFirst_var_arg(context.first_var_arg()));
                    if (context.first_block_arg() != null)
                        args.Add(new FirstBlockArgVisitor().VisitFirst_block_arg(context.first_block_arg()));
                }
                else if (context.first_block_arg() != null)
                {
                    args.Add(new FirstBlockArgVisitor().VisitFirst_block_arg(context.first_block_arg()));
                }
                else if (context.first_arg() != null)
                {
                    args.Add(new FirstArgVisitor().VisitFirst_arg(context.first_arg()));
                    var nav = new NextArgVisitor();
                    args.AddRange(context.next_arg().Select(arg => nav.VisitNext_arg(arg)));
                    if (context.end_var_arg() != null)
                        args.Add(new EndVarArgVisitor().VisitEnd_var_arg(context.end_var_arg()));

                    if (context.end_block_arg() != null)
                        args.Add(new EndBlockArgVisitor().VisitEnd_block_arg(context.end_block_arg()));
                }

                return args;
            }
        }

        private sealed class DefinitionArgumentListVisitor : DragonParserBaseVisitor<List<FunctionArgument>>
        {
            public override List<FunctionArgument> VisitDefinition_argument_list(
                Definition_argument_listContext context)
            {
                return context.definition_argument_list_no_paren() == null
                           ? new List<FunctionArgument>()
                           : new DefinitionArgumentListNoParenVisitor()
                               .VisitDefinition_argument_list_no_paren(context.definition_argument_list_no_paren());
            }
        }

        private sealed class FunctionNameVisitor : DragonParserBaseVisitor<string>
        {
            public override string VisitFunction_name(Function_nameContext context)
            {
                string Z(params ITerminalNode[] nodes)
                {
                    return nodes.FirstOrDefault(x => x != null)?.GetText();
                }

                return Z(context.IDENTIFIER(), context.IRANGE(), context.ERANGE(), context.BITWISEOR(),
                         context.BITWISEXOR(), context.BITWISEAND(),
                         context.COMPARE(), context.EQUAL(), context.WHENCOMP(), context.REGEXMATCH(),
                         context.LESSTHAN(),
                         context.LESSTHANEQUAL(), context.GREATERTHAN(), context.GREATERTHANEQUAL(), context.PLUS(),
                         context.MINUS(), context.MULTIPLY(),
                         context.DIVIDE(), context.MODULO(), context.EXPONENT(), context.SHIFTLEFT(),
                         context.SHIFTRIGHT(),
                         context.INCREMENT(), context.DECREMENT(), context.UPLUS(), context.UMINUS(),
                         context.BRACKETS(),
                         context.BRACKETSASSIGN(), context.DCOLON(),
                         context.OP());
            }
        }

        private sealed class FunctionDefinitionVisitor : DragonParserBaseVisitor<Expression>
        {
            public override Expression VisitFunction_definition(Function_definitionContext context)
            {
                var name = new FunctionNameVisitor().VisitFunction_name(context.function_name());
                var block = new BlockVisitor().VisitBlock(context.block());
                if (context.definition_argument_list() != null)
                {
                    var args = new DefinitionArgumentListVisitor().VisitDefinition_argument_list(
                     context.definition_argument_list());
                    if (context.lvalue() != null)
                        return SingletonDefinition(
                                                   new LvalueVisitor().VisitLvalue(context.lvalue()),
                                                   name, args, block);

                    return FunctionDefinition(name, args, block);
                }

                if (context.lvalue() != null)
                    return SingletonDefinition(
                                               new LvalueVisitor().VisitLvalue(context.lvalue()), name,
                                               null, block);

                return FunctionDefinition(name, new List<FunctionArgument>(), block);
            }
        }

        private sealed class AnonymousFunctionVisitor : DragonParserBaseVisitor<Expression>
        {
            public override Expression VisitAnonymous_function(Anonymous_functionContext context)
            {
                var body = new BlockVisitor().VisitBlock(context.block());
                var args = context.definition_argument_list() != null
                               ? new DefinitionArgumentListVisitor().VisitDefinition_argument_list(
                                context.definition_argument_list())
                               : new List<FunctionArgument>();

                return FunctionDefinition($"dragon$anonFunc{NumberOfAnonymous++}", args, body);
            }
        }

        private sealed class YieldBlockVisitor : DragonParserBaseVisitor<Expression>
        {
            public override Expression VisitYield_block(Yield_blockContext context)
            {
                var body = B(new BlockContentsVisitor().VisitBlock_contents(context.block_contents()).ToArray());
                var args = context.definition_argument_list_no_paren() != null
                               ? new DefinitionArgumentListNoParenVisitor().VisitDefinition_argument_list_no_paren(
                                context.definition_argument_list_no_paren())
                               : new List<FunctionArgument>();

                return FunctionDefinition($"dragon$yieldBlock{NumberOfAnonymous++}", args, body);
            }
        }

        private sealed class DoYieldBlockVisitor : DragonParserBaseVisitor<Expression>
        {
            public override Expression VisitDo_yield_block(Do_yield_blockContext context)
            {
                var body = B(new BlockContentsVisitor().VisitBlock_contents(context.block_contents()).ToArray());
                var args = context.definition_argument_list_no_paren() != null
                               ? new DefinitionArgumentListNoParenVisitor().VisitDefinition_argument_list_no_paren(
                                context.definition_argument_list_no_paren())
                               : new List<FunctionArgument>();

                return FunctionDefinition($"dragon$yieldBlock{NumberOfAnonymous++}", args, body);
            }
        }

        private sealed class ParallelAssignLeftVisitor : DragonParserBaseVisitor<List<ParallelAssignmentInfo>>
        {
            public override List<ParallelAssignmentInfo> VisitParallel_assign_left(Parallel_assign_leftContext context)
            {
                var args = new List<ParallelAssignmentInfo>
                           {
                               new()
                               {
                                   Value = new LvalueVisitor().VisitLvalue(context.lvalue()),
                                   IsWildcard = context.MULTIPLY() != null
                               }
                           };

                if (context.parallel_assign_left_item() == null) return args;
                var paliv = new ParallelAssignLeftItemVisitor();
                args.AddRange(context.parallel_assign_left_item().Select(x => paliv.VisitParallel_assign_left_item(x)));

                return args;
            }
        }

        private sealed class ParallelAssignLeftItemVisitor : DragonParserBaseVisitor<ParallelAssignmentInfo>
        {
            public override ParallelAssignmentInfo VisitParallel_assign_left_item(
                Parallel_assign_left_itemContext context)
            {
                if (context.parallel_assign_left() != null)
                    return new ParallelAssignmentInfo
                           {
                               Value = new ParallelAssignLeftVisitor().VisitParallel_assign_left(
                                context.parallel_assign_left())
                           };

                return new ParallelAssignmentInfo
                       {
                           Value = new LvalueVisitor().VisitLvalue(context.lvalue()),
                           IsWildcard = context.MULTIPLY() != null
                       };
            }
        }

        private sealed class ParallelAssignRightSideItemVisitor : DragonParserBaseVisitor<ParallelAssignmentInfo>
        {
            public override ParallelAssignmentInfo VisitParallel_assign_right_item(
                Parallel_assign_right_itemContext context)
            {
                if (context.lvalue() != null)
                    return new ParallelAssignmentInfo
                           {
                               Value = new LvalueVisitor().VisitLvalue(context.lvalue()),
                               IsWildcard = context.MULTIPLY() != null
                           };

                return new ParallelAssignmentInfo
                       {
                           Value = new ExpressionVisitor().VisitExpression(context.expression())
                       };
            }
        }

        private sealed class ParallelAssignRightVisitor : DragonParserBaseVisitor<List<ParallelAssignmentInfo>>
        {
            public override List<ParallelAssignmentInfo> VisitParallel_assign_right(
                Parallel_assign_rightContext context)
            {
                var pariv = new ParallelAssignRightSideItemVisitor();
                return context.parallel_assign_right_item().Select(x => pariv.VisitParallel_assign_right_item(x))
                              .ToList();
                ;
                ;
            }
        }

        private sealed class AliasVisitor : DragonParserBaseVisitor<Expression>
        {
            public override Expression VisitAlias(AliasContext context)
            {
                var apv = new AliasPartVisitor();
                return Alias(apv.VisitAlias_part(context.alias_part(0)),
                             apv.VisitAlias_part(context.alias_part(1)));
            }
        }

        private sealed class AliasPartVisitor : DragonParserBaseVisitor<Expression>
        {
            public override Expression VisitAlias_part(Alias_partContext context)
            {
                return Constant(S(context.IDENTIFIER()));
            }
        }

        private sealed class IncludeVisitor : DragonParserBaseVisitor<Expression>
        {
            public override Expression VisitInclude(IncludeContext context)
            {
                return Include(context.IDENTIFIER().Select(S).ToList());
            }
        }

        private sealed class ClassDeclarationVisitor : DragonParserBaseVisitor<Expression>
        {
            public override Expression VisitClass_declaration(Class_declarationContext context)
            {
                var body = new BlockContentsVisitor().VisitBlock_contents(context.block_contents());
                if (context.SHIFTLEFT() != null)
                {
                    var target = new PrimaryVisitor().VisitPrimary(context.primary());
                    return ClassOpen(target, body);
                }

                var name = S(context.IDENTIFIER(0));
                var parent = context.IDENTIFIER().Length > 1 ? S(context.IDENTIFIER(1)) : null;
                return DefineClass(name, parent, body);
            }
        }

        private sealed class AnonymousClassDeclarationVisitor : DragonParserBaseVisitor<Expression>
        {
            public override Expression VisitAnonymous_class_declaration(Anonymous_class_declarationContext context)
            {
                var body = new BlockContentsVisitor().VisitBlock_contents(context.block_contents());
                var parent = context.IDENTIFIER() != null ? S(context.IDENTIFIER()) : null;
                return DefineClass($"dragon$anonType{NumberOfAnonymousTypes++}", parent, body);
            }
        }

        private sealed class ModuleDeclarationVisitor : DragonParserBaseVisitor<Expression>
        {
            public override Expression VisitModule_declaration(Module_declarationContext context)
            {
                var name = S(context.IDENTIFIER());
                var body = new BlockContentsVisitor().VisitBlock_contents(context.block_contents());
                return DefineModule(name, body);
            }
        }

        private sealed class BeginConstructVisitor : DragonParserBaseVisitor<Expression>
        {
            public override Expression VisitBegin_construct(Begin_constructContext context)
            {
                var tryBlock = new BlockVisitor().VisitBlock(context.block());
                var rbv = new RescueBlockVisitor();
                var rescueBlocks = context.rescue_block().Select(x => rbv.VisitRescue_block(x)).ToList();
                var elseBlock = context.rescue_else_block() != null
                                    ? new RescueElseBlockVisitor().VisitRescue_else_block(context.rescue_else_block())
                                    : null;
                var ensureBlock = context.rescue_ensure_block() != null
                                      ? new RescueEnsureBlockVisitor()
                                          .VisitRescue_ensure_block(context.rescue_ensure_block())
                                      : null;

                return Begin(tryBlock, rescueBlocks, ensureBlock, elseBlock);
            }
        }

        private sealed class RescueBlockVisitor : DragonParserBaseVisitor<Expression>
        {
            public override Expression VisitRescue_block(Rescue_blockContext context)
            {
                var varName = context.IDENTIFIER() != null ? S(context.IDENTIFIER()) : "$#";
                var block = new BlockVisitor().VisitBlock(context.block());
                var identifiers = context.MULTIPLY() != null
                                      ? null
                                      : new IdentifiersVisitor().VisitIdentifiers(context.identifiers());

                var rescueBlock = Rescue(identifiers, block,
                                         varName);
                if (context.MULTIPLY() != null) ((RescueExpression)rescueBlock).IsWildcard = true;
                return rescueBlock;
            }
        }

        private sealed class RescueElseBlockVisitor : DragonParserBaseVisitor<BlockExpression>
        {
            public override BlockExpression VisitRescue_else_block(Rescue_else_blockContext context)
            {
                return new BlockVisitor().VisitBlock(context.block());
            }
        }

        private sealed class RescueEnsureBlockVisitor : DragonParserBaseVisitor<BlockExpression>
        {
            public override BlockExpression VisitRescue_ensure_block(Rescue_ensure_blockContext context)
            {
                return new BlockVisitor().VisitBlock(context.block());
            }
        }

        private sealed class IdentifiersVisitor : DragonParserBaseVisitor<List<string>>
        {
            public override List<string> VisitIdentifiers(IdentifiersContext context)
            {
                return context.IDENTIFIER().Select(S).ToList();
            }
        }

        private sealed class SyncConstructVisitor : DragonParserBaseVisitor<Expression>
        {
            public override Expression VisitSync_construct(Sync_constructContext context)
            {
                return Sync(S(context.IDENTIFIER()), new BlockVisitor().VisitBlock(context.block()));
            }
        }

        private sealed class PutsConstructVisitor : DragonParserBaseVisitor<Expression>
        {
            public override Expression VisitPuts_construct(Puts_constructContext context)
            {
                return Puts(new ExpressionVisitor().VisitExpression(context.expression()));
            }
        }
    }
}