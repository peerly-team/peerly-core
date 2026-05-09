using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using Peerly.Core.V1;

namespace Peerly.Core.IntegrationTests.Features.V1.Submissions.CorrectSubmittedHomeworkMark.Infrastructure;

public sealed class CorrectSubmittedHomeworkMarkGrpcClient
{
    private static readonly Method<V1CorrectSubmittedHomeworkMarkRequest, V1CorrectSubmittedHomeworkMarkResponse> s_correctSubmittedHomeworkMarkMethod = new(
        MethodType.Unary,
        "peerly.core.v1.SubmissionService",
        "V1CorrectSubmittedHomeworkMark",
        CreateMarshaller(V1CorrectSubmittedHomeworkMarkRequest.Parser),
        CreateMarshaller(V1CorrectSubmittedHomeworkMarkResponse.Parser));

    private readonly GrpcChannel _channel;

    public CorrectSubmittedHomeworkMarkGrpcClient(GrpcChannel channel)
    {
        _channel = channel;
    }

    public async Task<V1CorrectSubmittedHomeworkMarkResponse> V1CorrectSubmittedHomeworkMarkAsync(
        V1CorrectSubmittedHomeworkMarkRequest request,
        CancellationToken cancellationToken = default)
    {
        var call = _channel.CreateCallInvoker()
            .AsyncUnaryCall(
                s_correctSubmittedHomeworkMarkMethod,
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
