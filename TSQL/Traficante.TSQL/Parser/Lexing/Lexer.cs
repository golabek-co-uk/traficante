using System;
using System.Text.RegularExpressions;
using Traficante.TSQL.Parser.Nodes;
using Traficante.TSQL.Parser.Tokens;

namespace Traficante.TSQL.Parser.Lexing
{
    public class Lexer : LexerBase<Token>
    {
        private readonly bool _skipWhiteSpaces;

        public Lexer(string input, bool skipWhiteSpaces) :
            base(input, new NoneToken(), DefinitionSets.General)
        {
            _skipWhiteSpaces = skipWhiteSpaces;
        }

        private TokenType GetTokenCandidate(string tokenText, TokenDefinition matchedDefinition)
        {
            var regex = matchedDefinition.Regex.ToString();

            if (regex == TokenRegexDefinition.KIdentifierBracketed)
                return TokenType.Identifier;

            switch (tokenText.ToLowerInvariant())
            {
                case DescToken.TokenText:
                    return TokenType.Desc;
                case AscToken.TokenText:
                    return TokenType.Asc;
                case AndToken.TokenText:
                    return TokenType.And;
                case CommaToken.TokenText:
                    return TokenType.Comma;
                case DiffToken.TokenText:
                    return TokenType.Diff;
                case GreaterToken.TokenText:
                    return TokenType.Greater;
                case GreaterEqualToken.TokenText:
                    return TokenType.GreaterEqual;
                case HyphenToken.TokenText:
                    return TokenType.Hyphen;
                case LeftParenthesisToken.TokenText:
                    return TokenType.LeftParenthesis;
                case RightParenthesisToken.TokenText:
                    return TokenType.RightParenthesis;
                case LessToken.TokenText:
                    return TokenType.Less;
                case LessEqualToken.TokenText:
                    return TokenType.LessEqual;
                case ModuloToken.TokenText:
                    return TokenType.Mod;
                case NotToken.TokenText:
                    return TokenType.Not;
                case OrToken.TokenText:
                    return TokenType.Or;
                case PlusToken.TokenText:
                    return TokenType.Plus;
                case FSlashToken.TokenText:
                    return TokenType.FSlash;
                case StarToken.TokenText:
                    return TokenType.Star;
                case WhereToken.TokenText:
                    return TokenType.Where;
                case WhiteSpaceToken.TokenText:
                    return TokenType.WhiteSpace;
                case SelectToken.TokenText:
                    return TokenType.Select;
                case FromToken.TokenText:
                    return TokenType.From;
                case EqualityToken.TokenText:
                    return TokenType.Equality;
                case LikeToken.TokenText:
                    return TokenType.Like;
                case RLikeToken.TokenText:
                    return TokenType.RLike;
                case ContainsToken.TokenText:
                    return TokenType.Contains;
                case AsToken.TokenText:
                    return TokenType.As;
                case SetOperatorToken.ExceptOperatorText:
                    return TokenType.Except;
                case SetOperatorToken.IntersectOperatorText:
                    return TokenType.Intersect;
                case SetOperatorToken.UnionOperatorText:
                    return TokenType.Union;
                case DotToken.TokenText:
                    return TokenType.Dot;
                case HavingToken.TokenText:
                    return TokenType.Having;
                case TakeToken.TokenText:
                    return TokenType.Take;
                case SkipToken.TokenText:
                    return TokenType.Skip;
                case TopToken.TokenText:
                    return TokenType.Top;
                case WithToken.TokenText:
                    return TokenType.With;
                case OnToken.TokenText:
                    return TokenType.On;
                case IsToken.TokenText:
                    return TokenType.Is;
                case NullToken.TokenText:
                    return TokenType.Null;
                case TrueToken.TokenText:
                    return TokenType.True;
                case FalseToken.TokenText:
                    return TokenType.False;
                case InToken.TokenText:
                    return TokenType.In;
                case TableToken.TokenText:
                    return TokenType.Table;
                case LBracketToken.TokenText:
                    return TokenType.LBracket;
                case RBracketToken.TokenText:
                    return TokenType.RBracket;
                case SemicolonToken.TokenText:
                    return TokenType.Semicolon;
                case CaseToken.TokenText:
                    return TokenType.Case;
                case WhenToken.TokenText:
                    return TokenType.When;
                case ThenToken.TokenText:
                    return TokenType.Then;
                case ElseToken.TokenText:
                    return TokenType.Else;
                case EndToken.TokenText:
                    return TokenType.End;
                case DeclareToken.TokenText:
                    return TokenType.Declare;
                case SetToken.TokenText:
                    return TokenType.Set;
                case ExecuteToken.TokenText:
                    return TokenType.Execute;
            }

            if (string.IsNullOrWhiteSpace(tokenText))
                return TokenType.WhiteSpace;

            if (int.TryParse(tokenText, out _) && !tokenText.Contains(" "))
                return TokenType.Integer;

            if (regex == TokenRegexDefinition.KNotIn)
                return TokenType.NotIn;
            if (regex == TokenRegexDefinition.KNotLike)
                return TokenType.NotLike;
            if (regex == TokenRegexDefinition.KRNotLike)
                return TokenType.NotRLike;
            if (regex == TokenRegexDefinition.Function)
                return TokenType.Function;
            if (regex == TokenRegexDefinition.KKeyObjectAccess)
                return TokenType.KeyAccess;
            if (regex == TokenRegexDefinition.KNumericArrayAccess)
                return TokenType.NumericAccess;
            if (regex == TokenRegexDefinition.KDecimal)
                return TokenType.Decimal;
            if (regex == TokenRegexDefinition.KGroupBy)
                return TokenType.GroupBy;
            if (regex == TokenRegexDefinition.KUnionAll)
                return TokenType.UnionAll;
            if (regex == TokenRegexDefinition.KOrderBy)
                return TokenType.OrderBy;
            if (regex == TokenRegexDefinition.Function)
                return TokenType.Function;
            if (regex == TokenRegexDefinition.KVariable)
                return TokenType.Variable;
            if (regex == TokenRegexDefinition.KInnerJoin)
                return TokenType.InnerJoin;
            if (regex == TokenRegexDefinition.KOuterJoin)
                return TokenType.OuterJoin;
            if (regex == TokenRegexDefinition.KIdentifier)
                return TokenType.Identifier;

            return TokenType.Word;
        }

