using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using Peerly.Core.V1;

namespace Peerly.Core.IntegrationTests.Features.V1.Submissions.GetTeacherSubmittedHomework.Infrastructure;

public sealed class GetTeacherSubmittedHomeworkGrpcClient
{
    private static readonly Method<V1GetTeacherSubmittedHomeworkRequest, V1GetTeacherSubmittedHomeworkResponse> s_getTeacherSubmittedHomeworkMethod = new(
        MethodType.Unary,
        "peerly.core.v1.SubmissionService",
        "V1GetTeacherSubmittedHomework",
        CreateMarshaller(V1GetTeacherSubmittedHomeworkRequest.Parser),
        CreateMarshaller(V1GetTeacherSubmittedHomeworkResponse.Parser));

    private readonly GrpcChannel _channel;

    public GetTeacherSubmittedHomeworkGrpcClient(GrpcChannel channel)
    {
        _channel = channel;
    }

    public async Task<V1GetTeacherSubmittedHomeworkResponse> V1GetTeacherSubmittedHomeworkAsync(
        V1GetTeacherSubmittedHomeworkRequest request,
        CancellationToken cancellationToken = default)
    {
        var call = _channel.CreateCallInvoker()
            .AsyncUnaryCall(
                s_getTeacherSubmittedHomeworkMethod,
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
