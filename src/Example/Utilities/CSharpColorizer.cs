using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Example;

public sealed class CSharpColorizer
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
        private readonly StringBuilder _result;

        public ColorizerSyntaxWalker(SyntaxWalkerDepth depth = SyntaxWalkerDepth.Node)
            : base(depth)
        {
            _result = new StringBuilder();
        }

        public List<string> GetResult()
        {
            return _result.ToString()
                .Replace("\r\n", "\n", StringComparison.OrdinalIgnoreCase)
                .Replace("\r", string.Empty, StringComparison.OrdinalIgnoreCase)
                .TrimEnd('\n')
                .Split(new string[] { "\n" }, StringSplitOptions.None)
                .ToList();
        }

        protected override void VisitToken(SyntaxToken token)
        {
            ProcessTrivia(token.LeadingTrivia);

            if (token.IsKeyword())
            {
                _result.Append("[blue]").Append(token.ToString().EscapeMarkup()).Append("[/]");
            }
            else
            {
                if (token.Kind() == SyntaxKind.IdentifierToken)
                {
                    _result.Append("[white]").Append(token.ToString().EscapeMarkup()).Append("[/]");
                }
                else if (token.Kind() == SyntaxKind.StringLiteralToken)
                {
                    _result.Append("[grey]").Append(token.ToString().EscapeMarkup()).Append("[/]");
                }
                else
                {
                    _result.Append("[silver]").Append(token.ToString().EscapeMarkup()).Append("[/]");
                }
            }

            ProcessTrivia(token.TrailingTrivia);

            base.VisitToken(token);
        }

        private void ProcessTrivia(SyntaxTriviaList list)
        {
            foreach (var trivia in list)
            {
                if (trivia.IsKind(SyntaxKind.SingleLineCommentTrivia))
                {
                    _result.Append("[green]").Append(trivia.ToString().EscapeMarkup()).Append("[/]");
                }
                else if (trivia.IsKind(SyntaxKind.MultiLineCommentTrivia))
                {
                    var result = trivia.ToString().Split("\r\n", StringSplitOptions.None);
                    foreach (var (_, _, last, item) in result.Enumerate())
                    {
                        _result.Append("[green]").Append(item.EscapeMarkup()).Append("[/]");
                        if (!last)
                        {
                            _result.Append("\r\n");
                        }
                    }
                }
                else
                {
                    _result.Append(trivia.ToString().EscapeMarkup());
                }
            }
        }
    }
}
