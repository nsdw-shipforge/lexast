using System.Collections;
using System.Collections.Generic;

public class TokenParserContext {
    public TokenList Tokens;
    public List<PrecedenceLevelGrammar> Grammars;

    public TokenParserContext(TokenList tokenList, List<PrecedenceLevelGrammar> grammars) {
        Tokens = tokenList;
        Grammars = grammars;
    }

    public TokenParserContext Remove(TokenPatternMatchInfo info, int amount) {
        Tokens.ReplaceRange(info.Position, info.Position+amount, new object[0]);
        return this;
    }

    public TokenParserContext ReplaceWith(TokenPatternMatchInfo info, int amount, object[] elements) {
        Tokens.ReplaceRange(info.Position, info.Position+amount, elements);
        return this;
    }

    public object GetCurrent(TokenPatternMatchInfo info) {
        return Tokens[info.Position];
    }

    public object[] DoParse() {
        for(int i = 0; i<Grammars.Count; i++) {
            Grammars[i].MatchPatterns(this);
        }

        return Tokens.Obtain();
    }
}