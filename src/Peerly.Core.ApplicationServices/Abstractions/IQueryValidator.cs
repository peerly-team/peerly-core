using System.Threading;
using System.Threading.Tasks;

namespace Peerly.Core.ApplicationServices.Abstractions;

internal interface IQueryValidator<in TQuery, TQueryResponse>
    where TQuery : IQuery<TQueryResponse>
{
    Task ValidateAsync(TQuery query, CancellationToken cancellationToken);
}
