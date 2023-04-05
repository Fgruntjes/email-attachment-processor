using Pulumi;
using YamlDotNet.Serialization;

namespace App.Setup.Component;

public class GithubPulumiConfig : ComponentResource
{
    public GithubPulumiConfig(string name, GithubPulumiConfigArgs args, ComponentResourceOptions? options = null)
        : base("app:setup:PulumiConfig", name, args, options)
    {
        new GithubCiVariable("pulumi_config", new()
        {
            Value = args.ConfigMap.Apply(configMap =>
            {
                if (configMap == null || configMap.Count == 0)
                {
                    return "{}";
                }

                var serializer = new SerializerBuilder()
                    .WithQuotingNecessaryStrings()
                    .Build();

                return serializer.Serialize(configMap.ToDictionary(
                    kvp => kvp.Key,
                    kvp => new { value = kvp.Value.Value, secret = kvp.Value.IsSecret }));
            }),
            IsSecret = false,
            Repository = args.Repository,
        }, new ComponentResourceOptions { Parent = this });
    }
}