        private static class TokenRegexDefinition
        {
            private const string Keyword = @"(?<=[\s]{1,}|^){keyword}(?=[\s]{1,}|$)";
            public const string Function = @"[a-zA-Z_-]{1,}[a-zA-Z1-9_-]{1,}[\d]*\s?(?=[\(])";

            public static readonly string KAnd = Format(Keyword, AndToken.TokenText);
            public static readonly string KComma = CommaToken.TokenText;
            public static readonly string KDiff = DiffToken.TokenText;
            public static readonly string KfSlashToken = Format(Keyword, FSlashToken.TokenText);
            public static readonly string KGreater = Format(Keyword, GreaterToken.TokenText);
            public static readonly string KGreaterEqual = Format(Keyword, GreaterEqualToken.TokenText);
            public static readonly string KHyphen = $@"\{HyphenToken.TokenText}";
            public static readonly string KLeftParenthesis = $@"\{LeftParenthesisToken.TokenText}";
            public static readonly string KLess = Format(Keyword, LessToken.TokenText);
            public static readonly string KLessEqual = Format(Keyword, LessEqualToken.TokenText);
            public static readonly string KModulo = Format(Keyword, ModuloToken.TokenText);
            public static readonly string KNot = Format(Keyword, NotToken.TokenText);
            public static readonly string KOr = Format(Keyword, OrToken.TokenText);
            public static readonly string KPlus = $@"\{PlusToken.TokenText}";
            public static readonly string KRightParenthesis = $@"\{RightParenthesisToken.TokenText}";
            public static readonly string KIs = Format(Keyword, IsToken.TokenText);
            public static readonly string KNull = Format(Keyword, NullToken.TokenText);
            public static readonly string KStar = Format(Keyword, $@"\{StarToken.TokenText}");
            public static readonly string KWhere = Format(Keyword, WhereToken.TokenText);
            public static readonly string KWhiteSpace = @"[\s]{1,}";
            public static readonly string KWordBracketed = @"N?'((?:''|[^'])*)'";
            public static readonly string KEqual = Format(Keyword, EqualityToken.TokenText);
            public static readonly string KInsert = Format(Keyword, InsertToken.TokenText);
            public static readonly string KUpdate = Format(Keyword, UpdateToken.TokenText);
            public static readonly string KSelect = Format(Keyword, SelectToken.TokenText);
            public static readonly string KFrom = Format(Keyword, FromToken.TokenText);
            public static readonly string KIdentifier = @"(#?[\w*?_]){1,}";
            public static readonly string KIdentifierBracketed = @"\G\[([\\#\s\w*?_?-]){1,}\]?";
            public static readonly string KIdentifierBracketedValue = @"([\\#\s\w*?_?-]){1,}";
            public static readonly string KVariable = @"@@?([\w*?_?-]){1,}";
            public static readonly string KLike = Format(Keyword, LikeToken.TokenText);
            public static readonly string KNotLike = @"(?<=[\s]{1,}|^)not[\s]{1,}like(?=[\s]{1,}|$)";
            public static readonly string KRLike = Format(Keyword, RLikeToken.TokenText);
            public static readonly string KRNotLike = @"(?<=[\s]{1,}|^)not[\s]{1,}rlike(?=[\s]{1,}|$)";
            public static readonly string KAs = Format(Keyword, AsToken.TokenText);
            public static readonly string KUnion = Format(Keyword, SetOperatorToken.UnionOperatorText);
            public static readonly string KDot = "\\.";
            public static readonly string KIntersect = Format(Keyword, SetOperatorToken.IntersectOperatorText);
            public static readonly string KExcept = Format(Keyword, SetOperatorToken.ExceptOperatorText);
            public static readonly string KCompareWith = @"(?<=[\s]{1,}|^)compare[\s]{1,}with(?=[\s]{1,}|$)";
            public static readonly string KUnionAll = @"(?<=[\s]{1,}|^)union[\s]{1,}all(?=[\s]{1,}|$)";
            public static readonly string KGroupBy = @"(?<=[\s]{1,}|^)group[\s]{1,}by(?=[\s]{1,}|$)";
            public static readonly string KHaving = Format(Keyword, HavingToken.TokenText);
            public static readonly string KContains = Format(Keyword, ContainsToken.TokenText);
            public static readonly string KDecimal = @"[\-]?([0-9]+(\.[0-9]{1,})?)[dD]?";
            public static readonly string KNumericArrayAccess = "([\\w*?_]{1,})\\[([0-9]{1,})\\]";
            public static readonly string KKeyObjectAccess = "([\\w*?_]{1,})\\[([a-zA-Z0-9]{1,})\\]";

