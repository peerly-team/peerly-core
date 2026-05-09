using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using Peerly.Core.V1;

namespace Peerly.Core.IntegrationTests.Features.V1.Submissions.CreateSubmittedHomework.Infrastructure;

public sealed class CreateSubmittedHomeworkGrpcClient
{
    private static readonly Method<V1CreateSubmittedHomeworkRequest, V1CreateSubmittedHomeworkResponse> s_createSubmittedHomeworkMethod = new(
        MethodType.Unary,
        "peerly.core.v1.SubmissionService",
        "V1CreateSubmittedHomework",
        CreateMarshaller(V1CreateSubmittedHomeworkRequest.Parser),
        CreateMarshaller(V1CreateSubmittedHomeworkResponse.Parser));

    private readonly GrpcChannel _channel;

    public CreateSubmittedHomeworkGrpcClient(GrpcChannel channel)
    {
        _channel = channel;
    }

    public async Task<V1CreateSubmittedHomeworkResponse> V1CreateSubmittedHomeworkAsync(
        V1CreateSubmittedHomeworkRequest request,
        CancellationToken cancellationToken = default)
    {
        var call = _channel.CreateCallInvoker()
            .AsyncUnaryCall(
                s_createSubmittedHomeworkMethod,
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
