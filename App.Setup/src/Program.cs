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
            Console.WriteLine($"Waiting for debugger (pid: {Environment.ProcessId})");
            while (!Debugger.IsAttached)
            {
                Console.WriteLine($"Waiting for debugger (pid: {Environment.ProcessId})");
                Thread.Sleep(500);
            }
        }

        return Deployment.RunAsync<SetupStack>();
    }
}