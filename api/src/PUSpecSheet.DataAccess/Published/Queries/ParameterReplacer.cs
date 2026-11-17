using System.Linq.Expressions;

namespace PUSpecSheet.DataAccess.Published.Queries;

/// <summary>Swaps one lambda parameter for another, so two lambdas can be joined into one.</summary>
internal sealed class ParameterReplacer : ExpressionVisitor
{
    private readonly ParameterExpression _from;
    private readonly ParameterExpression _to;

    public ParameterReplacer(ParameterExpression from, ParameterExpression to)
    {
        _from = from;
        _to = to;
    }

    protected override Expression VisitParameter(ParameterExpression node)
    {
        return node == _from ? _to : base.VisitParameter(node);
    }
}
