using System.Linq.Expressions;

namespace PUSpecSheet.Application.Published;

/// <summary>Swaps one lambda parameter for another, so two lambdas can be joined into one.</summary>
internal sealed class ParameterReplacer(ParameterExpression from, ParameterExpression to) : ExpressionVisitor
{
    protected override Expression VisitParameter(ParameterExpression node)
    {
        return node == from ? to : base.VisitParameter(node);
    }
}
