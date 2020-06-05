﻿using System;
using System.Collections.Generic;
using System.Linq;
using Traficante.TSQL.Parser.Lexing;
using Traficante.TSQL.Parser.Nodes;
using Traficante.TSQL.Parser.Tokens;

namespace Traficante.TSQL.Parser
{
    public class Parser
    {
        private static readonly TokenType[] SetOperators =
            {TokenType.Union, TokenType.UnionAll, TokenType.Except, TokenType.Intersect};

        private readonly Lexer _lexer;

        private readonly Dictionary<TokenType, (short Precendence, Associativity Associativity)> _precDict =
            new Dictionary<TokenType, (short Precendence, Associativity Associativity)>
            {
                {TokenType.Plus, (1, Associativity.Left)},
                {TokenType.Hyphen, (1, Associativity.Left)},
                {TokenType.Star, (2, Associativity.Left)},
                {TokenType.FSlash, (2, Associativity.Left)},
                {TokenType.Mod, (2, Associativity.Left)},
                {TokenType.Dot, (3, Associativity.Left)}
            };

        public Parser(Lexer lexer)
        {
            _lexer = lexer;
        }

        private Token Current => _lexer.Current();

        public RootNode ComposeAll()
        {
            _lexer.Next();
            var statements = new List<StatementNode>();
            while (Current.TokenType != TokenType.EndOfFile)
            {
                statements.Add(ComposeStatement());
            }

            return new RootNode(new StatementsArrayNode(statements.ToArray()));
        }

        public WhereNode ComposeWhereConditions()
        {
            return ComposeWhere(true);
        }

        public StatementNode ComposeStatement()
        {
            switch (Current.TokenType)
            {
                case TokenType.Desc:
                    return ComposeAndSkipIfPresent(p => new StatementNode(p.ComposeDesc()), TokenType.Semicolon);
                case TokenType.Select:
                    return ComposeAndSkipIfPresent(p => new StatementNode(p.ComposeSetOps(0)), TokenType.Semicolon);
                case TokenType.Insert:
                    return ComposeAndSkipIfPresent(p => new StatementNode(p.ComposeInsert()), TokenType.Semicolon);
                case TokenType.Update:
                    return ComposeAndSkipIfPresent(p => new StatementNode(p.ComposeUpdate()), TokenType.Semicolon);
                case TokenType.With:
                    return ComposeAndSkipIfPresent(p => new StatementNode(p.ComposeCteExpression()), TokenType.Semicolon);
                case TokenType.Table:
                    return ComposeAndSkipIfPresent(p => new StatementNode(p.ComposeTable()), TokenType.Semicolon);
                case TokenType.Declare:
                    return ComposeAndSkipIfPresent(p => new StatementNode(p.ComposeDeclare()), TokenType.Semicolon);
                case TokenType.Set:
                    return ComposeAndSkipIfPresent(p => new StatementNode(p.ComposeSet()), TokenType.Semicolon);
                case TokenType.Execute:
                    return ComposeAndSkipIfPresent(p => new StatementNode(p.ComposeExecute()), TokenType.Semicolon);
                case TokenType.Function:
                    return ComposeAndSkipIfPresent(p => new StatementNode(p.ComposeFunctionMethod(new string[0])), TokenType.Semicolon);
                default:
                    throw new TSQLException(
                        $"{Current.TokenType} cannot be used here.",  _lexer.GetLocation(Current.Span.Start));
                  
            }
        }

        private Node ComposeDesc()
        {
            Consume(Current.TokenType);

            var name = ComposeWord();

            FromNode fromNode;
            if(Current.TokenType == TokenType.Dot)
            {
                Consume(TokenType.Dot);

                if ((Current is FunctionToken func))
                {
                    var function = ComposeFunctionMethod(new string[1] { name.Value });

                    fromNode = new FromFunctionNode(function, string.Empty);
                    return new DescNode(fromNode, DescForType.SpecificConstructor);
                }
                else
                {
                    var table = new WordNode(ConsumeAndGetToken(TokenType.Identifier).Value);

                    fromNode = new FromTableNode(new TableNode(table.Value, new string[1] { name.Value }), string.Empty);
                    return new DescNode(fromNode, DescForType.Constructors);
                }
            }
            else
            {
                return new DescNode(new FromTableNode(new TableNode(string.Empty, new string[1] { name.Value }), string.Empty), DescForType.Schema);
            }
        }

