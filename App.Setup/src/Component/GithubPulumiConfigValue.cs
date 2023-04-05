namespace App.Setup.Component;

public class GithubPulumiConfigValue
{
    public string Value { get; } = null!;
    public bool IsSecret { get; } = false;

    public GithubPulumiConfigValue(string value, bool isSecret = false)
    {
        Value = value;
        IsSecret = isSecret;
    }
}