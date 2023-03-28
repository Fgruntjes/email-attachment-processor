using System.Diagnostics;
using Pulumi;

namespace App.Setup;

class Program
{
    static Task<int> Main()
    {
        bool.TryParse(Environment.GetEnvironmentVariable("PULUMI_DEBUG"), out var debug);

        if (debug)
        {
            while (!Debugger.IsAttached)
            {
                Thread.Sleep(100);
            }
        }

        return Deployment.RunAsync<SetupStack>();
    }
}