using System.Collections;
using System.Collections.Generic;

using ASTPattern = System.Func<TokenPatternMatchInfo, bool>;
using ASTReducer = System.Func<TokenPatternMatchInfo, TokenPatternReduceInstruction>;

public class ASTPatternsContext {
    private readonly IDictionary<ASTPattern, ASTReducer> patterns;

    public ASTPatternsContext(IDictionary<ASTPattern, ASTReducer> patterns) {
        this.patterns = patterns;
    }

    public TokenPatternMatchInfo MatchPatterns(TokenList l) {
        for(int i = 0; i<l.Count; i++) {
            foreach(var kv in patterns) {
                var matchInfo = new TokenPatternMatchInfo(i, l, kv.Value);
                var matcher = kv.Key;
                if(matcher(matchInfo)) {
                    return matchInfo;
                }
            }
        }
        throw new System.InvalidOperationException("Current AST grammar is unable to match any known pattern in following list: "+l);
    }
}

public class TokenPatternMatchInfo {
    public int Position;
    public TokenList Tokens;
    public ASTReducer Handler;

    public TokenPatternMatchInfo(int position, TokenList tokens, ASTReducer handler) {
        Position = position;
        Handler = handler;
        Tokens = tokens;
    }
}