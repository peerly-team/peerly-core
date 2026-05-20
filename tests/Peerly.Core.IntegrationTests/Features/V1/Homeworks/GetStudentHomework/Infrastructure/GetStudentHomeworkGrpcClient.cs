using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using Peerly.Core.V1;

namespace Peerly.Core.IntegrationTests.Features.V1.Homeworks.GetStudentHomework.Infrastructure;

public sealed class GetStudentHomeworkGrpcClient
{
    private static readonly Method<V1GetStudentHomeworkRequest, V1GetStudentHomeworkResponse> s_getStudentHomeworkMethod = new(
        MethodType.Unary,
        "peerly.core.v1.HomeworkService",
        "V1GetStudentHomework",
        CreateMarshaller(V1GetStudentHomeworkRequest.Parser),
        CreateMarshaller(V1GetStudentHomeworkResponse.Parser));

    private readonly GrpcChannel _channel;

    public GetStudentHomeworkGrpcClient(GrpcChannel channel)
    {
        _channel = channel;
    }

    public async Task<V1GetStudentHomeworkResponse> V1GetStudentHomeworkAsync(
        V1GetStudentHomeworkRequest request,
        CancellationToken cancellationToken = default)
    {
        var call = _channel.CreateCallInvoker()
            .AsyncUnaryCall(
                s_getStudentHomeworkMethod,
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
