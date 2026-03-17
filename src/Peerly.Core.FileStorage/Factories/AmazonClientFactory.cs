using Amazon.S3;
using Microsoft.Extensions.Options;
using Peerly.Core.FileStorage.Configurations;
using Peerly.Core.FileStorage.Factories.Abstractions;

namespace Peerly.Core.FileStorage.Factories;

internal sealed class AmazonClientFactory : IAmazonClientFactory
{
    private readonly CephCredentials _credential;
    private readonly CephOptions _options;

    public AmazonClientFactory(IOptions<CephCredentials> credentials, IOptions<CephOptions> options)
    {
        _credential = credentials.Value;
        _options = options.Value;
    }

    public AmazonS3Client Create()
    {
        var config = new AmazonS3Config
        {
            ServiceURL = _options.BaseUri.ToString(),
            ForcePathStyle = _options.ForcePathStyle,
            AuthenticationRegion = "ru-central1"
        };

        return new AmazonS3Client(_credential.AccessKey, _credential.SecretKey, config);
    }
}
