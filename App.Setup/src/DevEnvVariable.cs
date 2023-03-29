using Pulumi;

namespace App.Setup;

internal class DevEnvVariable : ComponentResource
{
    public DevEnvVariable(string name, DevEnvVariableArgs args, ComponentResourceOptions? options = null)
        : base("app:setup:DevEnvVariable", name, args, options)
    {
        if (string.IsNullOrWhiteSpace(args.File))
            throw new ArgumentException("Argument cannot be null or empty.", nameof(args.File));
        if (string.IsNullOrWhiteSpace(args.Name))
            throw new ArgumentException("Argument cannot be null or empty.", nameof(args.Name));
        if (string.IsNullOrWhiteSpace(args.Value))
            throw new ArgumentException("Argument cannot be null or empty.", nameof(args.Value));

        var content = $"${args.Name}='${args.Value}'";
        if (!File.Exists(args.File))
        {
            using var sw = File.CreateText(args.File);
            sw.WriteLine(content);
        }
        else
        {
            using var sw = File.AppendText(args.File);
            sw.WriteLine(content);
        }
    }
}