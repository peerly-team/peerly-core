using System;

namespace Peerly.Core.FileStorage.Configurations;

internal sealed class CephOptions
{
    public const string SectionName = "CephOptions";

    public Uri BaseUri { get; set; } = null!;
    public string BucketName { get; set; } = null!;
    public bool ForcePathStyle { get; set; }
    public TimeSpan ExpirationTime { get; set; }
    public TimeSpan LongLifeExpirationTime { get; set; }
}
