using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using Peerly.Core.Abstractions.ApplicationServices;
using Peerly.Core.ApplicationServices.Services.Anonymization;
using Peerly.Core.ApplicationServices.Services.Anonymization.Models;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Students;
using Xunit;

namespace Peerly.Core.Tests.Services.Anonymization;

public sealed class FileAnonymizationServiceTests
{
    private byte[]? _uploadedBytes;
    private readonly Mock<IStorage> _storageMock = new();

    private readonly Fixture _fixture = new();
    private readonly FileAnonymizationService _service;

    public FileAnonymizationServiceTests()
    {
        _service = new FileAnonymizationService(_storageMock.Object);
    }

    [Fact]
    public async Task AnonymizeAsync_UnsupportedExtension_ShouldReturnNull()
    {
        // Arrange
        var request = CreateRequest("report.pdf", []);

        // Act
        var result = await _service.AnonymizeAsync(request, CancellationToken.None);

        // Assert
        result.Should().BeNull();
        _storageMock.Verify(
            s => s.GetObjectAsync(It.IsAny<StorageId>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task AnonymizeAsync_SupportedExtensionTxt_ShouldAnonymizeContentAndFileName()
    {
        // Arrange
        var email = _fixture.Create<string>();
        var name = _fixture.Create<string>();
        var content = $"Student {name} submitted by {email}";
        var student = CreateStudent(email, name);
        var request = CreateRequest($"{name}_hw.txt", [student]);
        SetupStorageWithContent(request.OriginalStorageId, content);

        // Act
        var result = await _service.AnonymizeAsync(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AnonymizedFileName.Should().NotContain(name);
        result.AnonymizedFileName.Should().Contain("[Студент X]");
        result.Size.Should().BeGreaterThan(0);

        var uploadedContent = CaptureUploadedContent();
        uploadedContent.Should().NotContain(name);
        uploadedContent.Should().NotContain(email);
        uploadedContent.Should().Contain("[Student X]");
        uploadedContent.Should().Contain("[Email X]");
    }

    [Fact]
    public async Task AnonymizeAsync_SupportedExtensionCsv_ShouldReturnNonNull()
    {
        // Arrange
        var student = CreateStudent(_fixture.Create<string>(), _fixture.Create<string>());
        var request = CreateRequest("data.csv", [student]);
        SetupStorageWithContent(request.OriginalStorageId, "some,csv,data");

        // Act
        var result = await _service.AnonymizeAsync(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task AnonymizeAsync_SupportedExtensionJson_ShouldReturnNonNull()
    {
        // Arrange
        var student = CreateStudent(_fixture.Create<string>(), _fixture.Create<string>());
        var request = CreateRequest("data.json", [student]);
        SetupStorageWithContent(request.OriginalStorageId, """{"key": "value"}""");

        // Act
        var result = await _service.AnonymizeAsync(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task AnonymizeAsync_SupportedExtensionXml_ShouldReturnNonNull()
    {
        // Arrange
        var student = CreateStudent(_fixture.Create<string>(), _fixture.Create<string>());
        var request = CreateRequest("data.xml", [student]);
        SetupStorageWithContent(request.OriginalStorageId, "<root>data</root>");

        // Act
        var result = await _service.AnonymizeAsync(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task AnonymizeAsync_CaseInsensitiveExtension_ShouldAnonymize()
    {
        // Arrange
        var email = _fixture.Create<string>();
        var student = CreateStudent(email, _fixture.Create<string>());
        var request = CreateRequest("DATA.TXT", [student]);
        SetupStorageWithContent(request.OriginalStorageId, email);

        // Act
        var result = await _service.AnonymizeAsync(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task AnonymizeAsync_StudentWithNullName_ShouldOnlyReplaceEmail()
    {
        // Arrange
        var email = _fixture.Create<string>();
        var content = $"Контакт {email} for details";
        var student = CreateStudent(email, null);
        var request = CreateRequest("file.txt", [student]);
        SetupStorageWithContent(request.OriginalStorageId, content);

        // Act
        var result = await _service.AnonymizeAsync(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();

        var uploadedContent = CaptureUploadedContent();
        uploadedContent.Should().NotContain(email);
        uploadedContent.Should().Contain("[Почта X]");
    }

    [Fact]
    public async Task AnonymizeAsync_PiiInFileName_ShouldAnonymizeFileName()
    {
        // Arrange
        var email = _fixture.Create<string>();
        var student = CreateStudent(email, _fixture.Create<string>());
        var request = CreateRequest($"{email}_hw.txt", [student]);
        SetupStorageWithContent(request.OriginalStorageId, "content");

        // Act
        var result = await _service.AnonymizeAsync(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.AnonymizedFileName.Should().Be("[Почта X]_hw.txt");
    }

    private static AnonymizationRequest CreateRequest(string fileName, IReadOnlyCollection<Student> students)
    {
        return new AnonymizationRequest
        {
            OriginalStorageId = (StorageId)Guid.NewGuid(),
            FileName = fileName,
            Students = students
        };
    }

    private Student CreateStudent(string email, string? name)
    {
        return new Student
        {
            Id = new StudentId(_fixture.Create<long>()),
            Email = email,
            Name = name
        };
    }

    private void SetupStorageWithContent(StorageId storageId, string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        _storageMock
            .Setup(s => s.GetObjectAsync(storageId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MemoryStream(bytes));

        _storageMock
            .Setup(s => s.PutObjectAsync(It.IsAny<StorageId>(), It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Callback<StorageId, Stream, CancellationToken>(
                (_, stream, _) =>
                {
                    using var copy = new MemoryStream();
                    stream.CopyTo(copy);
                    _uploadedBytes = copy.ToArray();
                })
            .Returns(Task.CompletedTask);
    }

    private string CaptureUploadedContent()
    {
        _storageMock.Invocations
            .Where(i => i.Method.Name == nameof(IStorage.PutObjectAsync))
            .Should().NotBeEmpty();

        _uploadedBytes.Should().NotBeNull();
        return Encoding.UTF8.GetString(_uploadedBytes!);
    }
}
