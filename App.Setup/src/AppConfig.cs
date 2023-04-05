using Pulumi;

namespace App.Setup;

public class AppConfig
{
    private readonly Config _config;
    public bool RepositoryIsOrganization => _config.RequireBoolean("repositoryIsOrganization");

    public string Slug => _config.Require("slug");
    public string Repository => _config.Require("repository");
    public string Region => _config.Require("region");

    public AppConfig()
    {
        _config = new Config("app");
    }
}