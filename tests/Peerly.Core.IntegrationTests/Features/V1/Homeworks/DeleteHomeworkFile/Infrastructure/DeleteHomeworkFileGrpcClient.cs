using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using Peerly.Core.V1;

namespace Peerly.Core.IntegrationTests.Features.V1.Homeworks.DeleteHomeworkFile.Infrastructure;

public sealed class DeleteHomeworkFileGrpcClient
{
    private static readonly Method<V1DeleteHomeworkFileRequest, V1DeleteHomeworkFileResponse> s_deleteHomeworkFileMethod = new(
        MethodType.Unary,
        "peerly.core.v1.HomeworkService",
        "V1DeleteHomeworkFile",
        CreateMarshaller(V1DeleteHomeworkFileRequest.Parser),
        CreateMarshaller(V1DeleteHomeworkFileResponse.Parser));

    private readonly GrpcChannel _channel;

    public DeleteHomeworkFileGrpcClient(GrpcChannel channel)
    {
        _channel = channel;
    }

    public async Task<V1DeleteHomeworkFileResponse> V1DeleteHomeworkFileAsync(
        V1DeleteHomeworkFileRequest request,
        CancellationToken cancellationToken = default)
    {
        var call = _channel.CreateCallInvoker()
            .AsyncUnaryCall(
                s_deleteHomeworkFileMethod,
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
