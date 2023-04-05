using Pulumi;
using Pulumi.Github;

namespace App.Setup.Component;

public class GithubCiVariable : ComponentResource
{
    public GithubCiVariable(string name, GithubCiVariableArgs args, ComponentResourceOptions? options = null)
        : base("app:setup:CiVariable", name, args, options)
    {
        var variableName = args.Name.Apply(variableName => variableName = variableName ?? name);
        var repository = args.Repository.Apply(repository => repository.Split('/', 2).Last());
        if (args.IsSecret)
        {
            CreateSecret(name, args, variableName, repository);
        }
        else
        {
            CreateVariable(name, args, variableName, repository);
        }

        new DevEnvVariable(name, new()
        {
            Name = variableName,
            Value = args.Value,
        }, new ComponentResourceOptions { Parent = this });
    }

    private void CreateVariable(string name, GithubCiVariableArgs args, Output<string> variableName, Output<string> repository)
    {
        new ActionsVariable(name, new()
        {
            Repository = repository,
            VariableName = variableName,
            Value = args.Value
        }, new CustomResourceOptions { Parent = this });
    }

    private void CreateSecret(string name, GithubCiVariableArgs args, Output<string> variableName, Output<string> repository)
    {
        new ActionsSecret(name, new()
        {
            Repository = repository,
            SecretName = variableName,
            PlaintextValue = args.Value
        }, new CustomResourceOptions { Parent = this });
        new DependabotSecret(name, new()
        {
            Repository = repository,
            SecretName = variableName,
            PlaintextValue = args.Value
        }, new CustomResourceOptions { Parent = this });
    }
}