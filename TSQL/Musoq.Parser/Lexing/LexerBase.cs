using System;
using System.Text.RegularExpressions;

namespace Musoq.Parser.Lexing
{
    /// <summary>
    ///     Idea how to implement this piece of code where founded here:
    ///     https://blogs.msdn.microsoft.com/drew/2009/12/31/a-simple-lexer-in-c-that-uses-regular-expressions/
    /// </summary>
    public abstract class LexerBase<TToken> : ILexer<TToken>
    {
        #region Constructors

        /// <summary>
        ///     Initialize instance.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="defaultToken">The Default token.</param>
        /// <param name="definitions">The Definitions.</param>
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

        #endregion

        #region Public properties

        /// <summary>
        ///     Determine if lexer position is out of range.
        /// </summary>
        protected bool IsOutOfRange => Position >= Input.Length;

        #endregion

        #region TokenUtils

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

        #endregion

        #region Private Variables

        private readonly TokenDefinition[] _definitions;
        private TToken _currentToken;
        private TToken _lastToken;

        #endregion

        #region Protected properties

        /// <summary>
        ///     Gets or sets the position.
        /// </summary>
        public int Position { get; protected set; }

        /// <summary>
        ///     Gets the input.
        /// </summary>
        protected string Input { get; }

        #endregion

        #region Protected Methods

        /// <summary>
        ///     Assigns token of specific type.
        /// </summary>
        /// <param name="instantiate">Instantiate token function.</param>
        /// <returns>The TToken.</returns>
        protected TToken AssignTokenOfType(Func<TToken> instantiate)
        {
            if (instantiate == null)
                throw new ArgumentNullException(nameof(instantiate));

            _lastToken = _currentToken;
            _currentToken = instantiate();
            return _currentToken;
        }

        /// <summary>
        ///     Gets the token.
        /// </summary>
        /// <param name="matchedDefinition">The matched definition.</param>
        /// <param name="match">The match.</param>
        /// <returns>The TToken.</returns>
        protected abstract TToken GetToken(TokenDefinition matchedDefinition, TokenMatch match);

        /// <summary>
        ///     Gets end of file token.
        /// </summary>
        /// <returns>The TToken.</returns>
        protected abstract TToken GetEndOfFileToken();

        #endregion

        #region Public Methods

        /// <summary>
        ///     Gets lastly processed token.
        /// </summary>
        /// <returns>The TToken.</returns>
        public virtual TToken Last()
        {
            return _lastToken;
        }

        /// <summary>
        ///     Gets currently processing token.
        /// </summary>
        /// <returns>The TToken.</returns>
        public virtual TToken Current()
        {
            return _currentToken;
        }

        /// <summary>
        ///     Compute next token.
        /// </summary>
        /// <returns>The TToken.</returns>
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
                    throw new UnknownTokenException(Position, Input[Position],
                        $"Unrecognized token exception at {Position} for {Input.Substring(Position)}");
                var token = GetToken(matchedDefinition, match);
                Position += match.Match.Length;

                return AssignTokenOfType(() => token);
            }

            return AssignTokenOfType(GetEndOfFileToken);
        }

        #endregion
    }
}