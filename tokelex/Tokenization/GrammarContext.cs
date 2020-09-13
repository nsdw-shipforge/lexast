using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;

namespace NSDW.ShipForge.LexAST.Lexing {
    using TokenMatchHandler = System.Action<TokenMatchInfo, TokenizerContext>;
    public class RegexPoweredGrammarContext {
        List<RegexAndHandler> sources;

        Regex pattern;
        TokenMatchHandler[] handlersByGroup;

        Regex nonTokenPattern = new Regex("");

        public string NonTokenCharsValidator { 
            set {
                nonTokenPattern = new Regex(value);
            }
        }

        public GrammarContextDescription Description { get; }

        public RegexPoweredGrammarContext(GrammarContextDescription description, RegexAndHandler[] handlers) {
            sources = new List<RegexAndHandler>(handlers);
            Description = description;
            CompileCompoundRegex();
        }

        public TokenMatchInfo MatchToken(TokenizerContext ctx) {
            var match = pattern.Match(ctx.Input, ctx.CurrentPosition);

            // we ensure there are no unexpected characters in the substring from current pos to match start
            // use case: most programming languages would skip whitespace characters
            string beforeMatch = ctx.Input.Substring(ctx.CurrentPosition, match.Index-ctx.CurrentPosition);
            ValidateNonToken(beforeMatch);

            // find which group did the matching, map it back to the TokenMatchHandler and return
            for(int i = 1; i<match.Groups.Count; i++) {
                var g = match.Groups[i];
                if(g.Success) {
                    var rv = new TokenMatchInfo();
                    rv.Handler = 
                        // groups are basically one-indexed
                        handlersByGroup[i-1];
                    rv.MatchedTokens = g.Value;
                    rv.EndsAt = g.Index + g.Length;
                    return rv;
                }
            }

            var errStr = "Saw no valid tokens beyond this point: [@"+ctx.CurrentPosition+"] " + 
                ctx.Input.Substring(ctx.CurrentPosition, ctx.Input.ClampIndex(100))+"...";
            throw new System.InvalidOperationException(errStr);
        }

        private void CompileCompoundRegex() {
            var patternStr = GetCompoundRegexStr();
            Description.compoundRegex = patternStr;
            pattern = new Regex(patternStr);
        }

        private string GetCompoundRegexStr() {
            // these are low priority since we expect the provided matchers to be well written
            // still would be nice to validate here
            // TODO: validate regex only has 1 capturing group
            // TODO: validate regex has no named capturing groups
            
            handlersByGroup = new TokenMatchHandler[sources.Count];

            List<string> subpatterns = new List<string>();
            int i = 0;
            foreach(var source in sources) {
                subpatterns.Add(source.RegexPattern);
                handlersByGroup[i] = source.Handler;
                i++;
            }

            return string.Join("|",subpatterns);
        }

        private void ValidateNonToken(string str) {
            var match = nonTokenPattern.Match(str);
            if(match.Index != 0 || match.Length != str.Length) {
                throw new System.InvalidOperationException("Unexpected string in input: "+str);
            }
        }

        public struct RegexAndHandler {
            public string RegexPattern;
            public TokenMatchHandler Handler;

            public RegexAndHandler(string regexPattern, TokenMatchHandler handler) {
                RegexPattern = "("+regexPattern+")";
                Handler = handler;
            }
        }
    }

    public class GrammarContextDescription {
        public string name;
        public string compoundRegex;

        public GrammarContextDescription(string name) {
            this.name = name;
        }

        public override string ToString() {
            return name;
        }
    }

    public class TokenMatchInfo {
        public string MatchedTokens;
        public int EndsAt;
        public TokenMatchHandler Handler;
    }
}