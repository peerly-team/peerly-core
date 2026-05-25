using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using Peerly.Core.V1;

namespace Peerly.Core.IntegrationTests.Features.V1.Users.GetStudent.Infrastructure;

public sealed class GetStudentGrpcClient
{
    private static readonly Method<V1GetStudentRequest, V1GetStudentResponse> s_getStudentMethod = new(
        MethodType.Unary,
        "peerly.core.v1.UserService",
        "V1GetStudent",
        CreateMarshaller(V1GetStudentRequest.Parser),
        CreateMarshaller(V1GetStudentResponse.Parser));

    private readonly GrpcChannel _channel;

    public GetStudentGrpcClient(GrpcChannel channel)
    {
        _channel = channel;
    }

    public async Task<V1GetStudentResponse> V1GetStudentAsync(
        V1GetStudentRequest request,
        CancellationToken cancellationToken = default)
    {
        var call = _channel.CreateCallInvoker()
            .AsyncUnaryCall(
                s_getStudentMethod,
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
