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

    public async Task<AnonymizationResult?> AnonymizeAsync(
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

        var anonymizedContent = ReplaceStudentPiiInContent(content, request.Students, encoding);
        var anonymizedFileName = ReplaceStudentPiiInContent(request.FileName, request.Students);
        var anonymizedBytes = encoding.GetBytes(anonymizedContent);
        var anonymizedStorageId = (StorageId)Guid.NewGuid();

        await using var uploadStream = new MemoryStream(anonymizedBytes);
        await _storage.PutObjectAsync(anonymizedStorageId, uploadStream, cancellationToken);

        return new AnonymizationResult
        {
            AnonymizedStorageId = anonymizedStorageId,
            Size = anonymizedBytes.Length,
            AnonymizedFileName = anonymizedFileName
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

    private static string ReplaceStudentPiiInContent(
        string content,
        IReadOnlyCollection<Student> students,
        Encoding? encoding = null)
    {
        var replacements = GetReplacementModels(students, encoding);

        foreach (var (original, replacement) in replacements)
        {
            content = content.Replace(original, replacement, StringComparison.OrdinalIgnoreCase);
        }

        return content;
    }

    private static List<ReplacementModel> GetReplacementModels(IReadOnlyCollection<Student> students, Encoding? encoding)
    {
        var result = new List<ReplacementModel>(2 * students.Count);
        var emailReplacement = CanEncode(encoding, "[Почта X]") ? "[Почта X]" : "[Email X]";
        var studentReplacement = CanEncode(encoding, "[Студент X]") ? "[Студент X]" : "[Student X]";
        foreach (var student in students)
        {
            result.Add(new ReplacementModel(student.Email, emailReplacement));

            if (student.Name is not null)
            {
                result.Add(new ReplacementModel(student.Name, studentReplacement));
            }
        }

        result.Sort((a, b) => b.Original.Length.CompareTo(a.Original.Length));

        return result;
    }

    private static bool CanEncode(Encoding? encoding, string content)
    {
        if (encoding is null)
        {
            return true;
        }

        return encoding.GetString(encoding.GetBytes(content)) == content;
    }

    private sealed record ReplacementModel(string Original, string Replacement);
}
