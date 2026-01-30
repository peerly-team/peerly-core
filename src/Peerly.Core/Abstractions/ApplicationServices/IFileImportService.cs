using System.Threading.Tasks;
using Peerly.Core.Models.Storage;

namespace Peerly.Core.Abstractions.ApplicationServices;

public interface IFileImportService
{
    Task<GeneratedUploadUrlInfo> GenerateUploadUrl();
}