        private DeclareNode ComposeDeclare()
        {
            Consume(TokenType.Declare);

            var variable = ConsumeAndGetToken(TokenType.Variable);
            var variableNode = new VariableNode(variable.Value);
            var typeNode = ComposeType();
            
            if (Current.TokenType == TokenType.Equality)
            {
                Consume(TokenType.Equality);
                var valueNode = ComposeBaseTypes();
                return new DeclareNode(variableNode, typeNode, valueNode);
            }

            return new DeclareNode(variableNode, typeNode);
        }

        private SetNode ComposeSet()
        {
            Consume(TokenType.Set);

            var variable = ConsumeAndGetToken(TokenType.Variable);
            var variableNode = new VariableNode(variable.Value);
            Consume(TokenType.Equality);
            var valueNode = ComposeBaseTypes();
            return new SetNode(variableNode, valueNode);
        }

        private Node ComposeExecute()
        {
            Consume(TokenType.Execute);

            VariableNode variableNode = null;
            if (Current.TokenType == TokenType.Variable)
            {
                variableNode = new VariableNode(ConsumeAndGetToken(TokenType.Variable).Value);
                Consume(TokenType.Equality);
            }

            List<string> path = ConsumeDottedIdentifiers();
            var args = ComposeExecuteArgs();
            var methodName = path.Last();
            var methodPath = path.Take(path.Count - 1);
            return new ExecuteNode(variableNode, new FunctionNode(methodName, args, methodPath.ToArray(), null));
        }

        private TypeNode ComposeType()
        {
            if (Current.TokenType == TokenType.Function)
            {
                var type = ConsumeAndGetToken(TokenType.Function);
                Consume(TokenType.LeftParenthesis);
                var size = ConsumeAndGetToken(TokenType.Integer);
                Consume(TokenType.RightParenthesis);
                var typeNode = new TypeNode(type.Value, long.Parse(size.Value));
                return typeNode;
            }
            if (Current.TokenType == TokenType.Identifier)
            {
                var type = ConsumeAndGetToken(TokenType.Identifier);
                var typeNode = new TypeNode(type.Value);
                return typeNode;
            }
            return null;
        }

        private CreateTableNode ComposeTable()
        {
            Consume(Current.TokenType);
            var tableName = Current.Value;
            Consume(TokenType.Identifier);
            Consume(TokenType.LBracket);

            var columns = new List<(string ColumnName, string TypeName)>();
            while (Current.TokenType != TokenType.RBracket)
            {
                var fieldName = Current.Value;
                Consume(TokenType.Identifier);
                var typeName = Current.Value;
                Consume(TokenType.Word);

                if (Current.TokenType == TokenType.Comma)
                    Consume(TokenType.Comma);

                columns.Add((fieldName, typeName));
            }

            Consume(Current.TokenType);

            return new CreateTableNode(tableName, columns.ToArray());
        }

        private CteExpressionNode ComposeCteExpression()
        {
            Consume(TokenType.With);

            var expressions = new List<CteInnerExpressionNode>();

            var col = ComposeBaseTypes() as IdentifierNode;
            Consume(TokenType.As);

            Consume(TokenType.LeftParenthesis);
            var innerSets = ComposeSetOps(0);
            expressions.Add(new CteInnerExpressionNode(innerSets, col.Name));
            Consume(TokenType.RightParenthesis);

            while (Current.TokenType == TokenType.Comma)
            {
                Consume(TokenType.Comma);

                col = ComposeBaseTypes() as IdentifierNode;
                Consume(TokenType.As);

                Consume(TokenType.LeftParenthesis);
                innerSets = ComposeSetOps(0);
                Consume(TokenType.RightParenthesis);
                expressions.Add(new CteInnerExpressionNode(innerSets, col.Name));
            }

            var outerSets = ComposeSetOps(0);

            return new CteExpressionNode(expressions.ToArray(), outerSets);
        }

