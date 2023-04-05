using Pulumi;
using Pulumi.Gcp.ArtifactRegistry;

namespace App.Setup.Component;

public class GoogleContainerRegistry : ComponentResource
{
    public Output<string> RegistryUrn { get; private set; }
    public GoogleContainerRegistry(string name, GoogleContainerRegistryArgs args, ComponentResourceOptions? options = null)
    : base("app:setup:GoogleContainerRegistry", name, args, options)
    {
        var registry = new Repository(name, new()
        {
            Project = args.GoogleProjectId,
            Location = args.GoogleRegion,
            RepositoryId = name,
            Format = "DOCKER",
        }, new CustomResourceOptions { Parent = this });

        RegistryUrn = args.GoogleProjectId.Apply(projectId =>
            args.GoogleRegion.Apply(region =>
                registry.Name.Apply(name => $"{region}-docker.pkg.dev/{projectId}/{name}")));
    }
}