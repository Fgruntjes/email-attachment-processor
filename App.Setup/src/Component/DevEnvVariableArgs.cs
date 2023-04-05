using Pulumi;

namespace App.Setup.Component;

public class DevEnvVariableArgs : ResourceArgs
{
    public Input<string> Name { get; set; } = null!;
    public Input<string> Value { get; set; } = null!;
}