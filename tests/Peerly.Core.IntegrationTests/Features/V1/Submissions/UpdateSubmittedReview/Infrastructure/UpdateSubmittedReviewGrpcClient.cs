using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using Peerly.Core.V1;

namespace Peerly.Core.IntegrationTests.Features.V1.Submissions.UpdateSubmittedReview.Infrastructure;

public sealed class UpdateSubmittedReviewGrpcClient
{
    private static readonly Method<V1UpdateSubmittedReviewRequest, V1UpdateSubmittedReviewResponse> s_updateSubmittedReviewMethod = new(
        MethodType.Unary,
        "peerly.core.v1.SubmissionService",
        "V1UpdateSubmittedReview",
        CreateMarshaller(V1UpdateSubmittedReviewRequest.Parser),
        CreateMarshaller(V1UpdateSubmittedReviewResponse.Parser));

    private readonly GrpcChannel _channel;

    public UpdateSubmittedReviewGrpcClient(GrpcChannel channel)
    {
        _channel = channel;
    }

    public async Task<V1UpdateSubmittedReviewResponse> V1UpdateSubmittedReviewAsync(
        V1UpdateSubmittedReviewRequest request,
        CancellationToken cancellationToken = default)
    {
        var call = _channel.CreateCallInvoker()
            .AsyncUnaryCall(
                s_updateSubmittedReviewMethod,
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