        private Node ComposeSetOps(int nestingLevel)
        {
            var isSet = false;
            var query = ComposeQuery();
            Node node = query;
            while (IsSetOperator(Current.TokenType))
            {
                isSet = true;
                var setOperatorType = Current.TokenType;
                Consume(Current.TokenType);

                var keys = ComposeSetOperatorKeys();

                var currentLevel = nestingLevel;
                var nextSet = ComposeSetOps(currentLevel + 1);
                var isQuery = nextSet is QueryNode;
                switch (setOperatorType)
                {
                    case TokenType.Except:
                        node = new ExceptNode(string.Empty, keys, node, nextSet, currentLevel != 0, isQuery);
                        break;
                    case TokenType.Union:
                        node = new UnionNode(string.Empty, keys, node, nextSet, currentLevel != 0, isQuery);
                        break;
                    case TokenType.UnionAll:
                        node = new UnionAllNode(string.Empty, keys, node, nextSet, currentLevel != 0, isQuery);
                        break;
                    case TokenType.Intersect:
                        node = new IntersectNode(string.Empty, keys, node, nextSet, currentLevel != 0, isQuery);
                        break;
                }
            }

            return isSet || nestingLevel > 0 ? node : new SingleSetNode(query);
        }

        private Node ComposeInsert()
        {
            Consume(TokenType.Insert);
            ConsumeWhiteSpaces();
            Consume(TokenType.InTo);
            ConsumeWhiteSpaces();
            var tablePath = ConsumeDottedIdentifiers();
            var tableNode = new TableNode(tablePath.FirstOrDefault(), tablePath.Skip(1).ToArray());
            ConsumeWhiteSpaces();

            Consume(TokenType.LBracket);

            int index = 0;
            var fields = new List<FieldNode>();
            while (Current.TokenType != TokenType.RBracket)
            {
                var field = ConsumeField(index++);
                if (Current.TokenType == TokenType.Comma)
                    Consume(TokenType.Comma);
                fields.Add(field);
            }
            Consume(TokenType.RBracket);

            List<Node> values = new List<Node>();
            SelectNode selectNode = null;
            if (Current.TokenType == TokenType.Values)
            {
                Consume(TokenType.LBracket);
                while (Current.TokenType != TokenType.RBracket)
                {
                    values.Add(ComposeOperations());
                }
                Consume(TokenType.RBracket);
            }
            else if (Current.TokenType == TokenType.Select)
            {
                selectNode = ComposeSelectNode();
            }

            return new InsertNode(tableNode, fields.ToArray(), values.ToArray(), selectNode);
        }

        private Node ComposeUpdate()
        {
            return null;
        }

        private string[] ComposeSetOperatorKeys()
        {
            var keys = new List<string>();

            if (Current.TokenType != TokenType.LeftParenthesis) return keys.ToArray();

            Consume(TokenType.LeftParenthesis);
            var value = Current.Value;
            Consume(Current.TokenType);
            if (Current.TokenType == TokenType.Dot)
            {
                Consume(Current.TokenType);
                value = $"{value}.{Current.Value}";
                Consume(Current.TokenType);
            }
            keys.Add(value);
            while (Current.TokenType == TokenType.Comma)
            {
                Consume(TokenType.Comma);
                value = Current.Value;
                Consume(Current.TokenType);
                if (Current.TokenType == TokenType.Dot)
                {
                    Consume(Current.TokenType);
                    value = $"{value}.{Current.Value}";
                    Consume(Current.TokenType);
                }
                keys.Add(value);
            }

            Consume(TokenType.RightParenthesis);

            return keys.ToArray();
        }

        private static bool IsSetOperator(TokenType currentTokenType)
        {
            return SetOperators.Contains(currentTokenType);
        }

        private QueryNode ComposeQuery()
        {
            var selectNode = ComposeSelectNode();
            var fromNode = ComposeFrom();

            fromNode = ComposeJoin(fromNode);

            var whereNode = ComposeWhere(false);
            var groupBy = ComposeGrouByNode();
            var orderBy = ComposeOrderBy();
            var skip = ComposeSkip();
            var take = ComposeTake();
            return new QueryNode(selectNode, fromNode, whereNode, groupBy, orderBy, skip, take);
        }

