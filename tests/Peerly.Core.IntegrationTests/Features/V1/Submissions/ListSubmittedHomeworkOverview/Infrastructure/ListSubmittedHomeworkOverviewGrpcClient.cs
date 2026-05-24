using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using Peerly.Core.V1;

namespace Peerly.Core.IntegrationTests.Features.V1.Submissions.ListSubmittedHomeworkOverview.Infrastructure;

public sealed class ListSubmittedHomeworkOverviewGrpcClient
{
    private static readonly Method<V1ListSubmittedHomeworkOverviewRequest, V1ListSubmittedHomeworkOverviewResponse> s_listSubmittedHomeworkOverviewMethod = new(
        MethodType.Unary,
        "peerly.core.v1.SubmissionService",
        "V1ListSubmittedHomeworkOverview",
        CreateMarshaller(V1ListSubmittedHomeworkOverviewRequest.Parser),
        CreateMarshaller(V1ListSubmittedHomeworkOverviewResponse.Parser));

    private readonly GrpcChannel _channel;

    public ListSubmittedHomeworkOverviewGrpcClient(GrpcChannel channel)
    {
        _channel = channel;
    }

    public async Task<V1ListSubmittedHomeworkOverviewResponse> V1ListSubmittedHomeworkOverviewAsync(
        V1ListSubmittedHomeworkOverviewRequest request,
        CancellationToken cancellationToken = default)
    {
        var call = _channel.CreateCallInvoker()
            .AsyncUnaryCall(
                s_listSubmittedHomeworkOverviewMethod,
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
