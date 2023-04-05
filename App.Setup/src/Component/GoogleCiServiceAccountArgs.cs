using Pulumi;

namespace App.Setup.Component;

public class GoogleCiServiceAccountArgs : ResourceArgs
{
    public Input<string> GoogleProjectId { get; init; } = null!;
    public Input<string> IdentityPoolName { get; init; } = null!;
    public Input<string> Repository { get; init; } = null!;
    public Input<string[]> Roles { get; init; } = new string[]{
        "roles/iam.serviceAccountUser",
        "roles/artifactregistry.repoAdmin",
        "roles/artifactregistry.writer",
        "roles/storage.objectAdmin",
    };
}