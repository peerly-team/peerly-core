using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using Peerly.Core.V1;

namespace Peerly.Core.IntegrationTests.Features.V1.Submissions.DeleteSubmittedReview.Infrastructure;

public sealed class DeleteSubmittedReviewGrpcClient
{
    private static readonly Method<V1DeleteSubmittedReviewRequest, V1DeleteSubmittedReviewResponse> s_deleteSubmittedReviewMethod = new(
        MethodType.Unary,
        "peerly.core.v1.SubmissionService",
        "V1DeleteSubmittedReview",
        CreateMarshaller(V1DeleteSubmittedReviewRequest.Parser),
        CreateMarshaller(V1DeleteSubmittedReviewResponse.Parser));

    private readonly GrpcChannel _channel;

    public DeleteSubmittedReviewGrpcClient(GrpcChannel channel)
    {
        _channel = channel;
    }

    public async Task<V1DeleteSubmittedReviewResponse> V1DeleteSubmittedReviewAsync(
        V1DeleteSubmittedReviewRequest request,
        CancellationToken cancellationToken = default)
    {
        var call = _channel.CreateCallInvoker()
            .AsyncUnaryCall(
                s_deleteSubmittedReviewMethod,
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
