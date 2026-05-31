using CommandLine.Generators.Models;

namespace CommandLine.Generators.Tests.Models;

public class CommandParameterModelTests
{
    private static CommandParameterModel CreateModel(
        string parameterName = "param",
        string parameterTypeName = "string",
        string description = "desc",
        string? valueHint = null,
        char? alias = null)
    {
        return new CommandParameterModel(parameterName, parameterTypeName, description, valueHint, alias);
    }

    [Test]
    public void Equals_returns_true_for_identical_models()
    {
        CommandParameterModel a = CreateModel();
        CommandParameterModel b = CreateModel();

        a.Equals(b).ShouldBeTrue();
    }

    [Test]
    public void Equals_returns_false_when_parameter_name_differs()
    {
        CommandParameterModel a = CreateModel(parameterName: "foo");
        CommandParameterModel b = CreateModel(parameterName: "bar");

        a.Equals(b).ShouldBeFalse();
    }

    [Test]
    public void Equals_returns_false_when_parameter_type_name_differs()
    {
        CommandParameterModel a = CreateModel(parameterTypeName: "int");
        CommandParameterModel b = CreateModel(parameterTypeName: "string");

        a.Equals(b).ShouldBeFalse();
    }

    [Test]
    public void Equals_returns_false_when_description_differs()
    {
        CommandParameterModel a = CreateModel(description: "desc A");
        CommandParameterModel b = CreateModel(description: "desc B");

        a.Equals(b).ShouldBeFalse();
    }

    [Test]
    public void Equals_returns_false_when_value_hint_differs()
    {
        CommandParameterModel a = CreateModel(valueHint: "hint");
        CommandParameterModel b = CreateModel(valueHint: null);

        a.Equals(b).ShouldBeFalse();
    }

    [Test]
    public void Equals_returns_false_when_alias_differs()
    {
        CommandParameterModel a = CreateModel(alias: 'x');
        CommandParameterModel b = CreateModel(alias: 'y');

        a.Equals(b).ShouldBeFalse();
    }

    [Test]
    public void Equals_returns_true_when_all_optional_fields_are_null_or_default()
    {
        CommandParameterModel a = CreateModel(valueHint: null, alias: null);
        CommandParameterModel b = CreateModel(valueHint: null, alias: null);

        a.Equals(b).ShouldBeTrue();
    }

    [Test]
    public void Object_equals_returns_true_for_identical_models()
    {
        CommandParameterModel a = CreateModel();
        object b = CreateModel();

        a.Equals(b).ShouldBeTrue();
    }

    [Test]
    public void Object_equals_returns_false_for_different_type()
    {
        CommandParameterModel a = CreateModel();

        a.Equals("not a model").ShouldBeFalse();
    }

    [Test]
    public void Get_hash_code_returns_same_value_for_identical_models()
    {
        CommandParameterModel a = CreateModel();
        CommandParameterModel b = CreateModel();

        a.GetHashCode().ShouldBe(b.GetHashCode());
    }

    [Test]
    public void Get_hash_code_returns_different_value_for_different_models()
    {
        CommandParameterModel a = CreateModel(parameterName: "foo");
        CommandParameterModel b = CreateModel(parameterName: "bar");

        a.GetHashCode().ShouldNotBe(b.GetHashCode());
    }
}
