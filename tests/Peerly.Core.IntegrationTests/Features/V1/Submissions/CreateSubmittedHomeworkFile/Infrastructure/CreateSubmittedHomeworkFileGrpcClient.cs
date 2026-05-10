using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using Peerly.Core.V1;

namespace Peerly.Core.IntegrationTests.Features.V1.Submissions.CreateSubmittedHomeworkFile.Infrastructure;

public sealed class CreateSubmittedHomeworkFileGrpcClient
{
    private static readonly Method<V1CreateSubmittedHomeworkFileRequest, V1CreateSubmittedHomeworkFileResponse> s_createSubmittedHomeworkFileMethod = new(
        MethodType.Unary,
        "peerly.core.v1.SubmissionService",
        "V1CreateSubmittedHomeworkFile",
        CreateMarshaller(V1CreateSubmittedHomeworkFileRequest.Parser),
        CreateMarshaller(V1CreateSubmittedHomeworkFileResponse.Parser));

    private readonly GrpcChannel _channel;

    public CreateSubmittedHomeworkFileGrpcClient(GrpcChannel channel)
    {
        _channel = channel;
    }

    public async Task<V1CreateSubmittedHomeworkFileResponse> V1CreateSubmittedHomeworkFileAsync(
        V1CreateSubmittedHomeworkFileRequest request,
        CancellationToken cancellationToken = default)
    {
        var call = _channel.CreateCallInvoker()
            .AsyncUnaryCall(
                s_createSubmittedHomeworkFileMethod,
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