        private FromNode ComposeJoin(FromNode from)
        {
            if (from == null)
                return null;

            if (IsJoinToken(Current.TokenType))
            {
                while (IsJoinToken(Current.TokenType))
                    switch (Current.TokenType)
                    {
                        case TokenType.InnerJoin:
                            Consume(TokenType.InnerJoin);
                            from = new JoinNode(from,
                                ComposeAndSkip(parser => parser.ComposeFrom(false), TokenType.On), ComposeOperations(),
                                JoinType.Inner);
                            break;
                        case TokenType.OuterJoin:
                            var outerToken = (OuterJoinToken) Current;
                            Consume(TokenType.OuterJoin);
                            from = new JoinNode(from,
                                ComposeAndSkip(parser => parser.ComposeFrom(false), TokenType.On), ComposeOperations(),
                                outerToken.Type == "left"
                                    ? JoinType.OuterLeft
                                    : JoinType.OuterRight);
                            break;
                    }
            }

            return new ExpressionFromNode(from);
        }

        private static bool IsJoinToken(TokenType currentTokenType)
        {
            return currentTokenType == TokenType.InnerJoin || currentTokenType == TokenType.OuterJoin;
        }

        private OrderByNode ComposeOrderBy()
        {
            if (Current.TokenType != TokenType.OrderBy) return null;

            Consume(TokenType.OrderBy);
            return new OrderByNode(ComposeOrderedFields());
        }

        private TakeNode ComposeTake()
        {
            if (Current.TokenType == TokenType.Take)
            {
                Consume(TokenType.Take);
                return new TakeNode(ComposeInteger());
            }

            return null;
        }

        private SkipNode ComposeSkip()
        {
            if (Current.TokenType == TokenType.Skip)
            {
                Consume(TokenType.Skip);
                return new SkipNode(ComposeInteger());
            }

            return null;
        }

        private TopNode ComposeTop()
        {
            if (Current.TokenType == TokenType.Top)
            {
                Consume(TokenType.Top);
                return new TopNode(ComposeInteger());
            }

            return null;
        }

        private GroupByNode ComposeGrouByNode()
        {
            if (Current.TokenType == TokenType.GroupBy)
            {
                Consume(TokenType.GroupBy);

                var fields = ComposeFields();

                if (Current.TokenType != TokenType.Having) return new GroupByNode(fields, null);

                Consume(TokenType.Having);
                
                var having = new HavingNode(ComposeOperations());

                return new GroupByNode(fields, having);
            }

            return null;
        }

        private SelectNode ComposeSelectNode()
        {
            Consume(TokenType.Select);
            ConsumeWhiteSpaces();

            var top = ComposeTop();

            var fields = ComposeFields();

            return new SelectNode(top, fields);
        }

        private FieldNode[] ComposeFields()
        {
            var fields = new List<FieldNode>();
            var i = 0;

            do
            {
                fields.Add(ConsumeField(i++));
            } while (!IsSetOperator(Current.TokenType) && Current.TokenType != TokenType.RightParenthesis &&
                     Current.TokenType != TokenType.From && Current.TokenType != TokenType.Having &&
                     Current.TokenType != TokenType.Skip && Current.TokenType != TokenType.Take &&
                     ConsumeAndGetToken().TokenType == TokenType.Comma);

            return fields.ToArray();
        }

        private FieldOrderedNode[] ComposeOrderedFields()
        {
            var fields = new List<FieldOrderedNode>();
            var i = 0;

            do
            {
                fields.Add(ConsumeFieldOrdered(i++));
            } while (!IsSetOperator(Current.TokenType) && Current.TokenType != TokenType.RightParenthesis &&
                     Current.TokenType != TokenType.Skip && Current.TokenType != TokenType.Take &&
                     ConsumeAndGetToken().TokenType == TokenType.Comma);

            return fields.ToArray();
        }

        private FieldNode ConsumeField(int order)
        {
            var fieldExpression = ComposeOperations();
            var alias = ComposeAlias();
            return new FieldNode(fieldExpression, order, alias);
        }

        private FieldOrderedNode ConsumeFieldOrdered(int level)
        {
            var fieldExpression = ComposeOperations();
            var order = ComposeOrder();
            return new FieldOrderedNode(fieldExpression, level, string.Empty, order);
        }

