using System.Collections;
using System.Collections.Generic;

namespace NSDW.ShipForge.LexAST.Lexing {
    public class GrammarContextState {
        public RegexPoweredGrammarContext Grammar {get; set;}
        // allows lexers to track nesting without spawning a retarded amount of copies of themselves
        public int CurrentNestLevel;
        public GrammarContextState(RegexPoweredGrammarContext ctx) {
            this.Grammar = ctx;
        }
    }

    public class TokenizerContext {
        public string Input;
        public int CurrentPosition;
        public List<Token> Tokens = new List<Token>();
        public Stack<GrammarContextState> Lexers = new Stack<GrammarContextState>();

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
            Lexers.Push(new GrammarContextState(ctx));
            return this;
        }

        public TokenizerContext NestLexer(RegexPoweredGrammarContext ctx) {
            Lexers.Push(new GrammarContextState(ctx));
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

        public TokenizerContext Nest(int deltaDepth) {
            Lexers.Peek().CurrentNestLevel+=deltaDepth;
            return this;
        }

        public TokenizerContext UnNest(int deltaDepth) {
            if(Lexers.Peek().CurrentNestLevel<=0) {
                ExitLexer();
            } else {
                Lexers.Peek().CurrentNestLevel-=deltaDepth;
            }
            return this;
        }
    }
}