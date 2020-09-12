using System.Collections;
using System.Collections.Generic;

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
}

public class TokenPatternBuilder {
    
}