            public static readonly string KSkip = Format(Keyword, SkipToken.TokenText);
            public static readonly string KTake = Format(Keyword, TakeToken.TokenText);
            public static readonly string KTop = Format(Keyword, TopToken.TokenText);
            public static readonly string KWith = Format(Keyword, WithToken.TokenText);
            public static readonly string KInnerJoin = @"(?<=[\s]{1,}|^)inner[\s]{1,}join(?=[\s]{1,}|$)";

            public static readonly string KOuterJoin =
                @"(?<=[\s]{1,}|^)(left|right)[\s]{1,}(outer[\s]{0,1})?(join[\s]{0,1})(?=[\s]{1,}|$)";

            public static readonly string KOn = Format(Keyword, OnToken.TokenText);
            public static readonly string KOrderBy = @"(?<=[\s]{1,}|^)order[\s]{1,}by(?=[\s]{1,}|$)";
            public static readonly string KAsc = Format(Keyword, AscToken.TokenText);
            public static readonly string KDesc = Format(Keyword, DescToken.TokenText);
            public static readonly string KTrue = Format(Keyword, TrueToken.TokenText);
            public static readonly string KFalse = Format(Keyword, FalseToken.TokenText);
            public static readonly string KIn = Format(Keyword, InToken.TokenText);
            public static readonly string KNotIn = @"(?<=[\s]{1,}|^)not[\s]{1,}in(?=[\s]{1,}|$)";

