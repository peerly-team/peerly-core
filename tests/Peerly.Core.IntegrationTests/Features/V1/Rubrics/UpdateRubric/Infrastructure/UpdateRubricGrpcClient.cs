using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using Peerly.Core.V1;

namespace Peerly.Core.IntegrationTests.Features.V1.Rubrics.UpdateRubric.Infrastructure;

public sealed class UpdateRubricGrpcClient
{
    private static readonly Method<V1UpdateRubricRequest, V1UpdateRubricResponse> s_updateRubricMethod = new(
        MethodType.Unary,
        "peerly.core.v1.RubricService",
        "V1UpdateRubric",
        CreateMarshaller(V1UpdateRubricRequest.Parser),
        CreateMarshaller(V1UpdateRubricResponse.Parser));

    private readonly GrpcChannel _channel;

    public UpdateRubricGrpcClient(GrpcChannel channel)
    {
        _channel = channel;
    }

    public async Task<V1UpdateRubricResponse> V1UpdateRubricAsync(
        V1UpdateRubricRequest request,
        CancellationToken cancellationToken = default)
    {
        var call = _channel.CreateCallInvoker()
            .AsyncUnaryCall(
                s_updateRubricMethod,
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
