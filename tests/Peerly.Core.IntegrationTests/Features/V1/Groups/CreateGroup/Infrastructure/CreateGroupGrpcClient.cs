using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using Peerly.Core.V1;

namespace Peerly.Core.IntegrationTests.Features.V1.Groups.CreateGroup.Infrastructure;

public sealed class CreateGroupGrpcClient
{
    private static readonly Method<V1CreateGroupRequest, V1CreateGroupResponse> s_createGroupMethod = new(
        MethodType.Unary,
        "peerly.core.v1.GroupService",
        "V1CreateGroup",
        CreateMarshaller(V1CreateGroupRequest.Parser),
        CreateMarshaller(V1CreateGroupResponse.Parser));

    private readonly GrpcChannel _channel;

    public CreateGroupGrpcClient(GrpcChannel channel)
    {
        _channel = channel;
    }

    public async Task<V1CreateGroupResponse> V1CreateGroupAsync(
        V1CreateGroupRequest request,
        CancellationToken cancellationToken = default)
    {
        var call = _channel.CreateCallInvoker()
            .AsyncUnaryCall(
                s_createGroupMethod,
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
