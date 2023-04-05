using Pulumi;
using Pulumi.Gcp.ServiceAccount;

namespace App.Setup.Component;

public class GoogleCiServiceAccount : ComponentResource
{
    public Output<string> ServiceAccountEmail { get; private set; }

    public GoogleCiServiceAccount(string name, GoogleCiServiceAccountArgs args, ComponentResourceOptions? options = null)
        : base("app:setup:GoogleCiServiceAccount", name, args, options)
    {
        var serviceAccount = new Account("cicd-serviceAccount", new()
        {
            AccountId = "github-cicd",
            DisplayName = "Github CI/CD",
        }, new CustomResourceOptions { Parent = this });

        new IAMBinding("iam-workload-identity-binding", new()
        {
            Role = "roles/iam.workloadIdentityUser",
            ServiceAccountId = serviceAccount.Name,
            Members = new[]
            {
                args.IdentityPoolName
                    .Apply(pool => args.Repository
                        .Apply(repository => $"principalSet://iam.googleapis.com/{pool}/attribute.repository/{repository}"))
            }
        }, new CustomResourceOptions { Parent = this });

        var roles = args.Roles.Apply(roles =>
        {
            foreach (var role in roles)
            {
                new Pulumi.Gcp.Projects.IAMBinding($"role-{role}", new()
                {
                    Role = role,
                    Project = args.GoogleProjectId,
                    Members = new[]
                    {
                        serviceAccount.Email.Apply(email => $"serviceAccount:{email}")
                    }
                }, new CustomResourceOptions { Parent = this });
            }

            return roles;
        });

        ServiceAccountEmail = serviceAccount.Email;
    }
}