using Pulumi;

namespace App.Setup.Component;

public class GithubCiVariableArgs : ResourceArgs
{
    public bool IsSecret { get; set; } = true;
    public Input<string> Value { get; set; } = null!;
    public Input<string> Repository { get; internal set; } = null!;
    public Input<string>? Name { get; set; }
}