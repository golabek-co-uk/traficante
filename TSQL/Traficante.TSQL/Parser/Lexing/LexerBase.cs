using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Traficante.TSQL.Parser.Lexing
{
    /// <summary>
    ///     Idea how to implement this piece of code where founded here:
    ///     https://blogs.msdn.microsoft.com/drew/2009/12/31/a-simple-lexer-in-c-that-uses-regular-expressions/
    /// </summary>
    public abstract class LexerBase<TToken> : ILexer<TToken>
    {

        protected LexerBase(string input, TToken defaultToken, params TokenDefinition[] definitions)
        {
            if (string.IsNullOrEmpty(input))
                throw new ArgumentException(nameof(input));

            if (definitions == null || definitions.Length == 0)
                throw new ArgumentException(nameof(definitions));

            Input = input.Trim();

            Position = 0;
            _currentToken = defaultToken;
            _definitions = definitions;
        }

        protected bool IsOutOfRange => Position >= Input.Length;

        protected class TokenDefinition
        {
            public Regex Regex { get; }
            public Regex SubRegex { get; }

            public TokenDefinition(string pattern, string subPattern = null)
            {
                Regex = new Regex(pattern);
                SubRegex = subPattern != null? new Regex(subPattern) : null;
            }

            public TokenDefinition(string pattern, RegexOptions options, string subPattern = null)
            {
                Regex = new Regex(pattern, options);
                SubRegex = subPattern != null ? new Regex(subPattern) : null;
            }

            public virtual TokenMatch Match(string input, int position)
            {
                var match = Regex.Match(input, position);
                if (match.Success)
                {
                    if (match.Index - position != 0)
                    {
                        return new TokenMatch { Success = false };
                    }
                    if (SubRegex != null)
                    {
                        var subMatch = SubRegex.Match(match.Value);
                        if (subMatch.Success)
                        {
                            return new TokenMatch
                            {
                                Success = true,
                                Match = match,
                                SubMatch = subMatch
                            };
                        }
                        else
                        {
                            return new TokenMatch
                            {
                                Success = false,
                                Match = match,
                            };
                        }
                    }
                    return new TokenMatch
                    {
                        Success = true,
                        Match = match,
                    };
                }
                return new TokenMatch { Success = false };
            }
        }

        public class TokenMatch
        {
            public bool Success { get; set; }
            public Match Match { get; set; }
            public Match SubMatch { get; set; }
        }

        private readonly TokenDefinition[] _definitions;
        private TToken _currentToken;
        private TToken _lastToken;


        public int Position { get; protected set; }

        public string Input { get; }

        protected TToken AssignTokenOfType(Func<TToken> instantiate)
        {
            if (instantiate == null)
                throw new ArgumentNullException(nameof(instantiate));

            _lastToken = _currentToken;
            _currentToken = instantiate();
            return _currentToken;
        }

        protected abstract TToken GetToken(TokenDefinition matchedDefinition, TokenMatch match);

        protected abstract TToken GetEndOfFileToken();

     
        public virtual TToken Last()
        {
            return _lastToken;
        }

        public virtual TToken Current()
        {
            return _currentToken;
        }

        public virtual TToken Next()
        {
            while (!IsOutOfRange)
            {
                TokenDefinition matchedDefinition = null;
                TokenMatch match = null;

                foreach (var rule in _definitions)
                {
                    var result = rule.Match(Input, Position);
                    if (result.Success)
                    {
                        matchedDefinition = rule;
                        match = result;
                        break;
                    }
                }

                if (matchedDefinition == null)
                    throw new TSQLException($"Unrecognized token", GetLocation(Position));
                var token = GetToken(matchedDefinition, match);
                Position += match.Match.Length;

                return AssignTokenOfType(() => token);
            }

            return AssignTokenOfType(GetEndOfFileToken);
        }

        public (int? LineNumber, int? ColumnNumber) GetLocation(int position)
        {
            var lineNumber = Input.Take(position).Count(c => c == '\n') + 1;
            var columnNumber = Input.Take(position).Reverse().TakeWhile(x => x != '\n').Count() + 1;
            return (lineNumber, columnNumber);
        }
    }
}