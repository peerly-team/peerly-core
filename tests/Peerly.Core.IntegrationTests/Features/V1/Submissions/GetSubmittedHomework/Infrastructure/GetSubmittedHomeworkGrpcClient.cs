using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using Peerly.Core.V1;

namespace Peerly.Core.IntegrationTests.Features.V1.Submissions.GetSubmittedHomework.Infrastructure;

public sealed class GetSubmittedHomeworkGrpcClient
{
    private static readonly Method<V1GetSubmittedHomeworkRequest, V1GetSubmittedHomeworkResponse> s_getSubmittedHomeworkMethod = new(
        MethodType.Unary,
        "peerly.core.v1.SubmissionService",
        "V1GetSubmittedHomework",
        CreateMarshaller(V1GetSubmittedHomeworkRequest.Parser),
        CreateMarshaller(V1GetSubmittedHomeworkResponse.Parser));

    private readonly GrpcChannel _channel;

    public GetSubmittedHomeworkGrpcClient(GrpcChannel channel)
    {
        _channel = channel;
    }

    public async Task<V1GetSubmittedHomeworkResponse> V1GetSubmittedHomeworkAsync(
        V1GetSubmittedHomeworkRequest request,
        CancellationToken cancellationToken = default)
    {
        var call = _channel.CreateCallInvoker()
            .AsyncUnaryCall(
                s_getSubmittedHomeworkMethod,
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