        private string ComposeAlias()
        {
            switch (Current.TokenType)
            {
                case TokenType.As:
                    Consume(TokenType.As);
                    var name = Current.Value;
                    Consume(Current.TokenType);
                    return name;
                case TokenType.Word:
                    return ConsumeAndGetToken(TokenType.Word).Value;
                case TokenType.Identifier:
                    return ConsumeAndGetToken(TokenType.Identifier).Value;
            }

            return string.Empty;
        }

        private Order ComposeOrder()
        {
            switch (Current.TokenType)
            {
                case TokenType.Asc:
                    Consume(TokenType.Asc);
                    return Order.Ascending;
                case TokenType.Desc:
                    Consume(TokenType.Desc);
                    return Order.Descending;
                case TokenType.Comma:
                    return Order.Ascending;
                case TokenType.EndOfFile:
                    return Order.Ascending;
                default:
                    throw new TSQLException($"Unrecognized token: {Current.TokenType}", _lexer.GetLocation(Current.Span.Start));
            }
        }

        private Node ComposeOperations()
        {
            var node = ComposeEqualityOperators();
            while (IsQueryOperator(Current))
                switch (Current.TokenType)
                {
                    case TokenType.And:
                        Consume(TokenType.And);
                        node = new AndNode(node, ComposeEqualityOperators());
                        break;
                    case TokenType.Or:
                        Consume(TokenType.Or);
                        node = new OrNode(node, ComposeEqualityOperators());
                        break;
                    default:
                        throw new TSQLException($"Unrecognized token: {Current.TokenType}", _lexer.GetLocation(Current.Span.Start));
                }
            return node;
        }

        private Node ComposeArithmeticExpression(int minPrec)
        {
            var left = ComposeBaseTypes(minPrec);

            while (IsArithmeticBinaryOperator(Current) && _precDict[Current.TokenType].Precendence >= minPrec)
            {
                var curr = Current;
                var op = _precDict[Current.TokenType];
                var nextMinPrec = op.Associativity == Associativity.Left ? op.Precendence + 1 : op.Precendence;
                Consume(Current.TokenType);
                var right = ComposeArithmeticExpression(nextMinPrec);

                switch (curr.TokenType)
                {
                    case TokenType.Plus:
                        left = new AddNode(left, right);
                        break;
                    case TokenType.Hyphen:
                        left = new HyphenNode(left, right);
                        break;
                    case TokenType.Star:
                        left = new StarNode(left, right);
                        break;
                    case TokenType.FSlash:
                        left = new FSlashNode(left, right);
                        break;
                    case TokenType.Mod:
                        left = new ModuloNode(left, right);
                        break;
                    case TokenType.Dot:
                        left = new DotNode(left, right, false, string.Empty);
                        break;
                    default:
                        throw new TSQLException($"Unrecognized: {curr.TokenType}", _lexer.GetLocation(curr.Span.Start));
                }
            }

            return left;
        }

        private Node ComposeEqualityOperators()
        {
            var node = ComposeArithmeticExpression(0);

            while (IsEqualityOperator(Current))
                switch (Current.TokenType)
                {
                    case TokenType.GreaterEqual:
                        Consume(TokenType.GreaterEqual);
                        node = new GreaterOrEqualNode(node, ComposeEqualityOperators());
                        break;
                    case TokenType.Greater:
                        Consume(TokenType.Greater);
                        node = new GreaterNode(node, ComposeEqualityOperators());
                        break;
                    case TokenType.LessEqual:
                        Consume(TokenType.LessEqual);
                        node = new LessOrEqualNode(node, ComposeEqualityOperators());
                        break;
                    case TokenType.Less:
                        Consume(TokenType.Less);
                        node = new LessNode(node, ComposeEqualityOperators());
                        break;
                    case TokenType.Equality:
                        Consume(TokenType.Equality);
                        node = new EqualityNode(node, ComposeEqualityOperators());
                        break;
                    case TokenType.Diff:
                        Consume(TokenType.Diff);
                        node = new DiffNode(node, ComposeEqualityOperators());
                        break;
                    case TokenType.Not:
                        Consume(TokenType.Not);
                        node = new NotNode(node);
                        break;
                    case TokenType.Like:
                        Consume(TokenType.Like);
                        node = new LikeNode(node, ComposeBaseTypes());
                        break;
                    case TokenType.NotLike:
                        Consume(TokenType.NotLike);
                        node = new NotNode(new LikeNode(node, ComposeBaseTypes()));
                        break;
                    case TokenType.RLike:
                        Consume(TokenType.RLike);
                        node = new RLikeNode(node, ComposeBaseTypes());
                        break;
                    case TokenType.NotRLike:
                        Consume(TokenType.NotRLike);
                        node = new NotNode(new RLikeNode(node, ComposeBaseTypes()));
                        break;
                    case TokenType.Contains:
                        Consume(TokenType.Contains);
                        node = new ContainsNode(node, ComposeFunctionArgs());
                        break;
                    case TokenType.Is:
                        Consume(TokenType.Is);
                        node = Current.TokenType == TokenType.Not ? 
                            SkipComposeSkip(TokenType.Not, parser => new IsNullNode(node, true), TokenType.Null) : 
                            ComposeAndSkip(parser => new IsNullNode(node, false), TokenType.Null);
                        break;
                    case TokenType.In:
                        Consume(TokenType.In);
                        node = new InNode(node, ComposeFunctionArgs());
                        break;
                    case TokenType.NotIn:
                        Consume(TokenType.NotIn);
                        node = new NotNode(new InNode(node, ComposeFunctionArgs()));
                        break;
                    default:
                        throw new TSQLException($"Unrecognized token: {Current.TokenType}", _lexer.GetLocation(Current.Span.Start));
                }

            return node;
        }

