using Pulumi;

namespace App.Setup.Component;

public class GoogleSevicesArgs : ResourceArgs
{
    public Input<string[]> Services { get; set; } = null!;
    public Input<string> GoogleProjectId { get; set; } = null!;
}