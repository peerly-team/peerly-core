using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using Peerly.Core.V1;

namespace Peerly.Core.IntegrationTests.Features.V1.Submissions.GetSubmittedReview.Infrastructure;

public sealed class GetSubmittedReviewGrpcClient
{
    private static readonly Method<V1GetSubmittedReviewRequest, V1GetSubmittedReviewResponse> s_getSubmittedReviewMethod = new(
        MethodType.Unary,
        "peerly.core.v1.SubmissionService",
        "V1GetSubmittedReview",
        CreateMarshaller(V1GetSubmittedReviewRequest.Parser),
        CreateMarshaller(V1GetSubmittedReviewResponse.Parser));

    private readonly GrpcChannel _channel;

    public GetSubmittedReviewGrpcClient(GrpcChannel channel)
    {
        _channel = channel;
    }

    public async Task<V1GetSubmittedReviewResponse> V1GetSubmittedReviewAsync(
        V1GetSubmittedReviewRequest request,
        CancellationToken cancellationToken = default)
    {
        var call = _channel.CreateCallInvoker()
            .AsyncUnaryCall(
                s_getSubmittedReviewMethod,
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
