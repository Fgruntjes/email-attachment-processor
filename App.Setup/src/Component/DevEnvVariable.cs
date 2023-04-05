using Pulumi;

namespace App.Setup.Component;

public class DevEnvVariable : ComponentResource, IDisposable
{
    private static StreamWriter? _devEnvFileWriter;

    private static StreamWriter DevEnvFileWriter
    {
        get
        {
            if (_devEnvFileWriter == null)
            {
                var file = GetDeployEnvFile();
                if (File.Exists(file))
                {
                    File.Delete(file);
                }

                _devEnvFileWriter = File.CreateText(file);
            }

            return _devEnvFileWriter;
        }
    }

    public DevEnvVariable(string name, DevEnvVariableArgs args, ComponentResourceOptions? options = null)
        : base("app:setup:DevEnvVariable", name, args, options)
    {
        args.Name.Apply(name =>
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Argument cannot be null or empty.", nameof(args.Name));

            args.Value.Apply(value =>
            {
                var fileWriter = DevEnvFileWriter;
                var content = $"{name}='{value}'";

                lock (fileWriter)
                {
                    fileWriter.WriteLine(content);
                    fileWriter.Flush();
                }

                return content;
            });


            return name;
        });
    }



    private static string GetDeployEnvFile()
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        string envDefaultsFile;
        do
        {
            envDefaultsFile = Path.Combine(currentDirectory, ".env");
            currentDirectory = Directory.GetParent(currentDirectory)?.FullName;
        } while (!File.Exists(envDefaultsFile) && currentDirectory != null);

        if (currentDirectory == null)
            throw new FileNotFoundException("Could not find project root.", Path.Combine(Directory.GetCurrentDirectory(), ".env"));

        var setupDirectory = Path.GetDirectoryName(envDefaultsFile);

        if (setupDirectory == null)
            throw new FileNotFoundException("Could not find project root.", Path.Combine(Directory.GetCurrentDirectory(), ".env"));

        return Path.Combine(setupDirectory, ".deploy.env");
    }

    public void Dispose()
    {
        if (_devEnvFileWriter != null)
        {
            DevEnvFileWriter.Dispose();
        }
    }
}