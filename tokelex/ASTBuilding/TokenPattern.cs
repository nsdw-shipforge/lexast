using System.Collections;
using System.Collections.Generic;

namespace NSDW.ShipForge.LexAST.AST {
    using ASTPattern = System.Func<TokenPatternMatchInfo, bool>;
    using ASTReducer = System.Action<TokenPatternMatchInfo, TokenParserContext>;
    public static class TokenPatternUtil {
        public static bool InvalidPosition(TokenPatternMatchInfo info, int pos) {
            return pos >= info.Tokens.Count || pos < 0;
        }
        public static bool CurrentElementIs<T>(TokenPatternMatchInfo info) {
            if(InvalidPosition(info, info.Position)) {
                return false;
            }
            return info.Tokens[info.Position] is T;
        }

        public static bool NextElementIs<T>(TokenPatternMatchInfo info, int offset = 1) {
            if(InvalidPosition(info, info.Position+offset)) {
                return false;
            }
            return info.Tokens[info.Position+offset] is T;
        }

        public static bool PreviousElementIs<T>(TokenPatternMatchInfo info) {
            if(InvalidPosition(info, info.Position-1)) {
                return false;
            }
            return info.Tokens[info.Position-1] is T;
        }

        public static bool MatchBracketedBlock<TOpen, TClose>(TokenPatternMatchInfo info, System.Func<int, bool> acceptCount, int offset) {
            int? finalInnerTokenCounter = CountBracketedTokens<TOpen, TClose> (info, offset);
            if(finalInnerTokenCounter == null) {
                return false;
            }
            return acceptCount((int)finalInnerTokenCounter);
        }

        /// <summary>
        /// Counts the number of tokens within brackets defined as TOpen, TClose
        /// If the current token is not a TOpen, or if there is no matching TClose, this method returns null
        /// </summary>
        public static int? CountBracketedTokens<TOpen, TClose>(TokenPatternMatchInfo info, int offset) {
            int pos = info.Position + offset;
            if(pos >= info.Tokens.Count) { return null; }
            if(info.Tokens[pos] is TOpen is false) {
                return null;
            }
            int i;
            int nestingLevel = 0;
            int innerTokenCounter = 0;
            int? finalInnerTokenCounter = null;
            for(i = pos + 1; i<info.Tokens.Count; i++) {
                var currentToken = info.Tokens[i];
                if(currentToken is TClose) {
                    if(nestingLevel == 0) {
                        finalInnerTokenCounter = innerTokenCounter;
                        break;
                    } else {
                        nestingLevel--;
                    }
                }
                if(currentToken is TOpen) {
                    nestingLevel++;
                }
                innerTokenCounter++;
            }
            return finalInnerTokenCounter;
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

        public static System.Tuple<ASTPattern, ASTReducer> SingleAroundMatcherConsumer<TVictimL, TAnchor, TVictimR>(System.Func<TVictimL, TAnchor, TVictimR, object> handler) 
            where TAnchor: class 
            where TVictimL: class
            where TVictimR: class {
            return new System.Tuple<ASTPattern, ASTReducer>(
                info => PreviousElementIs<TVictimL>(info) && CurrentElementIs<TAnchor>(info) && NextElementIs<TVictimR>(info), 
                SingleAroundConsumer<TVictimL, TAnchor, TVictimR>(handler)
            );
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

        public static ASTReducer SingleAroundConsumer<TVictimL, TAnchor, TVictimR> (System.Func<TVictimL, TAnchor, TVictimR, object> handler)  
            where TAnchor: class 
            where TVictimL: class
            where TVictimR: class {
            return (match, ctx) => {
                var anchor = ctx.GetCurrent(match) as TAnchor;
                var victimLeft = match.Tokens[match.Position-1] as TVictimL;
                var victimRight = match.Tokens[match.Position+1] as TVictimR;

                object rv = anchor;
                if(handler != null) {
                    rv = handler(victimLeft, anchor, victimRight);
                }
                
                match.Tokens.ReplaceRange(match.Position-1, match.Position+1, new object[] {rv});
            };
        }

        public static System.Tuple<ASTPattern, ASTReducer> FilterOutType<T>()  
            where T: class {
            return new System.Tuple<ASTPattern, ASTReducer>(
                info => CurrentElementIs<T>(info), 
                (match, ctx) => match.Tokens.ReplaceRange(match.Position, match.Position, new object[] {})
            );
        }

        public static System.Tuple<ASTPattern, ASTReducer> ConvertingEach<TFrom, TInto> (System.Func<TFrom, TInto> strategy) 
            where TFrom: class 
            where TInto: class {
            return new System.Tuple<ASTPattern, ASTReducer>(
                info => CurrentElementIs<TFrom>(info), 
                (match, ctx) => {
                    var from = ctx.GetCurrent(match) as TFrom;
                    var into = strategy(from);
                    match.Tokens.ReplaceRange(match.Position, match.Position, new object[] {into});
                });
        }

        public static System.Tuple<ASTPattern, ASTReducer> BracketsMatcherConsumer<TOpeningBracket, TClosingBracket>(System.Func<TokenPatternMatchInfo, TokenList, object[]> handler, int offset = 0) 
            where TOpeningBracket: class
            where TClosingBracket: class {
            return new System.Tuple<ASTPattern, ASTReducer>(
                info => MatchBracketedBlock<TOpeningBracket, TClosingBracket>(info, amount => amount>=0, offset), 
                (match, ctx) => {
                    var innerCount = CountBracketedTokens<TOpeningBracket, TClosingBracket>(match, offset);
                    var bracketedSublist = match.Tokens.GetSublist(match.Position+1, match.Position + (int)innerCount);
                    var into = handler(match, bracketedSublist);
                    match.Tokens.ReplaceRange(match.Position, match.Position+(int)innerCount+1, into);
                }
            );
        }
    }
}
