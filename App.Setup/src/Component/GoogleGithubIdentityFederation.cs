using Google.Protobuf.WellKnownTypes;
using Pulumi;
using Pulumi.Gcp.Iam;
using Pulumi.Gcp.Iam.Inputs;

namespace App.Setup.Component;

public class GoogleGithubIdentityFederation : ComponentResource
{
    public Output<string> IdentityPoolName { get; private set; }
    public Output<string> IdentityPoolProviderName { get; private set; }

    public GoogleGithubIdentityFederation(string name, GoogleGithubIdentityFederationArgs args, ComponentResourceOptions? options = null)
        : base("app:setup:GithubGoogleIdentityFederation", name, args, options)
    {
        var identityPool = new WorkloadIdentityPool("gh-pool", new()
        {
            Project = args.GoogleProjectId,
            WorkloadIdentityPoolId = "gh-pool",
        }, new CustomResourceOptions { Parent = this });
        IdentityPoolName = identityPool.Name;

        var identityPoolProvider = new WorkloadIdentityPoolProvider("gh-provider", new()
        {
            Project = args.GoogleProjectId,
            WorkloadIdentityPoolId = identityPool.WorkloadIdentityPoolId,
            WorkloadIdentityPoolProviderId = "gh-provider",
            Oidc = new WorkloadIdentityPoolProviderOidcArgs
            {
                IssuerUri = "https://token.actions.githubusercontent.com"
            },
            AttributeMapping =
            {
                {"google.subject", "assertion.sub"},
                {"attribute.actor", "assertion.actor"},
                {"attribute.repository", "assertion.repository"}
            }
        }, new CustomResourceOptions { Parent = this });
        IdentityPoolProviderName = identityPoolProvider.Name;
    }
}