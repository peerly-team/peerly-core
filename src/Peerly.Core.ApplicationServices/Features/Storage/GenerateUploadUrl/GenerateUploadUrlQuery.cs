using Peerly.Core.ApplicationServices.Abstractions;

namespace Peerly.Core.ApplicationServices.Features.Storage.GenerateUploadUrl;

public sealed record GenerateUploadUrlQuery : IQuery<GenerateUploadUrlQueryResponse>;