        private FromNode ComposeFrom(bool fromBefore = true)
        {
            if (fromBefore)
            {
                if (Current.TokenType != TokenType.From)
                    return null;
                Consume(TokenType.From);
            }

            List<string> path = ConsumeDottedIdentifiers();
            if (Current.TokenType == TokenType.Function)
            {
                var function = ComposeFunctionMethod(path.ToArray());
                var alias = ComposeAlias();
                return new FromFunctionNode(function, alias);
            }
            else
            {
                if (path.Count == 0)
                    throw new TSQLException($"Expected token is {TokenType.Identifier} or {TokenType.Function} but received {Current.TokenType}", _lexer.GetLocation(Current.Span.Start));

                var tableName = path.Last();
                var tablePath =  path.Count > 1 ? path.Take(path.Count - 1).ToArray() : new string[0];
                var alias = ComposeAlias();
                return new FromTableNode(new TableNode(tableName, tablePath), alias);
            }
        }

        private List<string> ConsumeDottedIdentifiers()
        {
            List<string> path = new List<string>();
            while (Current.TokenType == TokenType.Word ||
                  Current.TokenType == TokenType.Identifier)
            {
                if (Current.TokenType == TokenType.Word)
                    path.Add(ConsumeAndGetToken(TokenType.Word).Value);
                else if (Current.TokenType == TokenType.Identifier)
                    path.Add(ConsumeAndGetToken(TokenType.Identifier).Value);
                if (Current.TokenType == TokenType.Dot)
                    Consume(TokenType.Dot);
                else
                    break;
            }
            return path;
        }

        private void ConsumeWhiteSpaces()
        {
            while (Current.TokenType == TokenType.WhiteSpace)
                Consume(TokenType.WhiteSpace);
        }

        private WhereNode ComposeWhere(bool withoutWhereToken)
        {
            if (Current.TokenType == TokenType.Where)
            {
                Consume(TokenType.Where);
                return new WhereNode(ComposeOperations());
            }

            if (withoutWhereToken)
                return new WhereNode(ComposeOperations());

            return null;
        }

        private void Consume(TokenType tokenType)
        {
            if (Current.TokenType.Equals(tokenType))
            {
                _lexer.Next();
                return;
            }

            throw new TSQLException($"Unrecognized token: {Current.Value}", _lexer.GetLocation(Current.Span.Start));
        }

        private ArgsListNode ComposeFunctionArgs()
        {
            var args = new List<Node>();

            Consume(TokenType.LeftParenthesis);

            if (Current.TokenType != TokenType.RightParenthesis)
            {
                do
                {
                    if (Current.TokenType == TokenType.Comma)
                        Consume(Current.TokenType);

                    args.Add(ComposeEqualityOperators());
                } while (Current.TokenType == TokenType.Comma);
            }

            Consume(TokenType.RightParenthesis);

            return new ArgsListNode(args.ToArray());
        }

