using Pulumi;
using Pulumi.Gcp.Projects;

namespace App.Setup.Component;

public class GoogleSevices : ComponentResource
{
    public GoogleSevices(string name, GoogleSevicesArgs args, ComponentResourceOptions? options = null)
        : base("app:setup:GoogleServices", name, args, options)
    {
        args.Services.Apply(services =>
        {
            foreach (var service in services)
            {
                new Service(service, new()
                {
                    ServiceName = service,
                    Project = args.GoogleProjectId,
                    DisableOnDestroy = false,
                }, new CustomResourceOptions { Parent = this });
            }

            return services;
        });


    }
}