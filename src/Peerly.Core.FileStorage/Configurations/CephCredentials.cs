namespace Peerly.Core.FileStorage.Configurations;

internal sealed class CephCredentials
{
    public const string SectionName = "Ceph";

    public string AccessKey { get; set; } = null!;
    public string SecretKey { get; set; } = null!;
}
