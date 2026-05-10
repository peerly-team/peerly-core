using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using Peerly.Core.V1;

namespace Peerly.Core.IntegrationTests.Features.V1.Submissions.DeleteSubmittedHomework.Infrastructure;

public sealed class DeleteSubmittedHomeworkGrpcClient
{
    private static readonly Method<V1DeleteSubmittedHomeworkRequest, V1DeleteSubmittedHomeworkResponse> s_deleteSubmittedHomeworkMethod = new(
        MethodType.Unary,
        "peerly.core.v1.SubmissionService",
        "V1DeleteSubmittedHomework",
        CreateMarshaller(V1DeleteSubmittedHomeworkRequest.Parser),
        CreateMarshaller(V1DeleteSubmittedHomeworkResponse.Parser));

    private readonly GrpcChannel _channel;

    public DeleteSubmittedHomeworkGrpcClient(GrpcChannel channel)
    {
        _channel = channel;
    }

    public async Task<V1DeleteSubmittedHomeworkResponse> V1DeleteSubmittedHomeworkAsync(
        V1DeleteSubmittedHomeworkRequest request,
        CancellationToken cancellationToken = default)
    {
        var call = _channel.CreateCallInvoker()
            .AsyncUnaryCall(
                s_deleteSubmittedHomeworkMethod,
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