        private ArgsListNode ComposeCastArgs()
        {
            var args = new List<Node>();

            Consume(TokenType.LeftParenthesis);

            args.Add(ComposeEqualityOperators());

            Consume(TokenType.As);

            args.Add(ComposeType());

            Consume(TokenType.RightParenthesis);

            return new ArgsListNode(args.ToArray());
        }

        private ArgsListNode ComposeConvertArgs()
        {
            var args = new List<Node>();
            Consume(TokenType.LeftParenthesis);
            args.Add(ComposeType());

            if (Current.TokenType != TokenType.RightParenthesis)
            {
                do
                {
                    if (Current.TokenType == TokenType.Comma)
                        Consume(Current.TokenType);

                    args.Add(ComposeEqualityOperators());
                } while (Current.TokenType == TokenType.Comma);
            }

            Consume(TokenType.RightParenthesis);

            return new ArgsListNode(args.ToArray());
        }

        private ArgsListNode ComposeExecuteArgs()
        {
            var args = new List<Node>();

            do
            {
                if (Current.TokenType == TokenType.Comma)
                    Consume(Current.TokenType);

                args.Add(ComposeEqualityOperators());
            } while (Current.TokenType == TokenType.Comma);

            return new ArgsListNode(args.ToArray());
        }

        private Node ComposeBaseTypes(int minPrec = 0)
        {
            switch (Current.TokenType)
            {
                case TokenType.Decimal:
                    var token = ConsumeAndGetToken(TokenType.Decimal);
                    return new DecimalNode(token.Value);
                case TokenType.Integer:
                    return ComposeInteger();
                case TokenType.Word:
                    return ComposeWord();
                case TokenType.Function:
                    return ComposeFunctionMethod(new string[0]);
                case TokenType.Identifier:

                    if (!(Current is ColumnToken column))
                        throw new TSQLException($"Expected token is {TokenType.Identifier} but received {Current.TokenType}", _lexer.GetLocation(Current.Span.Start));

                    Consume(TokenType.Identifier);

                    return new IdentifierNode(column.Value);
                case TokenType.KeyAccess:
                    var keyAccess = (KeyAccessToken) Current;
                    Consume(TokenType.KeyAccess);
                    return new AccessObjectKeyNode(keyAccess);
                case TokenType.NumericAccess:
                    var numiercAccess = (NumericAccessToken) Current;
                    Consume(TokenType.NumericAccess);
                    return new AccessArrayFieldNode(numiercAccess);
                case TokenType.Star:
                    Consume(TokenType.Star);
                    return new AllColumnsNode();
                case TokenType.True:
                    Consume(TokenType.True);
                    return new BooleanNode(true);
                case TokenType.False:
                    Consume(TokenType.False);
                    return new BooleanNode(false);
                case TokenType.LeftParenthesis:
                    return SkipComposeSkip(TokenType.LeftParenthesis, f => f.ComposeOperations(),
                        TokenType.RightParenthesis);
                case TokenType.Hyphen:
                    Consume(TokenType.Hyphen);
                    return new StarNode(new IntegerNode("-1"), Compose(f => f.ComposeArithmeticExpression(minPrec)));
                case TokenType.Case:
                    var caseNodes = ComposeCase();
                    return new CaseNode(caseNodes.WhenThenNodes, caseNodes.ElseNode);
                case TokenType.Variable:
                    token = ConsumeAndGetToken(TokenType.Variable);
                    return new VariableNode(token.Value);
                case TokenType.Null:
                    token = ConsumeAndGetToken(TokenType.Null);
                    return new NullNode();
            }

            throw new TSQLException($"Token {Current.Value}({Current.TokenType}) cannot be used here.", _lexer.GetLocation(Current.Span.Start));
        }

        private ((Node When, Node Then)[] WhenThenNodes, Node ElseNode) ComposeCase()
        {
            Consume(TokenType.Case);


            var whenThenNodes = new List<(Node When, Node Then)>();

            while(Current.TokenType == TokenType.When)
            {
                Consume(TokenType.When);
                var whenNode = ComposeOperations();
                Consume(TokenType.Then);
                var thenNode = ComposeEqualityOperators();
                whenThenNodes.Add((whenNode, thenNode));
            }

            Consume(TokenType.Else);
            var elseNode = ComposeEqualityOperators();
            Consume(TokenType.End);

            return (whenThenNodes.ToArray(), elseNode);
        }

