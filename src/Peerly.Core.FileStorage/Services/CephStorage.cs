using System;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using Peerly.Core.Abstractions.ApplicationServices;
using Peerly.Core.FileStorage.Configurations;
using Peerly.Core.FileStorage.Factories.Abstractions;
using Peerly.Core.Identifiers;

namespace Peerly.Core.FileStorage.Services;

internal sealed class CephStorage : IStorage
{
    private readonly IAmazonClientFactory _amazonClientFactory;
    private readonly CephOptions _options;

    public CephStorage(IAmazonClientFactory amazonClientFactory, IOptions<CephOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _amazonClientFactory = amazonClientFactory;
        _options = options.Value;
    }

    public async Task<Uri> GenerateUploadUrlAsync(TemporaryStorageId temporaryStorageId)
    {
        using var client = _amazonClientFactory.Create();
        var stringUri = await client.GetPreSignedURLAsync(
            new GetPreSignedUrlRequest
            {
                Expires = DateTime.Now.Add(_options.ExpirationTime),
                BucketName = _options.BucketName,
                Key = temporaryStorageId.ToString(),
                Protocol = Protocol.HTTPS,
                Verb = HttpVerb.PUT
            });

        return new Uri(stringUri);
    }
}
