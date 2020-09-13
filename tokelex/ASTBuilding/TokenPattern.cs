using System.Collections;
using System.Collections.Generic;

using ASTPattern = System.Func<TokenPatternMatchInfo, bool>;
using ASTReducer = System.Action<TokenPatternMatchInfo, TokenParserContext>;

public static class TokenPatternUtil {
    public static bool CurrentElementIs<T>(TokenPatternMatchInfo info) {
        return info.Tokens[info.Position] is T;
    }

    public static bool NextElementIs<T>(TokenPatternMatchInfo info) {
        if(info.Position+1 >= info.Tokens.Count) {
            return false;
        }
        return info.Tokens[info.Position+1] is T;
    }

    public static bool CurrentElementMatches(TokenPatternMatchInfo info, System.Func<object, bool> predicate) {
        return predicate(info.Tokens[info.Position]);
    }

    public static bool NextElementMatches(TokenPatternMatchInfo info, System.Func<object, bool> predicate) {
        if(info.Position+1 >= info.Tokens.Count) {
            return false;
        }
        return predicate(info.Tokens[info.Position+1]);
    }

    public static bool ConsideringCurrentAndNext(TokenPatternMatchInfo info, System.Func<object, object, bool> predicate) {
        object next = null;
        if(info.Position+1 < info.Tokens.Count) {
            next = info.Tokens[info.Position+1];
        }
        return predicate(info.Tokens[info.Position], next);
    }

    public static int CountAllOfType<T>(TokenPatternMatchInfo info) {
        int matched = 0;
        for(int i = info.Position; i<info.Tokens.Count; i++) {
            if(info.Tokens[i] !is T) {
                break;
            }
            matched++;
        }
        return matched;
    }

    public static T GetCurrentElementAs<T>(TokenPatternMatchInfo info) where T: class {
        return info.Tokens[info.Position] as T;
    }

    public static T GetNextElementAs<T>(TokenPatternMatchInfo info) where T: class {
        if(info.Position+1 >= info.Tokens.Count) {
            return null;
        }
        return info.Tokens[info.Position+1] as T;
    }

    public static System.Tuple<ASTPattern, ASTReducer> SingleNextMatcherConsumer<TAnchor, TVictim>(System.Func<TAnchor, TVictim, object> handler) 
        where TAnchor: class 
        where TVictim: class {
        return new System.Tuple<ASTPattern, ASTReducer>(
            info => CurrentElementIs<TAnchor>(info) && NextElementIs<TVictim>(info), 
            SingleNextConsumer<TAnchor, TVictim>(handler)
        );
    }

    public static ASTReducer SingleNextConsumer<TAnchor, TVictim>(System.Func<TAnchor, TVictim, object> handler)  
        where TAnchor: class 
        where TVictim: class {
        return (match, ctx) => {
            var anchor = ctx.GetCurrent(match) as TAnchor;
            var victim = match.Tokens[match.Position+1] as TVictim;

            object rv = anchor;
            if(handler != null) {
                rv = handler(anchor, victim);
            }
            
            match.Tokens.ReplaceRange(match.Position, match.Position+1, new object[] {rv});
        };
    }

    public static System.Tuple<ASTPattern, ASTReducer> FilterOutType<T>()  
        where T: class {
        return new System.Tuple<ASTPattern, ASTReducer>(
            info => CurrentElementIs<T>(info), 
            (match, ctx) => match.Tokens.ReplaceRange(match.Position, match.Position, new object[] {})
        );
    }
}

public class TokenPatternBuilder {
    
}
