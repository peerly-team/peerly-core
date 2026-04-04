using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.ApplicationServices;
using Peerly.Core.ApplicationServices.Services.Anonymization.Abstractions;
using Peerly.Core.ApplicationServices.Services.Anonymization.Models;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Students;
using UtfUnknown;

namespace Peerly.Core.ApplicationServices.Services.Anonymization;

internal sealed class FileAnonymizationService : IFileAnonymizationService
{
    private static readonly HashSet<string> s_supportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".txt", ".csv", ".json", ".xml"
    };

    private readonly IStorage _storage;

    public FileAnonymizationService(IStorage storage)
    {
        _storage = storage;
    }

    public async Task<AnonymizationResponse?> AnonymizeAsync(
        AnonymizationRequest request,
        CancellationToken cancellationToken)
    {
        var extension = Path.GetExtension(request.FileName);
        if (!s_supportedExtensions.Contains(extension))
        {
            return null;
        }

        await using var originalStream = await _storage.GetObjectAsync(request.OriginalStorageId, cancellationToken);
        var (content, encoding) = await ReadContentAsync(originalStream, cancellationToken);

        var anonymizedContent = ReplaceStudentPiiInContent(content, request.Students);
        var anonymizedBytes = encoding.GetBytes(anonymizedContent);
        var anonymizedStorageId = (StorageId)Guid.NewGuid();

        await using var uploadStream = new MemoryStream(anonymizedBytes);
        await _storage.PutObjectAsync(anonymizedStorageId, uploadStream, cancellationToken);

        return new AnonymizationResponse
        {
            AnonymizedStorageId = anonymizedStorageId,
            Size = anonymizedBytes.Length
        };
    }

    private static async Task<(string Content, Encoding Encoding)> ReadContentAsync(
        Stream stream,
        CancellationToken cancellationToken)
    {
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms, cancellationToken);
        var bytes = ms.ToArray();

        var detectionResult = CharsetDetector.DetectFromBytes(bytes);
        var encoding = detectionResult.Detected?.Encoding ?? Encoding.UTF8;

        return (encoding.GetString(bytes), encoding);
    }

    private static string ReplaceStudentPiiInContent(string content, IReadOnlyCollection<Student> students)
    {
        var replacements = GetReplacementModels(students);

        foreach (var (original, replacement) in replacements)
        {
            content = content.Replace(original, replacement, StringComparison.OrdinalIgnoreCase);
        }

        return content;
    }

    private static List<ReplacementModel> GetReplacementModels(IReadOnlyCollection<Student> students)
    {
        var result = new List<ReplacementModel>(2 * students.Count);
        foreach (var student in students)
        {
            result.Add(new ReplacementModel(student.Email, "[Почта X]"));

            if (student.Name is not null)
            {
                result.Add(new ReplacementModel(student.Name, "[Студент X]"));
            }
        }

        result.Sort((a, b) => b.Original.Length.CompareTo(a.Original.Length));

        return result;
    }

    private sealed record ReplacementModel(string Original, string Replacement);
}
