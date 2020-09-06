using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Example
{
    public sealed class Colorizer
    {
        public static List<string> Colorize(string source)
        {
            var tree = CSharpSyntaxTree.ParseText(source);
            var walker = new ColorizerSyntaxWalker(SyntaxWalkerDepth.StructuredTrivia);
            walker.Visit(tree.GetRoot());

            return walker.GetResult();
        }

        private sealed class ColorizerSyntaxWalker : SyntaxWalker
        {
            private readonly List<string> _result;
            private readonly StringBuilder _line;

            public ColorizerSyntaxWalker(SyntaxWalkerDepth depth = SyntaxWalkerDepth.Node)
                : base(depth)
            {
                _result = new List<string>();
                _line = new StringBuilder();
            }

            public List<string> GetResult()
            {
                if (_line.Length > 0)
                {
                    _result.Add(_line.ToString());
                    _line.Clear();
                }

                return _result;
            }

            protected override void VisitToken(SyntaxToken token)
            {
                if (token.LeadingTrivia != null)
                {
                    var leading = token.LeadingTrivia.ToString().EscapeMarkup();
                    if (leading == "\r\n")
                    {
                        _result.Add(_line.ToString());
                        _line.Clear();
                    }
                    else
                    {
                        _line.Append(leading);
                    }
                }

                if (token.IsKeyword())
                {
                    _line.Append("[blue]" + token.ToString().EscapeMarkup() + "[/]");
                }
                else
                {
                    if (token.Kind() == SyntaxKind.IdentifierToken)
                    {
                        _line.Append("[white]" + token.ToString().EscapeMarkup() + "[/]");
                    }
                    else if (token.Kind() == SyntaxKind.StringLiteralToken)
                    {
                        _line.Append("[grey]" + token.ToString().EscapeMarkup() + "[/]");
                    }
                    else
                    {
                        _line.Append(token.ToString().EscapeMarkup());
                    }
                }

                if (token.TrailingTrivia != null)
                {
                    var trailing = token.TrailingTrivia.ToString();
                    if (trailing.EndsWith("\r\n"))
                    {
                        _result.Add(_line.ToString());
                        _line.Clear();
                    }
                    else
                    {
                        _line.Append(trailing);
                    }
                }

                base.VisitToken(token);
            }
        }
    }
}
