using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using Peerly.Core.V1;

namespace Peerly.Core.IntegrationTests.Features.V1.Submissions.GetAssignedReview.Infrastructure;

public sealed class GetAssignedReviewGrpcClient
{
    private static readonly Method<V1GetAssignedReviewRequest, V1GetAssignedReviewResponse> s_getAssignedReviewMethod = new(
        MethodType.Unary,
        "peerly.core.v1.SubmissionService",
        "V1GetAssignedReview",
        CreateMarshaller(V1GetAssignedReviewRequest.Parser),
        CreateMarshaller(V1GetAssignedReviewResponse.Parser));

    private readonly GrpcChannel _channel;

    public GetAssignedReviewGrpcClient(GrpcChannel channel)
    {
        _channel = channel;
    }

    public async Task<V1GetAssignedReviewResponse> V1GetAssignedReviewAsync(
        V1GetAssignedReviewRequest request,
        CancellationToken cancellationToken = default)
    {
        var call = _channel.CreateCallInvoker()
            .AsyncUnaryCall(
                s_getAssignedReviewMethod,
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
