using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using Peerly.Core.V1;

namespace Peerly.Core.IntegrationTests.Features.V1.Homeworks.DeleteHomework.Infrastructure;

public sealed class DeleteHomeworkGrpcClient
{
    private static readonly Method<V1DeleteHomeworkRequest, V1DeleteHomeworkResponse> s_deleteHomeworkMethod = new(
        MethodType.Unary,
        "peerly.core.v1.HomeworkService",
        "V1DeleteHomework",
        CreateMarshaller(V1DeleteHomeworkRequest.Parser),
        CreateMarshaller(V1DeleteHomeworkResponse.Parser));

    private readonly GrpcChannel _channel;

    public DeleteHomeworkGrpcClient(GrpcChannel channel)
    {
        _channel = channel;
    }

    public async Task<V1DeleteHomeworkResponse> V1DeleteHomeworkAsync(
        V1DeleteHomeworkRequest request,
        CancellationToken cancellationToken = default)
    {
        var call = _channel.CreateCallInvoker()
            .AsyncUnaryCall(
                s_deleteHomeworkMethod,
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
