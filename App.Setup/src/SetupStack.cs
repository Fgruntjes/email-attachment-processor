using Pulumi;
using Pulumi.Gcp.Iam;
using Pulumi.Gcp.Iam.Inputs;
using Pulumi.Gcp.Projects;
using Pulumi.Gcp.ServiceAccount;
using Pulumi.Github;
using Pulumi.Github.Inputs;
using IAMBinding = Pulumi.Gcp.ServiceAccount.IAMBinding;

namespace App.Setup;

public class SetupStack : Stack
{
    private readonly AppConfig _appConfig;

    private string? _deployEnvFile;
    private string DeployEnvFile => _deployEnvFile ??= GetDeployEnvFile();

    public SetupStack()
    {
        _appConfig = new AppConfig();

        ClearDeployEnvFile();
        GcpServices();
        GithubWorkloadIdentity();
        GithubBranchProtection();
    }

    private void GithubBranchProtection()
    {
        // TODO: Auto detect if branch is owned by organisation.
        if (!_appConfig.RepositoryIsOrganization)
            return;

        new BranchProtectionV3("branch-protection", new()
        {
            Repository = _appConfig.Repository,
            Branch = "[bm][ea][ti][an]",
            EnforceAdmins = true,
            RequireConversationResolution = true,
            RequiredStatusChecks = new BranchProtectionV3RequiredStatusChecksArgs()
            {

                Strict = true,
                IncludeAdmins = true,
                Checks = new[]
                {
                    "test_quality_success",
                    "test_unit_success",
                    "test_integration_success",
                }
            }
        });
    }

    private void GcpServices()
    {
        new Service("project", new()
        {
            Project = _appConfig.Slug,
            ServiceName = "iam.googleapis.com",
        });
    }

    private void GithubWorkloadIdentity()
    {
        var identityPool = new WorkloadIdentityPool("github", new()
        {
            Project = _appConfig.Slug,
            WorkloadIdentityPoolId = "github",
        });

        var identityPoolProvider = new WorkloadIdentityPoolProvider("github-provider", new()
        {
            Project = _appConfig.Slug,
            WorkloadIdentityPoolId = identityPool.WorkloadIdentityPoolId,
            WorkloadIdentityPoolProviderId = "github-provider",
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
        });

        var serviceAccount = new Account("cicd-serviceAccount", new()
        {
            AccountId = "github-cicd",
            DisplayName = "Github CI/CD",
        });

        new IAMBinding("iam-workload-identity-binding", new()
        {
            Role = "roles/iam.workloadIdentityUser",
            ServiceAccountId = serviceAccount.Name,
            Members = new[]
            {
                identityPool.Name.Apply(pool => $"principalSet://iam.googleapis.com/{pool}/attribute.repository/{_appConfig.Repository}")
            }
        });

        SetVariable("GOOGLE_WORKLOAD_IDENTITY_PROVIDER", identityPoolProvider.Name, false);
        SetVariable("GOOGLE_SERVICE_ACCOUNT", serviceAccount.Email, false);
        SetVariable("GOOGLE_PROJECT_ID", _appConfig.Slug, false);
        SetVariable("GOOGLE_REGION", _appConfig.Region, false);
    }

    private void SetVariable(string key, Output<string> valueOutput, bool secret)
    {
        valueOutput.Apply(value => SetVariable(key, value, secret));
    }

    private string SetVariable(string key, string value, bool secret)
    {
        if (secret)
        {
            new ActionsSecret(key, new()
            {
                Repository = _appConfig.Repository
                    .Split('/', 2)
                    .Last(),
                SecretName = key,
                PlaintextValue = value
            });
            new DependabotSecret(key, new()
            {
                Repository = _appConfig.Repository
                    .Split('/', 2)
                    .Last(),
                SecretName = key,
                PlaintextValue = value
            });
        }
        else
        {
            new ActionsVariable(key, new()
            {
                Repository = _appConfig.Repository
                .Split('/', 2)
                .Last(),
                VariableName = key,
                Value = value
            });
        }

        new DevEnvVariable(key, new()
        {
            Name = key,
            Value = value,
            File = DeployEnvFile
        });

        return value;
    }

    private string GetDeployEnvFile()
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        string setupScript;
        do
        {
            setupScript = Path.Combine(currentDirectory, "App.Setup", "setup.sh");
            currentDirectory = Directory.GetParent(currentDirectory)?.FullName;
        } while (!File.Exists(setupScript) && currentDirectory != null);

        if (currentDirectory == null)
            throw new FileNotFoundException("Could not find setup `App.Setup/setup.sh`.");

        var setupDirectory = Directory.GetParent(setupScript)?.FullName;

        if (setupDirectory == null)
            throw new FileNotFoundException("Could not find setup `App.Setup/setup.sh`.");

        return Path.Combine(setupDirectory, ".env.deploy.local");
    }

    private void ClearDeployEnvFile()
    {
        if (File.Exists(DeployEnvFile))
            File.Delete(DeployEnvFile);
    }
}