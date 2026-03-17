using Peerly.Core.ApplicationServices.Abstractions;

namespace Peerly.Core.ApplicationServices.Features.V1.Storage.GenerateUploadUrl;

public sealed record GenerateUploadUrlQuery : IQuery<GenerateUploadUrlQueryResponse>;
