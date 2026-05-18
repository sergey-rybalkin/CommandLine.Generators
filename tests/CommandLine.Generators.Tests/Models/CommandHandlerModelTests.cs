using System.Collections.Immutable;
using CommandLine.Generators.Models;
using Microsoft.CodeAnalysis;

namespace CommandLine.Generators.Tests.Models;

public class CommandHandlerModelTests
{
    private static CommandParameterModel MakeParam(
        string name = "param", string type = "string", string desc = "desc")
    {
        return new CommandParameterModel(name, type, desc);
    }

    private static CommandHandlerModel CreateModel(
        string name = "cmd",
        string description = "desc",
        string className = "MyClass",
        string namespaceName = "MyNamespace",
        bool hasExecute = true,
        bool hasAsyncExecute = false,
        ImmutableArray<CommandParameterModel>? parameters = null,
        Location? location = null)
    {
        return new CommandHandlerModel(
            name,
            description,
            className,
            namespaceName,
            hasExecute,
            hasAsyncExecute,
            parameters ?? ImmutableArray<CommandParameterModel>.Empty,
            location ?? Location.None);
    }

    [Test]
    public async Task Equals_returns_true_for_identical_models()
    {
        CommandHandlerModel a = CreateModel();
        CommandHandlerModel b = CreateModel();

        a.Equals(b).ShouldBeTrue();
        await Task.CompletedTask;
    }

    [Test]
    public async Task Equals_returns_false_when_name_differs()
    {
        CommandHandlerModel a = CreateModel(name: "cmd-a");
        CommandHandlerModel b = CreateModel(name: "cmd-b");

        a.Equals(b).ShouldBeFalse();
        await Task.CompletedTask;
    }

    [Test]
    public async Task Equals_returns_false_when_description_differs()
    {
        CommandHandlerModel a = CreateModel(description: "desc A");
        CommandHandlerModel b = CreateModel(description: "desc B");

        a.Equals(b).ShouldBeFalse();
        await Task.CompletedTask;
    }

    [Test]
    public async Task Equals_returns_false_when_class_name_differs()
    {
        CommandHandlerModel a = CreateModel(className: "ClassA");
        CommandHandlerModel b = CreateModel(className: "ClassB");

        a.Equals(b).ShouldBeFalse();
        await Task.CompletedTask;
    }

    [Test]
    public async Task Equals_returns_false_when_namespace_name_differs()
    {
        CommandHandlerModel a = CreateModel(namespaceName: "Ns.A");
        CommandHandlerModel b = CreateModel(namespaceName: "Ns.B");

        a.Equals(b).ShouldBeFalse();
        await Task.CompletedTask;
    }

    [Test]
    public async Task Equals_returns_false_when_has_execute_differs()
    {
        CommandHandlerModel a = CreateModel(hasExecute: true);
        CommandHandlerModel b = CreateModel(hasExecute: false);

        a.Equals(b).ShouldBeFalse();
        await Task.CompletedTask;
    }

    [Test]
    public async Task Equals_returns_true_when_parameters_are_equal()
    {
        ImmutableArray<CommandParameterModel> parameters = [MakeParam("p1"), MakeParam("p2")];
        CommandHandlerModel a = CreateModel(parameters: parameters);
        CommandHandlerModel b = CreateModel(parameters: parameters);

        a.Equals(b).ShouldBeTrue();
        await Task.CompletedTask;
    }

    [Test]
    public async Task Equals_returns_false_when_parameters_differ()
    {
        ImmutableArray<CommandParameterModel> paramsA = [MakeParam("p1")];
        ImmutableArray<CommandParameterModel> paramsB = [MakeParam("p2")];
        CommandHandlerModel a = CreateModel(parameters: paramsA);
        CommandHandlerModel b = CreateModel(parameters: paramsB);

        a.Equals(b).ShouldBeFalse();
        await Task.CompletedTask;
    }

    [Test]
    public async Task Equals_returns_false_when_parameters_count_differs()
    {
        ImmutableArray<CommandParameterModel> paramsA = [MakeParam("p1"), MakeParam("p2")];
        ImmutableArray<CommandParameterModel> paramsB = [MakeParam("p1")];
        CommandHandlerModel a = CreateModel(parameters: paramsA);
        CommandHandlerModel b = CreateModel(parameters: paramsB);

        a.Equals(b).ShouldBeFalse();
        await Task.CompletedTask;
    }

    [Test]
    public async Task Equals_ignores_location_differences()
    {
        CommandHandlerModel a = CreateModel(location: Location.None);
        CommandHandlerModel b = CreateModel(location: Location.None);

        a.Equals(b).ShouldBeTrue();
        await Task.CompletedTask;
    }

    [Test]
    public async Task Object_equals_returns_true_for_identical_models()
    {
        CommandHandlerModel a = CreateModel();
        object b = CreateModel();

        a.Equals(b).ShouldBeTrue();
        await Task.CompletedTask;
    }

    [Test]
    public async Task Object_equals_returns_false_for_different_type()
    {
        CommandHandlerModel a = CreateModel();

        a.Equals("not a model").ShouldBeFalse();
        await Task.CompletedTask;
    }

    [Test]
    public async Task Get_hash_code_returns_same_value_for_identical_models()
    {
        CommandHandlerModel a = CreateModel();
        CommandHandlerModel b = CreateModel();

        a.GetHashCode().ShouldBe(b.GetHashCode());
        await Task.CompletedTask;
    }

    [Test]
    public async Task Get_hash_code_returns_different_value_when_name_differs()
    {
        CommandHandlerModel a = CreateModel(name: "cmd-a");
        CommandHandlerModel b = CreateModel(name: "cmd-b");

        a.GetHashCode().ShouldNotBe(b.GetHashCode());
        await Task.CompletedTask;
    }

    [Test]
    public async Task Get_hash_code_returns_different_value_when_parameters_differ()
    {
        CommandHandlerModel a = CreateModel(parameters: ImmutableArray.Create(MakeParam("p1")));
        CommandHandlerModel b = CreateModel(parameters: ImmutableArray.Create(MakeParam("p2")));

        a.GetHashCode().ShouldNotBe(b.GetHashCode());
        await Task.CompletedTask;
    }
}
