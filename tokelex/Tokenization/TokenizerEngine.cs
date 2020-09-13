using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace NSDW.ShipForge.LexAST.Lexing{
    public class TokenizerEngine {
        TokenizerContext ctx;
        public TokenizerEngine(RegexPoweredGrammarContext startingGrammarCtx) {
            ctx = new TokenizerContext();
            ctx.Lexers.Push(startingGrammarCtx);
        }
        public Token[] Tokenize(string input) {
            ctx.Input = input;

            int i = 0; // TODO remove
            while(i<500000 && ctx.CurrentPosition < ctx.Input.Length ) {
                try {
                    i++;
                    
                    var match = ctx.Lexers.Peek().MatchToken(ctx);
                    if(match == null) {
                        break;
                    }
                    match.Handler.Invoke(match, ctx);

                } catch(System.Exception e) {
                    throw new TokenizationException(e, ctx, ctx.Lexers.Peek().Description);
                }
            }

            return ctx.Tokens.ToArray();
        }
    }

    public class TokenizationException : System.Exception {
        private readonly GrammarContextDescription source;
        private readonly TokenizerContext ctx;

        public TokenizationException(Exception cause, TokenizerContext ctx, GrammarContextDescription source) : base(GetMsg(source, ctx), cause) {
            this.source = source;
            this.ctx = ctx;
        }

        private static string GetMsg(GrammarContextDescription src, TokenizerContext ctx) {
            return 
                "Encountered an error during tokenization!\n"+
                "  | -> was in token grammar context: "+src.name+"\n"+
                "  | -> with compoundRegex: "+src.compoundRegex+"\n"+
                "  | -> having parsed these tokens: "+String.Join(", ", ctx.Tokens)+"\n"+
                "  | -> internally caused by...\n";
        }
    }

    public class Token {
        public string Value;

        public Token(string v) {
            Value = v;
        }

        public Token() {}

        public override string ToString()
        {
            return GetType().Name+"["+Value.Replace("\n", "\\n")+"]";
        }
    }
}