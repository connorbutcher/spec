using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using PUSpecSheet.Api.Controllers;
using PUSpecSheet.Domain.Users;

namespace PUSpecSheet.Api.Tests.Authorization;

/// <summary>
/// Which endpoints ask for a permission, read from the controllers themselves, so a new action can't be
/// added without deciding who may call it.
/// </summary>
public sealed class EndpointPermissionTests
{
    private static readonly List<(Type Controller, MethodInfo Action)> Actions = typeof(PhasesController).Assembly
        .GetTypes()
        .Where(type => type.IsSubclassOf(typeof(ControllerBase)) && !type.IsAbstract)
        .SelectMany(controller => controller
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(method => method.GetCustomAttributes<HttpMethodAttribute>().Any())
            .Select(action => (controller, action)))
        .ToList();

    [Fact]
    public void EveryActionThatChangesSomething_AsksForAPermission()
    {
        var unprotected = Actions
            .Where(entry => !IsOpen(entry.Controller) && !IsRead(entry.Action) && PolicyOf(entry.Controller, entry.Action) is null)
            .Select(entry => $"{entry.Controller.Name}.{entry.Action.Name}")
            .ToList();

        Assert.Empty(unprotected);
    }

    [Fact]
    public void EveryPolicyAsked_IsAPermissionKey()
    {
        var policies = Actions
            .Select(entry => PolicyOf(entry.Controller, entry.Action))
            .OfType<string>()
            .Distinct()
            .ToList();

        Assert.NotEmpty(policies);
        Assert.All(policies, policy => Assert.Contains(policy, PermissionKeys.All));
    }

    [Fact]
    public void EveryPermissionKey_GuardsAtLeastOneAction()
    {
        var used = Actions.Select(entry => PolicyOf(entry.Controller, entry.Action)).OfType<string>().ToHashSet();

        Assert.Equal(PermissionKeys.All.Order(), used.Order());
    }

    [Fact]
    public void OnlyThePublishedApi_IsOpenWithoutSigningIn_AndItOnlyReads()
    {
        var open = Actions.Where(entry => IsOpen(entry.Controller)).ToList();

        Assert.NotEmpty(open);
        Assert.All(open, entry =>
        {
            Assert.StartsWith("Published", entry.Controller.Name);
            Assert.StartsWith("api/published", entry.Controller.GetCustomAttribute<RouteAttribute>()!.Template);
        });
    }

    [Theory]
    [InlineData(typeof(SheetsController), nameof(SheetsController.Publish), PermissionKeys.SheetsPublish)]
    [InlineData(typeof(SheetsController), nameof(SheetsController.AddTable), PermissionKeys.SheetsEdit)]
    [InlineData(typeof(SheetRowsController), nameof(SheetRowsController.SaveValues), PermissionKeys.SheetsEdit)]
    [InlineData(typeof(TemplateCellsController), nameof(TemplateCellsController.Update), PermissionKeys.TemplatesManage)]
    [InlineData(typeof(TableTemplatesController), nameof(TableTemplatesController.Delete), PermissionKeys.TemplatesManage)]
    [InlineData(typeof(CellTypesController), nameof(CellTypesController.Create), PermissionKeys.CellTypesManage)]
    [InlineData(typeof(PhasesController), nameof(PhasesController.Move), PermissionKeys.PhasesManage)]
    [InlineData(typeof(SheetTypesController), nameof(SheetTypesController.Create), PermissionKeys.SheetTypesManage)]
    public void AnAction_AsksForThePermissionThatFitsIt(Type controller, string action, string permission)
    {
        Assert.Equal(permission, PolicyOf(controller, controller.GetMethod(action)!));
    }

    [Theory]
    [InlineData(typeof(SheetsController), nameof(SheetsController.Get))]
    [InlineData(typeof(TableTemplatesController), nameof(TableTemplatesController.GetAll))]
    [InlineData(typeof(CellTypesController), nameof(CellTypesController.GetAll))]
    [InlineData(typeof(CurrentUserController), nameof(CurrentUserController.Get))]
    public void Reading_NeedsOnlyASignedInUser(Type controller, string action)
    {
        Assert.Null(PolicyOf(controller, controller.GetMethod(action)!));
        Assert.False(IsOpen(controller));
    }

    private static bool IsOpen(Type controller)
    {
        return controller.GetCustomAttribute<AllowAnonymousAttribute>() is not null;
    }

    private static bool IsRead(MethodInfo action)
    {
        return action.GetCustomAttributes<HttpMethodAttribute>().All(attribute => attribute is HttpGetAttribute);
    }

    private static string? PolicyOf(Type controller, MethodInfo action)
    {
        return action.GetCustomAttribute<AuthorizeAttribute>()?.Policy
            ?? controller.GetCustomAttribute<AuthorizeAttribute>()?.Policy;
    }
}