        private IntegerNode ComposeInteger()
        {
            var token = ConsumeAndGetToken(TokenType.Integer);
            return new IntegerNode(token.Value);
        }

        private WordNode ComposeWord()
        {
            return new WordNode(ConsumeAndGetToken(TokenType.Word).Value);
        }

        private WordNode ComposeIdentifier()
        {
            return new WordNode(ConsumeAndGetToken(TokenType.Identifier).Value);
        }

        private FunctionNode ComposeFunctionMethod(string[] path)
        {
            if (!(Current is FunctionToken func))
                throw new TSQLException($"Expected token is {TokenType.Function} but {Current.TokenType} received", _lexer.GetLocation(Current.Span.Start));

            bool isCastFunction = Current.Value.Equals("CAST", StringComparison.CurrentCultureIgnoreCase);
            bool isConvertFunction = Current.Value.Equals("CONVERT", StringComparison.CurrentCultureIgnoreCase);

            Consume(TokenType.Function);

            ArgsListNode args = null;
            if (isCastFunction)
                args = ComposeCastArgs();
            else if (isConvertFunction)
                args = ComposeConvertArgs();
            else
                args = ComposeFunctionArgs();

            return new FunctionNode(func.Value.Trim(), args, path, null);
        }

        private Token ConsumeAndGetToken(TokenType expected)
        {
            var token = Current;
            Consume(expected);
            return token;
        }

        private Token ConsumeAndGetToken()
        {
            return ConsumeAndGetToken(Current.TokenType);
        }

        private TNode SkipComposeSkip<TNode>(TokenType pType, Func<Parser, TNode> parserAction,
            TokenType aType)
        {
            Consume(pType);
            return ComposeAndSkip(parserAction, aType);
        }

        private TNode ComposeAndSkip<TNode>(Func<Parser, TNode> parserAction, TokenType type)
        {
            var node = Compose(parserAction);
            Consume(type);
            return node;
        }

        private TNode ComposeAndSkipIfPresent<TNode>(Func<Parser, TNode> parserAction, TokenType type)
        {
            var node = Compose(parserAction);
            if (Current.TokenType == type)
                Consume(type);

            return node;
        }

        private TNode Compose<TNode>(Func<Parser, TNode> parserAction)
        {
            if (parserAction == null)
                throw new ArgumentNullException(nameof(parserAction));

            var node = parserAction(this);
            return node;
        }

        private static bool IsArithmeticBinaryOperator(Token currentToken)
        {
            return currentToken.TokenType == TokenType.Star ||
                   currentToken.TokenType == TokenType.FSlash ||
                   currentToken.TokenType == TokenType.Mod ||
                   currentToken.TokenType == TokenType.Plus ||
                   currentToken.TokenType == TokenType.Hyphen ||
                   currentToken.TokenType == TokenType.Dot;
        }

        private static bool IsEqualityOperator(Token currentToken)
        {
            return currentToken.TokenType == TokenType.Greater ||
                   currentToken.TokenType == TokenType.GreaterEqual ||
                   currentToken.TokenType == TokenType.Less ||
                   currentToken.TokenType == TokenType.LessEqual ||
                   currentToken.TokenType == TokenType.Equality ||
                   currentToken.TokenType == TokenType.Not ||
                   currentToken.TokenType == TokenType.Diff ||
                   currentToken.TokenType == TokenType.Like ||
                   currentToken.TokenType == TokenType.NotLike ||
                   currentToken.TokenType == TokenType.Contains ||
                   currentToken.TokenType == TokenType.Is ||
                   currentToken.TokenType == TokenType.In ||
                   currentToken.TokenType == TokenType.NotIn ||
                   currentToken.TokenType == TokenType.RLike ||
                   currentToken.TokenType == TokenType.NotRLike;
        }

        private static bool IsQueryOperator(Token currentToken)
        {
            return currentToken.TokenType == TokenType.And || currentToken.TokenType == TokenType.Or;
        }

        private enum Associativity
        {
            Left,
            Right
        }
    }
}