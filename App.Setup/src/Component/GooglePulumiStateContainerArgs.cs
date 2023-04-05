using Pulumi;

namespace App.Setup.Component;

public class GooglePulumiStateContainerArgs : ResourceArgs
{
    public Input<string> GoogleProjectId { get; init; } = null!;
    public Input<string> GoogleRegion { get; init; } = null!;
}