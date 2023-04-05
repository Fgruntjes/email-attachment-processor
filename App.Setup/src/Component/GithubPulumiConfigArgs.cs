using Pulumi;

namespace App.Setup.Component;

public class GithubPulumiConfigArgs : ResourceArgs
{
    public Input<string> Repository { get; init; } = null!;
    public InputMap<GithubPulumiConfigValue> ConfigMap { get; init; } = null!;
}
