using System.Collections;
using System.Collections.Generic;

using ASTPattern = System.Func<TokenPatternMatchInfo, bool>;
using ASTReducer = System.Action<TokenPatternMatchInfo, TokenParserContext>;

public class PrecedenceLevelGrammar {
    private readonly IDictionary<ASTPattern, ASTReducer> patterns;

    public PrecedenceLevelGrammar(IDictionary<ASTPattern, ASTReducer> patterns) {
        this.patterns = patterns;
    }

    public PrecedenceLevelGrammar() {
        this.patterns = new Dictionary<ASTPattern, ASTReducer> ();
    }

    public PrecedenceLevelGrammar AddPatternReducerPair(ASTPattern pattern, ASTReducer reducer) {
        patterns.Add(pattern, reducer);
        return this;
    }

    public PrecedenceLevelGrammar AddPatternReducerPair(System.Tuple<ASTPattern, ASTReducer> pair) {
        patterns.Add(pair.Item1, pair.Item2);
        return this;
    }

    public void MatchPatterns(TokenParserContext c) {
        try {
            // we keep iterating until the current precedence level no longer matches anything
            // TODO this probably should be configurable
            bool matched = false;
            int noInfinite = 0;
            do {
                noInfinite++; // TODO remove
                // iterate over all tokens
                for(int i = 0; i<c.Tokens.Count; i++) {
                    // at each token, check for every known pattern
                    foreach(var kv in patterns) {
                        var matchInfo = new TokenPatternMatchInfo(i, c.Tokens, kv.Value);
                        var matcher = kv.Key;
                        // upon matching a pattern, invoke the associated action
                        // (which may or may not reduce it)
                        if(matcher(matchInfo)) {
                            matched = true;
                            matchInfo.Handler(matchInfo, c);
                        }
                    }
                }
            } while(matched && noInfinite < 50000);
        } catch(System.Exception e) {
            System.Console.WriteLine("Exception caught! Current list state: "+string.Join(", ", c.Tokens));
            throw;
        }
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

public class TokenList {
    private List<object> list;

    public object this[int index] { 
        get {
            return list[index];
        }
    }

    public TokenList(IEnumerable<object> tokens) {
        list = new List<object>(tokens);
    }

    public void ReplaceRange(int from, int to, object[] with) {
        list.RemoveRange(from, (to-from) + 1);
        list.InsertRange(from, with);
    }

    public TokenList GetSublist(int from, int to) {
        var sublist = list.GetRange(from, (to-from)+1);
        return new TokenList(sublist);
    }

    public override string ToString() {
        return string.Join(",", list);
    }

    public object[] Obtain() {
        return list.ToArray();
    }

    public int Count { get => list.Count; }
}
