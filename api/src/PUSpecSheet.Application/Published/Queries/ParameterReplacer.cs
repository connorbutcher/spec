using System.Linq.Expressions;

namespace PUSpecSheet.Application.Published.Queries;

/// <summary>Swaps one lambda parameter for another, so two lambdas can be joined into one.</summary>
internal sealed class ParameterReplacer : ExpressionVisitor
{
    private readonly ParameterExpression from;
    private readonly ParameterExpression to;

    public ParameterReplacer(ParameterExpression from, ParameterExpression to)
    {
        this.from = from;
        this.to = to;
    }

    protected override Expression VisitParameter(ParameterExpression node)
    {
        return node == from ? to : base.VisitParameter(node);
    }
}
