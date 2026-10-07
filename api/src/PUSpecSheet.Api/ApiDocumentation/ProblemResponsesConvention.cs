using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace PUSpecSheet.Api.ApiDocumentation;

/// <summary>
/// Declares the problem details responses the exception handler produces, so controllers only carry
/// their happy path: 400 where a request can be invalid, 403 where a permission is asked for, 404 where the
/// address names something, and 409 where a change can conflict with the current state. It only affects the documentation.
/// </summary>
public sealed class ProblemResponsesConvention : IActionModelConvention
{
    private const string PublishedRoutePrefix = "api/published";

    public void Apply(ActionModel action)
    {
        var declared = action.Filters
            .OfType<IApiResponseMetadataProvider>()
            .Select(provider => provider.StatusCode)
            .ToHashSet();

        // Declaring any response replaces the one inferred from the return type, so that one is declared too.
        if (!declared.Any(status => status is >= 200 and < 300) && ResultTypeOf(action) is { } result)
        {
            action.Filters.Add(new ProducesResponseTypeAttribute(result, StatusCodes.Status200OK));
        }

        var isPublished = IsPublished(action);
        var isRead = IsRead(action);
        var takesInput = action.Parameters.Any(parameter => IsFrom(parameter, BindingSource.Body) || IsFrom(parameter, BindingSource.Query));
        var namesSomething = RouteTemplates(action).Any(template => template.Contains('{', StringComparison.Ordinal));

        // The published endpoints only read, whatever their HTTP method, so they never conflict.
        var changesState = !isRead && !isPublished;

        if (takesInput || changesState)
        {
            AddProblem(action, declared, StatusCodes.Status400BadRequest);
        }

        if (NeedsPermission(action))
        {
            AddProblem(action, declared, StatusCodes.Status403Forbidden);
        }

        if (namesSomething || isPublished)
        {
            AddProblem(action, declared, StatusCodes.Status404NotFound);
        }

        if (changesState)
        {
            AddProblem(action, declared, StatusCodes.Status409Conflict);
        }
    }

    private static void AddProblem(ActionModel action, HashSet<int> declared, int status)
    {
        if (declared.Add(status))
        {
            action.Filters.Add(new ProducesResponseTypeAttribute(typeof(ProblemDetails), status, ProblemResponseDescriptionsTransformer.ContentType));
        }
    }

    /// <summary>Whether the action, or its whole controller, asks for a permission policy.</summary>
    private static bool NeedsPermission(ActionModel action)
    {
        return action.Attributes
            .Concat(action.Controller.Attributes)
            .OfType<AuthorizeAttribute>()
            .Any(authorize => !string.IsNullOrEmpty(authorize.Policy));
    }

    private static bool IsRead(ActionModel action)
    {
        return action.Selectors
            .SelectMany(selector => selector.ActionConstraints.OfType<HttpMethodActionConstraint>())
            .SelectMany(constraint => constraint.HttpMethods)
            .All(HttpMethods.IsGet);
    }

    private static bool IsPublished(ActionModel action)
    {
        return RouteTemplates(action).Any(template => template.StartsWith(PublishedRoutePrefix, StringComparison.OrdinalIgnoreCase));
    }

    private static IEnumerable<string> RouteTemplates(ActionModel action)
    {
        return action.Selectors
            .Concat(action.Controller.Selectors)
            .Select(selector => selector.AttributeRouteModel?.Template)
            .OfType<string>();
    }

    private static bool IsFrom(ParameterModel parameter, BindingSource source)
    {
        return parameter.BindingInfo?.BindingSource is { } actual && actual.CanAcceptDataFrom(source);
    }

    /// <summary>The <c>T</c> of an action that returns <c>ActionResult&lt;T&gt;</c>, whether or not it is awaited.</summary>
    private static Type? ResultTypeOf(ActionModel action)
    {
        var type = action.ActionMethod.ReturnType;
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>))
        {
            type = type.GetGenericArguments()[0];
        }

        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ActionResult<>)
            ? type.GetGenericArguments()[0]
            : null;
    }
}
