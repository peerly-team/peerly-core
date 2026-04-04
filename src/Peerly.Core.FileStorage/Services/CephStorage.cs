using System;
using System.IO;
using System.Threading;
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

    public async Task<Uri> GenerateUploadUrlAsync(StorageId storageId)
    {
        using var client = _amazonClientFactory.Create();
        var stringUri = await client.GetPreSignedURLAsync(
            new GetPreSignedUrlRequest
            {
                Expires = DateTime.Now.Add(_options.ExpirationTime),
                BucketName = _options.BucketName,
                Key = storageId.ToString(),
                Protocol = Protocol.HTTPS,
                Verb = HttpVerb.PUT
            });

        return new Uri(stringUri);
    }

    public async Task<Uri> GenerateDownloadUrlAsync(StorageId storageId, string originalObjectName)
    {
        using var client = _amazonClientFactory.Create();
        var stringUri = await client.GetPreSignedURLAsync(
            new GetPreSignedUrlRequest
            {
                Expires = DateTime.Now.Add(_options.ExpirationTime),
                BucketName = _options.BucketName,
                Key = storageId.ToString(),
                Protocol = Protocol.HTTPS,
                Verb = HttpVerb.GET,
                ResponseHeaderOverrides =
                {
                    ContentDisposition = $"attachment; filename={originalObjectName}"
                }
            });

        return new Uri(stringUri);
    }

    public async Task<Stream> GetObjectAsync(StorageId storageId, CancellationToken cancellationToken)
    {
        using var client = _amazonClientFactory.Create();
        var response = await client.GetObjectAsync(
            new GetObjectRequest
            {
                BucketName = _options.BucketName,
                Key = storageId.ToString()
            },
            cancellationToken);

        var memoryStream = new MemoryStream();
        await response.ResponseStream.CopyToAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;

        return memoryStream;
    }

    public async Task PutObjectAsync(StorageId storageId, Stream content, CancellationToken cancellationToken)
    {
        using var client = _amazonClientFactory.Create();
        await client.PutObjectAsync(
            new PutObjectRequest
            {
                BucketName = _options.BucketName,
                Key = storageId.ToString(),
                InputStream = content
            },
            cancellationToken);
    }
}