            public static readonly string KTable = Format(Keyword, TableToken.TokenText);
            public static readonly string KLeftBracket = "\\{";
            public static readonly string KRightBracket = "\\}";
            public static readonly string KSemicolon = "\\;";
            public static readonly string KCase = Format(Keyword, CaseToken.TokenText);
            public static readonly string KWhen = Format(Keyword, WhenToken.TokenText);
            public static readonly string KThen = Format(Keyword, ThenToken.TokenText);
            public static readonly string KElse = Format(Keyword, ElseToken.TokenText);
            public static readonly string KEnd = Format(Keyword, EndToken.TokenText);
            public static readonly string KDeclare = Format(Keyword, DeclareToken.TokenText);
            public static readonly string KSet = Format(Keyword, SetToken.TokenText);

            private static string Format(string keyword, string arg)
            {
                try
                {
                    return keyword.Replace("{keyword}", arg);
                }
                catch(Exception exc)
                {
                    throw exc;
                }
            }
        }

        private static class DefinitionSets
        {
            public static TokenDefinition[] General => new[]
            {
                new TokenDefinition(TokenRegexDefinition.KDesc, RegexOptions.IgnoreCase),
                new TokenDefinition(TokenRegexDefinition.KAsc, RegexOptions.IgnoreCase),
                new TokenDefinition(TokenRegexDefinition.KLike, RegexOptions.IgnoreCase),
                new TokenDefinition(TokenRegexDefinition.KNotLike, RegexOptions.IgnoreCase),
                new TokenDefinition(TokenRegexDefinition.KRLike, RegexOptions.IgnoreCase),
                new TokenDefinition(TokenRegexDefinition.KRNotLike, RegexOptions.IgnoreCase),
                new TokenDefinition(TokenRegexDefinition.KNotIn, RegexOptions.IgnoreCase), 
                new TokenDefinition(TokenRegexDefinition.KDecimal),
                new TokenDefinition(TokenRegexDefinition.KAs, RegexOptions.IgnoreCase),
                new TokenDefinition(TokenRegexDefinition.KAnd, RegexOptions.IgnoreCase),
                new TokenDefinition(TokenRegexDefinition.KComma),
                new TokenDefinition(TokenRegexDefinition.KDiff),
                new TokenDefinition(TokenRegexDefinition.KfSlashToken),
                new TokenDefinition(TokenRegexDefinition.KGreater),
                new TokenDefinition(TokenRegexDefinition.KGreaterEqual),
                new TokenDefinition(TokenRegexDefinition.KHyphen),
                new TokenDefinition(TokenRegexDefinition.KLeftParenthesis),
                new TokenDefinition(TokenRegexDefinition.KRightParenthesis),
                new TokenDefinition(TokenRegexDefinition.KLess),
                new TokenDefinition(TokenRegexDefinition.KLessEqual),
                new TokenDefinition(TokenRegexDefinition.KEqual),
                new TokenDefinition(TokenRegexDefinition.KModulo),
                new TokenDefinition(TokenRegexDefinition.KNot, RegexOptions.IgnoreCase),
                new TokenDefinition(TokenRegexDefinition.KOr, RegexOptions.IgnoreCase),
                new TokenDefinition(TokenRegexDefinition.KPlus),
                new TokenDefinition(TokenRegexDefinition.KStar),
                new TokenDefinition(TokenRegexDefinition.KIs, RegexOptions.IgnoreCase),
                new TokenDefinition(TokenRegexDefinition.KIn, RegexOptions.IgnoreCase), 
                new TokenDefinition(TokenRegexDefinition.KNull, RegexOptions.IgnoreCase),
                new TokenDefinition(TokenRegexDefinition.KWith, RegexOptions.IgnoreCase),
                new TokenDefinition(TokenRegexDefinition.KWhere, RegexOptions.IgnoreCase),
                new TokenDefinition(TokenRegexDefinition.KContains, RegexOptions.IgnoreCase),
                new TokenDefinition(TokenRegexDefinition.KWhiteSpace),
                new TokenDefinition(TokenRegexDefinition.KUnionAll, RegexOptions.IgnoreCase),
                new TokenDefinition(TokenRegexDefinition.KWordBracketed, RegexOptions.ECMAScript),
                new TokenDefinition(TokenRegexDefinition.KInsert, RegexOptions.IgnoreCase),
                new TokenDefinition(TokenRegexDefinition.KUpdate, RegexOptions.IgnoreCase),
                new TokenDefinition(TokenRegexDefinition.KSelect, RegexOptions.IgnoreCase),
                new TokenDefinition(TokenRegexDefinition.KFrom, RegexOptions.IgnoreCase),
                new TokenDefinition(TokenRegexDefinition.KUnion, RegexOptions.IgnoreCase),
                new TokenDefinition(TokenRegexDefinition.KExcept, RegexOptions.IgnoreCase),
                new TokenDefinition(TokenRegexDefinition.KIntersect, RegexOptions.IgnoreCase),
                new TokenDefinition(TokenRegexDefinition.KGroupBy, RegexOptions.IgnoreCase),
                new TokenDefinition(TokenRegexDefinition.KHaving, RegexOptions.IgnoreCase),
                new TokenDefinition(TokenRegexDefinition.KSkip, RegexOptions.IgnoreCase),
                new TokenDefinition(TokenRegexDefinition.KTake, RegexOptions.IgnoreCase),
                new TokenDefinition(TokenRegexDefinition.KTop, RegexOptions.IgnoreCase),
                new TokenDefinition(TokenRegexDefinition.KNumericArrayAccess),
                new TokenDefinition(TokenRegexDefinition.Function),
                new TokenDefinition(TokenRegexDefinition.KKeyObjectAccess),
                new TokenDefinition(TokenRegexDefinition.KInnerJoin, RegexOptions.IgnoreCase),
                new TokenDefinition(TokenRegexDefinition.KOuterJoin, RegexOptions.IgnoreCase),
                new TokenDefinition(TokenRegexDefinition.KOrderBy, RegexOptions.IgnoreCase),
                new TokenDefinition(TokenRegexDefinition.KTrue, RegexOptions.IgnoreCase),
                new TokenDefinition(TokenRegexDefinition.KFalse, RegexOptions.IgnoreCase),

                new TokenDefinition(TokenRegexDefinition.KVariable),
                new TokenDefinition(TokenRegexDefinition.KDot),
                new TokenDefinition(TokenRegexDefinition.KOn, RegexOptions.IgnoreCase),
                new TokenDefinition(TokenRegexDefinition.KTable, RegexOptions.IgnoreCase),
                new TokenDefinition(TokenRegexDefinition.KLeftBracket),
                new TokenDefinition(TokenRegexDefinition.KRightBracket),
                new TokenDefinition(TokenRegexDefinition.KSemicolon),
                new TokenDefinition(TokenRegexDefinition.KCase, RegexOptions.IgnoreCase),
                new TokenDefinition(TokenRegexDefinition.KWhen, RegexOptions.IgnoreCase),
                new TokenDefinition(TokenRegexDefinition.KElse, RegexOptions.IgnoreCase),
                new TokenDefinition(TokenRegexDefinition.KThen, RegexOptions.IgnoreCase),
                new TokenDefinition(TokenRegexDefinition.KEnd, RegexOptions.IgnoreCase),
                new TokenDefinition(TokenRegexDefinition.KDeclare, RegexOptions.IgnoreCase),
                new TokenDefinition(TokenRegexDefinition.KSet, RegexOptions.IgnoreCase),

                new TokenDefinition(TokenRegexDefinition.KIdentifier),
                new TokenDefinition(TokenRegexDefinition.KIdentifierBracketed, subPattern: TokenRegexDefinition.KIdentifierBracketedValue),
            };
        }

