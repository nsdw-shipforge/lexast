using System.Collections;
using System.Collections.Generic;

public class ASTEngine {
    Stack<ASTPatternsContext> grammars = new Stack<ASTPatternsContext>();

    public ASTEngine(ASTPatternsContext rootPattern) {
        grammars.Push(rootPattern);
    }

    // TODO EFFORT TokenizerEngine and ASTBuilder execute roughly the same code.
    //  it would be nice to somehow generalize it, if possible. that will take some time, though.
    public object[] ReduceTokensToAST(Token[] tokens) {
        TokenList l = new TokenList(tokens);

        bool hadMatch = true;

        int i = 0; // TODO remove
        while(i<500000 && hadMatch ) {
            try {
                i++;

                hadMatch = false;
                
                var matchInfo = grammars.Peek().MatchPatterns(l);
                // TODO pass readonly list
                matchInfo.Tokens = l;
                var resolution = matchInfo.Handler.Invoke(matchInfo);

                l.ReplaceRange(matchInfo.Position, matchInfo.Position + resolution.RemoveAmount, resolution.ReplaceWith);

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
            } catch(System.Exception e) {
                throw;
                //throw new TokenizationException(e, grammars.Peek().Description);
            }
        }

        return l.Obtain();
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

public class TokenPatternReduceInstruction {
    public int RemoveAmount;
    public object[] ReplaceWith;
    public bool ExitGrammar;
    public ASTPatternsContext SwitchToGrammar;

    public TokenPatternReduceInstruction Removing(int amount) {
        RemoveAmount = amount;
        return this;
    }

    public TokenPatternReduceInstruction Exiting(bool v) {
        ExitGrammar = v;
        return this;
    }

    public TokenPatternReduceInstruction ReplacingWith(object[] elements) {
        ReplaceWith = elements;
        return this;
    }

    public TokenPatternReduceInstruction SwitchingTo(ASTPatternsContext ctx) {
        SwitchToGrammar = ctx;
        return this;
    }

    public TokenPatternReduceInstruction ReplacingWithOne(object o) {
        ReplaceWith = new object[] {o};
        return this;
    }
}