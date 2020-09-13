using System.Collections;
using System.Collections.Generic;

namespace NSDW.ShipForge.LexAST.Lexing {
    public class TokenizerContext {
        public string Input;
        public int CurrentPosition;
        public List<Token> Tokens = new List<Token>();
        public Stack<RegexPoweredGrammarContext> Lexers = new Stack<RegexPoweredGrammarContext>();

        public TokenizerContext Advance(int amount) {
            CurrentPosition += amount;
            return this;
        }

        public TokenizerContext AdvanceForMatch(TokenMatchInfo info) {
            CurrentPosition = info.EndsAt;
            return this;
        }

        public TokenizerContext SwitchLexer(RegexPoweredGrammarContext ctx) {
            Lexers.Pop();
            Lexers.Push(ctx);
            return this;
        }

        public TokenizerContext NestLexer(RegexPoweredGrammarContext ctx) {
            Lexers.Push(ctx);
            return this;
        }

        public TokenizerContext ExitLexer() {
            Lexers.Pop();
            return this;
        }

        public TokenizerContext AcceptToken(Token token) {
            Tokens.Add(token);
            return this;
        }

        public TokenizerContext AcceptTokens(Token[] newTokens) {
            Tokens.AddRange(newTokens);
            return this;
        }

        public TokenizerContext AcceptEntireMatchAs<T>(TokenMatchInfo info) where T: Token, new() {
            var t = new T();
            t.Value = info.MatchedTokens;
            Tokens.Add(t);
            return this;
        }
    }
}