using App.Setup.Component;
using Pulumi;
using Pulumi.Github;
using Pulumi.Github.Inputs;

namespace App.Setup;

public class SetupStack : Stack
{
    private readonly AppConfig _appConfig;

    public SetupStack()
    {
        _appConfig = new AppConfig();
        var services = new GoogleSevices("services", new()
        {
            GoogleProjectId = _appConfig.Slug,
            Services = new[]
            {
                "iam.googleapis.com",
                "artifactregistry.googleapis.com",
            }
        });

        var identityFederation = new GoogleGithubIdentityFederation("identity-federation", new()
        {
            GoogleProjectId = _appConfig.Slug,
            GoogleRegion = _appConfig.Region,
        }, new ComponentResourceOptions { Parent = this });

        var serviceAccount = new GoogleCiServiceAccount("ci-service-account", new()
        {
            GoogleProjectId = _appConfig.Slug,
            IdentityPoolName = identityFederation.IdentityPoolName,
            Repository = _appConfig.Repository,
        }, new ComponentResourceOptions { Parent = this });
        CreateVariable("google_service_account", serviceAccount.ServiceAccountEmail);

        var registry = new GoogleContainerRegistry("registry", new()
        {
            GoogleProjectId = _appConfig.Slug,
            GoogleRegion = _appConfig.Region,
        }, new ComponentResourceOptions { Parent = this, DependsOn = new[] { services } });
        CreateVariable("docker_registry", registry.RegistryUrn);


        if (_appConfig.RepositoryIsOrganization)
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
            }, new CustomResourceOptions { Parent = this });


        CreateVariable("google_workload_identity_provider", identityFederation.IdentityPoolProviderName);
        CreateVariable("google_project_id", _appConfig.Slug);
        CreateVariable("google_region", _appConfig.Region);

        new GithubPulumiConfig("gcp:project", new()
        {
            Repository = _appConfig.Repository,
            ConfigMap = new InputMap<GithubPulumiConfigValue>() {
                {"gcp:project", new GithubPulumiConfigValue(_appConfig.Slug)},
                {"gcp:region", new GithubPulumiConfigValue(_appConfig.Region)},
            },
        }, new ComponentResourceOptions { Parent = this });
    }

    private void CreateVariable(string name, Input<string> value)
    {
        new GithubCiVariable(name, new()
        {
            Value = value,
            IsSecret = false,
            Repository = _appConfig.Repository,
        }, new ComponentResourceOptions { Parent = this });
    }
}