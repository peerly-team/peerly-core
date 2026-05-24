using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using Peerly.Core.V1;

namespace Peerly.Core.IntegrationTests.Features.V1.Submissions.ListAssignedReviews.Infrastructure;

public sealed class ListAssignedReviewsGrpcClient
{
    private static readonly Method<V1ListAssignedReviewsRequest, V1ListAssignedReviewsResponse> s_listAssignedReviewsMethod = new(
        MethodType.Unary,
        "peerly.core.v1.SubmissionService",
        "V1ListAssignedReviews",
        CreateMarshaller(V1ListAssignedReviewsRequest.Parser),
        CreateMarshaller(V1ListAssignedReviewsResponse.Parser));

    private readonly GrpcChannel _channel;

    public ListAssignedReviewsGrpcClient(GrpcChannel channel)
    {
        _channel = channel;
    }

    public async Task<V1ListAssignedReviewsResponse> V1ListAssignedReviewsAsync(
        V1ListAssignedReviewsRequest request,
        CancellationToken cancellationToken = default)
    {
        var call = _channel.CreateCallInvoker()
            .AsyncUnaryCall(
                s_listAssignedReviewsMethod,
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
