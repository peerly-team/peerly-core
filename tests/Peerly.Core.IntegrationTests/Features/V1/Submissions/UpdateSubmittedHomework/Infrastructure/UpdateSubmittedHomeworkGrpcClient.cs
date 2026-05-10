using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using Peerly.Core.V1;

namespace Peerly.Core.IntegrationTests.Features.V1.Submissions.UpdateSubmittedHomework.Infrastructure;

public sealed class UpdateSubmittedHomeworkGrpcClient
{
    private static readonly Method<V1UpdateSubmittedHomeworkRequest, V1UpdateSubmittedHomeworkResponse> s_updateSubmittedHomeworkMethod = new(
        MethodType.Unary,
        "peerly.core.v1.SubmissionService",
        "V1UpdateSubmittedHomework",
        CreateMarshaller(V1UpdateSubmittedHomeworkRequest.Parser),
        CreateMarshaller(V1UpdateSubmittedHomeworkResponse.Parser));

    private readonly GrpcChannel _channel;

    public UpdateSubmittedHomeworkGrpcClient(GrpcChannel channel)
    {
        _channel = channel;
    }

    public async Task<V1UpdateSubmittedHomeworkResponse> V1UpdateSubmittedHomeworkAsync(
        V1UpdateSubmittedHomeworkRequest request,
        CancellationToken cancellationToken = default)
    {
        var call = _channel.CreateCallInvoker()
            .AsyncUnaryCall(
                s_updateSubmittedHomeworkMethod,
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
