using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Hosting;

namespace Peerly.Core.Api.Interceptors.ServiceVersions;

internal sealed class ServiceVersionInterceptor(
    IServiceVersionContext serviceVersionContext,
    IHostEnvironment env) : Interceptor
{
    private const string MetadataKey = "x-peerly-service-versions";

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        ReadIfStaging(context);
        return await continuation(request, context);
    }

    public override async Task<TResponse> ClientStreamingServerHandler<TRequest, TResponse>(
        IAsyncStreamReader<TRequest> requestStream,
        ServerCallContext context,
        ClientStreamingServerMethod<TRequest, TResponse> continuation)
    {
        ReadIfStaging(context);
        return await continuation(requestStream, context);
    }

    private void ReadIfStaging(ServerCallContext rpc)
    {
        if (!env.IsStaging())
            return;

        var entry = rpc.RequestHeaders.Get(MetadataKey);
        if (entry is null || string.IsNullOrWhiteSpace(entry.Value))
            return;

        var parsed = Parse(entry.Value);
        if (parsed.Count > 0)
            serviceVersionContext.Set(parsed);
    }

    private static IReadOnlyDictionary<string, string> Parse(string value)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var entry in value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var eq = entry.IndexOf('=');
            if (eq <= 0 || eq == entry.Length - 1) continue;
            var key = entry[..eq].Trim();
            var tag = entry[(eq + 1)..].Trim();
            if (key.Length == 0 || tag.Length == 0) continue;
            result[key] = tag;
        }
        return result;
    }
}
