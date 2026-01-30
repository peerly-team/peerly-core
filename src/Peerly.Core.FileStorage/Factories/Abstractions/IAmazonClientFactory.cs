using Amazon.S3;

namespace Peerly.Core.FileStorage.Factories.Abstractions;

internal interface IAmazonClientFactory
{
    AmazonS3Client Create();
}
