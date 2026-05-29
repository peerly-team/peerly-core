using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using Peerly.Core.V1;

namespace Peerly.Core.IntegrationTests.Features.V1.Rubrics.DeleteRubric.Infrastructure;

public sealed class DeleteRubricGrpcClient
{
    private static readonly Method<V1DeleteRubricRequest, V1DeleteRubricResponse> s_deleteRubricMethod = new(
        MethodType.Unary,
        "peerly.core.v1.RubricService",
        "V1DeleteRubric",
        CreateMarshaller(V1DeleteRubricRequest.Parser),
        CreateMarshaller(V1DeleteRubricResponse.Parser));

    private readonly GrpcChannel _channel;

    public DeleteRubricGrpcClient(GrpcChannel channel)
    {
        _channel = channel;
    }

    public async Task<V1DeleteRubricResponse> V1DeleteRubricAsync(
        V1DeleteRubricRequest request,
        CancellationToken cancellationToken = default)
    {
        var call = _channel.CreateCallInvoker()
            .AsyncUnaryCall(
                s_deleteRubricMethod,
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
