using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using Peerly.Core.V1;

namespace Peerly.Core.IntegrationTests.Features.V1.Submissions.DeleteSubmittedHomeworkFile.Infrastructure;

public sealed class DeleteSubmittedHomeworkFileGrpcClient
{
    private static readonly Method<V1DeleteSubmittedHomeworkFileRequest, V1DeleteSubmittedHomeworkFileResponse> s_deleteSubmittedHomeworkFileMethod = new(
        MethodType.Unary,
        "peerly.core.v1.SubmissionService",
        "V1DeleteSubmittedHomeworkFile",
        CreateMarshaller(V1DeleteSubmittedHomeworkFileRequest.Parser),
        CreateMarshaller(V1DeleteSubmittedHomeworkFileResponse.Parser));

    private readonly GrpcChannel _channel;

    public DeleteSubmittedHomeworkFileGrpcClient(GrpcChannel channel)
    {
        _channel = channel;
    }

    public async Task<V1DeleteSubmittedHomeworkFileResponse> V1DeleteSubmittedHomeworkFileAsync(
        V1DeleteSubmittedHomeworkFileRequest request,
        CancellationToken cancellationToken = default)
    {
        var call = _channel.CreateCallInvoker()
            .AsyncUnaryCall(
                s_deleteSubmittedHomeworkFileMethod,
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
