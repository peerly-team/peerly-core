using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using Peerly.Core.V1;

namespace Peerly.Core.IntegrationTests.Features.V1.Homeworks.GetTeacherHomework.Infrastructure;

public sealed class GetTeacherHomeworkGrpcClient
{
    private static readonly Method<V1GetTeacherHomeworkRequest, V1GetTeacherHomeworkResponse> s_getTeacherHomeworkMethod = new(
        MethodType.Unary,
        "peerly.core.v1.HomeworkService",
        "V1GetTeacherHomework",
        CreateMarshaller(V1GetTeacherHomeworkRequest.Parser),
        CreateMarshaller(V1GetTeacherHomeworkResponse.Parser));

    private readonly GrpcChannel _channel;

    public GetTeacherHomeworkGrpcClient(GrpcChannel channel)
    {
        _channel = channel;
    }

    public async Task<V1GetTeacherHomeworkResponse> V1GetTeacherHomeworkAsync(
        V1GetTeacherHomeworkRequest request,
        CancellationToken cancellationToken = default)
    {
        var call = _channel.CreateCallInvoker()
            .AsyncUnaryCall(
                s_getTeacherHomeworkMethod,
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
