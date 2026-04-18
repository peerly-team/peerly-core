namespace Peerly.Core.ApplicationServices.Executors.Shared.Abstractions;

internal interface IMassExecutorOptions
{
    int MaxDegreeOfParallelism { get; }
}
