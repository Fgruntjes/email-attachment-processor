using Pulumi;
using Pulumi.Gcp.Storage;
using Pulumi.Gcp.Storage.Inputs;

namespace App.Setup.Component;

public class GooglePulumiStateContainer : ComponentResource
{
    public GooglePulumiStateContainer(string name, GooglePulumiStateContainerArgs args, ComponentResourceOptions? options = null)
        : base("app:setup:GooglePulumiStateContainer", name, args, options)
    {
        new Bucket("pulumi-state", new()
        {
            Project = args.GoogleProjectId,
            Location = args.GoogleRegion,
            StorageClass = "REGIONAL",
            PublicAccessPrevention = "inherited",
            Versioning = new BucketVersioningArgs { Enabled = true },
            LifecycleRules = new[]
            {
                new BucketLifecycleRuleArgs
                {
                    Action = new BucketLifecycleRuleActionArgs
                    {
                        Type = "Delete"
                    },
                    Condition = new BucketLifecycleRuleConditionArgs
                    {
                        NumNewerVersions = 3
                    }
                }
            }
        }, new CustomResourceOptions { Parent = this });
    }
}