using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.ApplicationServices.Services.Anonymization.Models;

namespace Peerly.Core.ApplicationServices.Services.Anonymization.Abstractions;

internal interface IFileAnonymizationService
{
    Task<AnonymizationResult?> AnonymizeAsync(AnonymizationRequest request, CancellationToken cancellationToken);
}
