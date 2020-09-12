using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

public class TokenizerEngine {
    private Stack<RegexPoweredGrammarContext> grammars = new Stack<RegexPoweredGrammarContext>();
    public TokenizerEngine(RegexPoweredGrammarContext startingGrammarCtx) {
        this.grammars.Push(startingGrammarCtx);
    }
    public Token[] Tokenize(string input) {
        var ctx = new TokenizerContext();
        ctx.Input = input;

        List<Token> output = new List<Token>();

        int i = 0; // TODO remove
        while(i<500000 && ctx.CurrentPosition < ctx.Input.Length ) {
            try {
                i++;
                
                var match = grammars.Peek().MatchToken(ctx);
                var resolution = match.Handler.Invoke(match);

                if(resolution.Advance) {
                    ctx.CurrentPosition = match.EndsAt;
                }

                if(resolution.ExitGrammar) {
                    if(grammars.Count <= 1) {
                        // should we throw here? probably not. just for the sake of better composability
                        // so that the current grammar doesn't have to worry about whether it's root
                    } else {
                        grammars.Pop();
                    }
                }
                
                // two possible use cases here:
                // exitGrammar = false && switchToGrammar != null -> enter nested grammar
                // exitGrammar = true && switchToGrammar != null -> replace current grammar
                if(resolution.SwitchToGrammar != null) {
                    grammars.Push(resolution.SwitchToGrammar);
                }

                if(resolution.Tokens != null) {
                    output.AddRange(resolution.Tokens);
                }
            } catch(System.Exception e) {
                throw new TokenizationException(e, grammars.Peek().Description);
            }
        }

        return output.ToArray();
    }
}

public class TokenizationException : System.Exception {
    private readonly GrammarContextDescription source;

    public TokenizationException(Exception cause, GrammarContextDescription source) : base(GetMsg(source), cause) {
        this.source = source;
    }

    private static string GetMsg(GrammarContextDescription src) {
        return 
            "Encountered an error during tokenization!\n"+
            "  | -> was in token grammar context: "+src.name+"\n"+
            "  | -> with compoundRegex: "+src.compoundRegex+"\n"+
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

public class MatchingResolution {
    public bool Advance;
    public bool ExitGrammar;
    public RegexPoweredGrammarContext SwitchToGrammar;
    public Token[] Tokens;

    public MatchingResolution Advancing(bool v) {
        Advance = v;
        return this;
    }

    public MatchingResolution Exiting(bool v) {
        ExitGrammar = v;
        return this;
    }

    public MatchingResolution SwitchingTo(RegexPoweredGrammarContext ctx) {
        SwitchToGrammar = ctx;
        return this;
    }

    public MatchingResolution Returning(Token[] tokens) {
        Tokens = tokens;
        return this;
    }

    public MatchingResolution ReturningEntireMatch(TokenMatchInfo info) {
        Tokens = new Token[] { new Token(info.MatchedTokens) };
        return this;
    }

    public MatchingResolution ReturningEntireMatchAs<T>(TokenMatchInfo info) where T : Token, new() {
        var t = new T();
        t.Value = info.MatchedTokens;
        Tokens = new Token[] { t };
        return this;
    }
}