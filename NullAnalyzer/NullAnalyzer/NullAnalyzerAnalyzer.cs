using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace NullAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class NullAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "NullAnalyzer";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Usage";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(AnalyzeNullCheck_EqualsExpression, SyntaxKind.IfStatement);
            context.RegisterSyntaxNodeAction(AnalyzeNullCheck_IsPattern, SyntaxKind.IfStatement);
            context.RegisterSyntaxNodeAction(AnalyzeNullCheck_ReferenceEquals, SyntaxKind.IfStatement);
            context.RegisterSyntaxNodeAction(AnalyzeNullCheck_NotObject, SyntaxKind.IfStatement);

            context.RegisterSyntaxNodeAction(AnalyzeNullCheck_CoalesceExpression, SyntaxKind.CoalesceExpression);

            context.RegisterSyntaxNodeAction(AnalyzeNullCheck_EqualsConditionalExpression, SyntaxKind.ConditionalExpression);
            context.RegisterSyntaxNodeAction(AnalyzeNullCheck_IsConditionalExpression, SyntaxKind.ConditionalExpression);
            context.RegisterSyntaxNodeAction(AnalyzeNullCheck_ReferenceEqualsConditionalExpression, SyntaxKind.ConditionalExpression);
            context.RegisterSyntaxNodeAction(AnalyzeNullCheck_NotObjectConditionalExpression, SyntaxKind.ConditionalExpression);
        }

        /// <summary>
        /// Analyzes null checks like: if (obj == null)
        /// </summary>
        private void AnalyzeNullCheck_EqualsExpression(SyntaxNodeAnalysisContext context)
        {
            var ifStatement = (IfStatementSyntax)context.Node;
            EqualsCheck(context, ifStatement.Condition);
        }

        /// <summary>
        /// Analyzes null checks like: if (obj is null)
        /// </summary>
        private void AnalyzeNullCheck_IsPattern(SyntaxNodeAnalysisContext context)
        {
            var ifStatement = (IfStatementSyntax)context.Node;
            IsPatternCheck(context, ifStatement.Condition);
        }

        /// <summary>
        /// Analyzes null checks like: if (Object.ReferenceEquals(null, obj))
        /// </summary>
        private void AnalyzeNullCheck_ReferenceEquals(SyntaxNodeAnalysisContext context)
        {
            var ifStatement = (IfStatementSyntax)context.Node;
            ReferenceEqualCheck(context, ifStatement.Condition);
        }

        /// <summary>
        /// Analyzes null checks like: if (!(obj is object))
        /// </summary>
        private void AnalyzeNullCheck_NotObject(SyntaxNodeAnalysisContext context)
        {
            var ifStatement = (IfStatementSyntax)context.Node;
            NotObjectCheck(context, ifStatement.Condition);
        }

        /// <summary>
        /// Analyzes null checks like: obj = obj1 ?? obj2;
        /// </summary>
        private void AnalyzeNullCheck_CoalesceExpression(SyntaxNodeAnalysisContext context)
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation()));
        }

        /// <summary>
        /// Analyzes null checks like: obj = obj1 == null ? obj2 : obj3;
        /// </summary>
        private void AnalyzeNullCheck_EqualsConditionalExpression(SyntaxNodeAnalysisContext context)
        {
            var expr = (ConditionalExpressionSyntax)context.Node;
            EqualsCheck(context, expr.Condition);
        }

        /// <summary>
        /// Analyzes null checks like: obj = obj1 is null ? obj2 : obj3;
        /// </summary>
        private void AnalyzeNullCheck_IsConditionalExpression(SyntaxNodeAnalysisContext context)
        {
            var expr = (ConditionalExpressionSyntax)context.Node;
            IsPatternCheck(context, expr.Condition);
        }

        /// <summary>
        /// Analyzes null checks like: obj = Object.ReferenceEquals(null, obj) ? obj2 : obj3;
        /// </summary>
        private void AnalyzeNullCheck_ReferenceEqualsConditionalExpression(SyntaxNodeAnalysisContext context)
        {
            var expr = (ConditionalExpressionSyntax)context.Node;
            ReferenceEqualCheck(context, expr.Condition);
        }

        /// <summary>
        /// Analyzes null checks like: obj = !(obj is object) ? obj2 : obj3;
        /// </summary>
        private void AnalyzeNullCheck_NotObjectConditionalExpression(SyntaxNodeAnalysisContext context)
        {
            var expr = (ConditionalExpressionSyntax)context.Node;
            NotObjectCheck(context, expr.Condition);
        }


        private void EqualsCheck(SyntaxNodeAnalysisContext context, ExpressionSyntax conditionExpr)
        {
            if (conditionExpr.IsKind(SyntaxKind.EqualsExpression))
            {
                var equalsExpr = (BinaryExpressionSyntax)conditionExpr;

                if (!equalsExpr.Left.IsKind(SyntaxKind.NullLiteralExpression) &&
                    !equalsExpr.Right.IsKind(SyntaxKind.NullLiteralExpression))
                {
                    return;
                }
            }
            else
            {
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation()));
        }

        private void IsPatternCheck(SyntaxNodeAnalysisContext context, ExpressionSyntax conditionExpr)
        {
            if (conditionExpr.IsKind(SyntaxKind.IsPatternExpression))
            {
                var isExpr = (IsPatternExpressionSyntax)conditionExpr;

                if (!isExpr.Pattern.IsKind(SyntaxKind.ConstantPattern) ||
                    !((ConstantPatternSyntax)(isExpr.Pattern)).Expression.IsKind(SyntaxKind.NullLiteralExpression))
                {
                    return;
                }
            }
            else
            {
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation()));
        }
        
        private void ReferenceEqualCheck(SyntaxNodeAnalysisContext context, ExpressionSyntax conditionExpr)
        {
            if (conditionExpr.IsKind(SyntaxKind.InvocationExpression))
            {
                var condition = (InvocationExpressionSyntax)conditionExpr;
                var invocation = condition.Expression;
                var args = condition.ArgumentList;

                if (args.Arguments.Count < 2)
                {
                    return;
                }

                if (!args.Arguments[0].Expression.IsKind(SyntaxKind.NullLiteralExpression) &&
                    !args.Arguments[1].Expression.IsKind(SyntaxKind.NullLiteralExpression))
                {
                    return;
                }

                if (!invocation.IsKind(SyntaxKind.SimpleMemberAccessExpression))
                {
                    return;
                }

                var methodIdentifier = ((MemberAccessExpressionSyntax)invocation).Name;
                var obj = ((MemberAccessExpressionSyntax)invocation).Expression;

                if (obj.ToString() != "Object" ||
                    !methodIdentifier.IsKind(SyntaxKind.IdentifierName) ||
                    ((IdentifierNameSyntax)methodIdentifier).Identifier.ValueText != "ReferenceEquals")
                {
                    return;
                }
            }
            else
            {
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation()));
        }

        private void NotObjectCheck(SyntaxNodeAnalysisContext context, ExpressionSyntax conditionExpr)
        {
            if (!conditionExpr.IsKind(SyntaxKind.LogicalNotExpression))
            {
                return;
            }

            var operand = ((PrefixUnaryExpressionSyntax)conditionExpr).Operand;

            if (!operand.IsKind(SyntaxKind.ParenthesizedExpression))
            {
                return;
            }

            var inner = ((ParenthesizedExpressionSyntax)operand).Expression;

            if (inner.IsKind(SyntaxKind.IsExpression))
            {
                var isExpr = (BinaryExpressionSyntax)inner;

                if ((!isExpr.Right.IsKind(SyntaxKind.PredefinedType) ||
                     !((PredefinedTypeSyntax)isExpr.Right).Keyword.IsKind(SyntaxKind.ObjectKeyword))
                    &&
                    (!isExpr.Right.IsKind(SyntaxKind.IdentifierName) ||
                     ((IdentifierNameSyntax)isExpr.Right).Identifier.ValueText != "Object"))
                {
                    return;
                }
            }
            else
            {
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation()));
        }
    }
}
