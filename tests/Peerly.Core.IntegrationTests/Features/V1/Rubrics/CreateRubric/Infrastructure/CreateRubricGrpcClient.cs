using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using Peerly.Core.V1;

namespace Peerly.Core.IntegrationTests.Features.V1.Rubrics.CreateRubric.Infrastructure;

public sealed class CreateRubricGrpcClient
{
    private static readonly Method<V1CreateRubricRequest, V1CreateRubricResponse> s_createRubricMethod = new(
        MethodType.Unary,
        "peerly.core.v1.RubricService",
        "V1CreateRubric",
        CreateMarshaller(V1CreateRubricRequest.Parser),
        CreateMarshaller(V1CreateRubricResponse.Parser));

    private readonly GrpcChannel _channel;

    public CreateRubricGrpcClient(GrpcChannel channel)
    {
        _channel = channel;
    }

    public async Task<V1CreateRubricResponse> V1CreateRubricAsync(
        V1CreateRubricRequest request,
        CancellationToken cancellationToken = default)
    {
        var call = _channel.CreateCallInvoker()
            .AsyncUnaryCall(
                s_createRubricMethod,
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
