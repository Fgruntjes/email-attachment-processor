using Pulumi;

namespace App.Setup;

internal class DevEnvVariableArgs : ResourceArgs
{
    public string File { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Value { get; set; } = null!;
}