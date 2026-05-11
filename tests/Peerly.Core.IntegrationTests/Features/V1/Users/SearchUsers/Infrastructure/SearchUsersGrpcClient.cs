using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using Peerly.Core.V1;

namespace Peerly.Core.IntegrationTests.Features.V1.Users.SearchUsers.Infrastructure;

public sealed class SearchUsersGrpcClient
{
    private static readonly Method<V1SearchUsersRequest, V1SearchUsersResponse> s_searchUsersMethod = new(
        MethodType.Unary,
        "peerly.core.v1.UserService",
        "V1SearchUsers",
        CreateMarshaller(V1SearchUsersRequest.Parser),
        CreateMarshaller(V1SearchUsersResponse.Parser));

    private readonly GrpcChannel _channel;

    public SearchUsersGrpcClient(GrpcChannel channel)
    {
        _channel = channel;
    }

    public async Task<V1SearchUsersResponse> V1SearchUsersAsync(
        V1SearchUsersRequest request,
        CancellationToken cancellationToken = default)
    {
        var call = _channel.CreateCallInvoker()
            .AsyncUnaryCall(
                s_searchUsersMethod,
                host: null,
                options: new CallOptions(cancellationToken: cancellationToken),
                request);

        return await call.ResponseAsync;
    }

    private static Marshaller<TMessage> CreateMarshaller<TMessage>(MessageParser<TMessage> parser)
        where TMessage : class, IMessage<TMessage>
    {
        return Marshallers.Create(
            message => message.ToByteArray(),
            parser.ParseFrom);
    }
}
