using System.Threading;
using System.Threading.Tasks;

namespace Peerly.Core.ApplicationServices.Abstractions;

public interface IQueryHandler<in TQuery, TQueryResponse>
    where TQuery : IQuery<TQueryResponse>
{
    Task<TQueryResponse> Execute(TQuery query, CancellationToken cancellationToken);
}
