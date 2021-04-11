using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Formatting;

namespace NullAnalyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(NullAnalyzerCodeFixProvider)), Shared]
    public class NullAnalyzerCodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(NullAnalyzerAnalyzer.DiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;


            // Null checks in if-statements
            var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<IfStatementSyntax>().ToArray();

            if (declaration.Any())
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        title: CodeFixResources.CodeFixTitle,
                        createChangedDocument: c => DeleteIfNullCheckAsync(context.Document, declaration.First(), c),
                        equivalenceKey: nameof(CodeFixResources.CodeFixTitle)),
                    diagnostic);

                return;
            }

            // Null checks in conditional expressions.
            var declarationConditionalExpr = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<ConditionalExpressionSyntax>().ToArray();

            if (declarationConditionalExpr.Any())
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        title: CodeFixResources.CodeFixTitle,
                        createChangedDocument: c => DeleteConditionalNullCheckAsync(context.Document, declarationConditionalExpr.First(), c),
                        equivalenceKey: nameof(CodeFixResources.CodeFixTitle)),
                    diagnostic);
                return;
            }

            // Null checks in Coalesce expressions.
            var declarationCoalesceExpr = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<BinaryExpressionSyntax>().ToArray();

            if (declarationCoalesceExpr.Any())
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        title: CodeFixResources.CodeFixTitle,
                        createChangedDocument: c => DeleteCoalesceNullCheckAsync(context.Document, declarationCoalesceExpr.First(), c),
                        equivalenceKey: nameof(CodeFixResources.CodeFixTitle)),
                    diagnostic);
            }
        }

        /// <summary>
        /// Deletes if statement with null check.
        /// </summary>
        private async Task<Document> DeleteIfNullCheckAsync(Document document, IfStatementSyntax ifStatement, CancellationToken cancellationToken)
        {
            ElseClauseSyntax elseBlock = ifStatement.Else;

            SyntaxNode oldRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            SyntaxNode newRoot;

            if (elseBlock == null)
            {
                newRoot = oldRoot.RemoveNode(ifStatement, SyntaxRemoveOptions.KeepNoTrivia);
            }

            else if (elseBlock.Statement is IfStatementSyntax)
            {
                var formatted = elseBlock.Statement.WithAdditionalAnnotations(Formatter.Annotation);
                newRoot = oldRoot.ReplaceNode(ifStatement, formatted);
            }

            else
            {
                List<SyntaxNode> formatted = new List<SyntaxNode>();
                foreach (var child in elseBlock.Statement.ChildNodes())
                {
                    formatted.Add(child.WithAdditionalAnnotations(Formatter.Annotation));
                }
                newRoot = oldRoot.ReplaceNode(ifStatement, formatted);
            }

            // Return document with transformed tree.
            return document.WithSyntaxRoot(newRoot);
        }

        /// <summary>
        /// Removes coalesce expression as if the value that is being checked for null is never null.
        /// </summary>
        private async Task<Document> DeleteCoalesceNullCheckAsync(Document document, BinaryExpressionSyntax ceStatement, CancellationToken cancellationToken)
        {
            SyntaxNode oldRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            SyntaxNode newRoot = oldRoot.ReplaceNode(ceStatement, ceStatement.Left).WithAdditionalAnnotations(Formatter.Annotation);

            // Return document with transformed tree.
            return document.WithSyntaxRoot(newRoot);
        }

        /// <summary>
        /// Removes conditional expression with null check as if the value that is being checked for null is never null.
        /// </summary>
        private async Task<Document> DeleteConditionalNullCheckAsync(Document document, ConditionalExpressionSyntax ceStatement, CancellationToken cancellationToken)
        {
            SyntaxNode oldRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            SyntaxNode newRoot = oldRoot.ReplaceNode(ceStatement, ceStatement.WhenFalse).WithAdditionalAnnotations(Formatter.Annotation);

            // Return document with transformed tree.
            return document.WithSyntaxRoot(newRoot);
        }
    }
}