        public override Token Next()
        {
            var token = base.Next();
            while (_skipWhiteSpaces && token.TokenType == TokenType.WhiteSpace)
                token = base.Next();
            return token;
        }

        protected override Token GetEndOfFileToken()
        {
            return new EndOfFileToken(new TextSpan(Input.Length, 0));
        }

        protected override Token GetToken(TokenDefinition matchedDefinition, TokenMatch match)
        {
            var tokenText = match.SubMatch != null ? match.SubMatch.Value : match.Match.Value;
            var token = GetTokenCandidate(tokenText, matchedDefinition);
            Match definitionMatch = null;

            switch (token)
            {
                case TokenType.Desc:
                    return new DescToken(new TextSpan(Position, tokenText.Length));
                case TokenType.Asc:
                    return new AscToken(new TextSpan(Position, tokenText.Length));
                case TokenType.And:
                    return new AndToken(new TextSpan(Position, tokenText.Length));
                case TokenType.Comma:
                    return new CommaToken(new TextSpan(Position, tokenText.Length));
                case TokenType.Diff:
                    return new DiffToken(new TextSpan(Position, tokenText.Length));
                case TokenType.Equality:
                    return new EqualityToken(new TextSpan(Position, tokenText.Length));
                case TokenType.FSlash:
                    return new FSlashToken(new TextSpan(Position, tokenText.Length));
                case TokenType.Function:
                    return new FunctionToken(tokenText, new TextSpan(Position, tokenText.Length));
                case TokenType.Greater:
                    return new GreaterToken(new TextSpan(Position, tokenText.Length));
                case TokenType.GreaterEqual:
                    return new GreaterEqualToken(new TextSpan(Position, tokenText.Length));
                case TokenType.Hyphen:
                    return new HyphenToken(new TextSpan(Position, tokenText.Length));
                case TokenType.LeftParenthesis:
                    return new LeftParenthesisToken(new TextSpan(Position, tokenText.Length));
                case TokenType.Less:
                    return new LessToken(new TextSpan(Position, tokenText.Length));
                case TokenType.LessEqual:
                    return new LessEqualToken(new TextSpan(Position, tokenText.Length));
                case TokenType.Mod:
                    return new ModuloToken(new TextSpan(Position, tokenText.Length));
                case TokenType.Not:
                    return new NotToken(new TextSpan(Position, tokenText.Length));
                case TokenType.Decimal:
                    return new DecimalToken(tokenText.TrimEnd('d'), new TextSpan(Position, tokenText.Length));
                case TokenType.Integer:
                    return new IntegerToken(tokenText, new TextSpan(Position, tokenText.Length));
                case TokenType.Or:
                    return new OrToken(new TextSpan(Position, tokenText.Length));
                case TokenType.Plus:
                    return new PlusToken(new TextSpan(Position, tokenText.Length));
                case TokenType.RightParenthesis:
                    return new RightParenthesisToken(new TextSpan(Position, tokenText.Length));
                case TokenType.Star:
                    return new StarToken(new TextSpan(Position, tokenText.Length));
                case TokenType.Where:
                    return new WhereToken(new TextSpan(Position, tokenText.Length));
                case TokenType.WhiteSpace:
                    return new WhiteSpaceToken(new TextSpan(Position, tokenText.Length));
                case TokenType.Insert:
                    return new InsertToken(new TextSpan(Position, tokenText.Length));
                case TokenType.Update:
                    return new UpdateToken(new TextSpan(Position, tokenText.Length));
                case TokenType.InTo:
                    return new InToToken(new TextSpan(Position, tokenText.Length));
                case TokenType.Values:
                    return new ValuesToken(new TextSpan(Position, tokenText.Length));
                case TokenType.From:
                    return new FromToken(new TextSpan(Position, tokenText.Length));
                case TokenType.Select:
                    return new SelectToken(new TextSpan(Position, tokenText.Length));
                case TokenType.Identifier:
                    return new ColumnToken(tokenText, new TextSpan(Position, tokenText.Length));
                case TokenType.Variable:
                    return new VariableToken(tokenText, new TextSpan(Position, tokenText.Length));
                case TokenType.Like:
                    return new LikeToken(new TextSpan(Position, tokenText.Length));
                case TokenType.NotLike:
                    return new NotLikeToken(new TextSpan(Position, tokenText.Length));
                case TokenType.RLike:
                    return new RLikeToken(new TextSpan(Position, tokenText.Length));
                case TokenType.NotRLike:
                    return new NotRLikeToken(new TextSpan(Position, tokenText.Length));
                case TokenType.As:
                    return new AsToken(new TextSpan(Position, tokenText.Length));
                case TokenType.Except:
                    return new ExceptToken(new TextSpan(Position, tokenText.Length));
                case TokenType.Union:
                    return new UnionToken(new TextSpan(Position, tokenText.Length));
                case TokenType.Intersect:
                    return new IntersectToken(new TextSpan(Position, tokenText.Length));
                case TokenType.UnionAll:
                    return new UnionAllToken(new TextSpan(Position, tokenText.Length));
                case TokenType.Dot:
                    return new DotToken(new TextSpan(Position, tokenText.Length));
                case TokenType.GroupBy:
                    return new GroupByToken(new TextSpan(Position, tokenText.Length));
                case TokenType.Having:
                    return new HavingToken(new TextSpan(Position, tokenText.Length));
                case TokenType.NumericAccess:
                    definitionMatch = matchedDefinition.Regex.Match(tokenText);
                    return new NumericAccessToken(definitionMatch.Groups[1].Value, definitionMatch.Groups[2].Value,
                        new TextSpan(Position, tokenText.Length));
                case TokenType.KeyAccess:
                    definitionMatch = matchedDefinition.Regex.Match(tokenText);
                    return new KeyAccessToken(definitionMatch.Groups[1].Value, definitionMatch.Groups[2].Value,
                        new TextSpan(Position, tokenText.Length));
                case TokenType.Contains:
                    return new ContainsToken(new TextSpan(Position, tokenText.Length));
                case TokenType.Skip:
                    return new SkipToken(new TextSpan(Position, tokenText.Length));
                case TokenType.Take:
                    return new TakeToken(new TextSpan(Position, tokenText.Length));
                case TokenType.Top:
                    return new TopToken(new TextSpan(Position, tokenText.Length));
                case TokenType.With:
                    return new WithToken(new TextSpan(Position, tokenText.Length));
                case TokenType.On:
                    return new OnToken(new TextSpan(Position, tokenText.Length));
                case TokenType.InnerJoin:
                    return new InnerJoinToken(new TextSpan(Position, tokenText.Length));
                case TokenType.OuterJoin:
                    var type = match.Match.Groups[1].Value.ToLowerInvariant() == "left"
                        ? "left"
                        : "right";

                    return new OuterJoinToken(type, new TextSpan(Position, tokenText.Length));
                //case TokenType.Function:
                //    return new MethodAccessToken(match.Match.Groups[0].Value,
                //        new TextSpan(Position, match.Match.Groups[0].Value.Length));
                case TokenType.Is:
                    return new IsToken(new TextSpan(Position, tokenText.Length));
                case TokenType.Null:
                    return new NullToken(new TextSpan(Position, tokenText.Length));
                case TokenType.OrderBy:
                    return new OrderByToken(new TextSpan(Position, tokenText.Length));
                case TokenType.True:
                    return new TrueToken(new TextSpan(Position, tokenText.Length));
                case TokenType.False:
                    return new FalseToken(new TextSpan(Position, tokenText.Length));
                case TokenType.In:
                    return new InToken(new TextSpan(Position, tokenText.Length));
                case TokenType.NotIn:
                    return new NotInToken(new TextSpan(Position, tokenText.Length));
                case TokenType.Table:
                    return new TableToken(new TextSpan(Position, tokenText.Length));
                case TokenType.LBracket:
                    return new LBracketToken(new TextSpan(Position, tokenText.Length));
                case TokenType.RBracket:
                    return new RBracketToken(new TextSpan(Position, tokenText.Length));
                case TokenType.Semicolon:
                    return new SemicolonToken(new TextSpan(Position, tokenText.Length));
                case TokenType.Case:
                    return new CaseToken(new TextSpan(Position, tokenText.Length));
                case TokenType.When:
                    return new WhenToken(new TextSpan(Position, tokenText.Length));
                case TokenType.Then:
                    return new ThenToken(new TextSpan(Position, tokenText.Length));
                case TokenType.Else:
                    return new ElseToken(new TextSpan(Position, tokenText.Length));
                case TokenType.End:
                    return new EndToken(new TextSpan(Position, tokenText.Length));
                case TokenType.Declare:
                    return new DeclareToken(new TextSpan(Position, tokenText.Length));
                case TokenType.Set:
                    return new SetToken(new TextSpan(Position, tokenText.Length));
                case TokenType.Execute:
                    return new ExecuteToken(new TextSpan(Position, tokenText.Length));
            }

            if (matchedDefinition.Regex.ToString() == TokenRegexDefinition.KWordBracketed)
                return new WordToken(match.Match.Groups[1].Value, new TextSpan(Position + 1, match.Match.Groups[1].Value.Length));
            return new WordToken(tokenText, new TextSpan(Position, tokenText.Length));
        }
    }
}