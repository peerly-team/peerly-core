using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using Peerly.Core.V1;

namespace Peerly.Core.IntegrationTests.Features.V1.Users.GetTeacher.Infrastructure;

public sealed class GetTeacherGrpcClient
{
    private static readonly Method<V1GetTeacherRequest, V1GetTeacherResponse> s_getTeacherMethod = new(
        MethodType.Unary,
        "peerly.core.v1.UserService",
        "V1GetTeacher",
        CreateMarshaller(V1GetTeacherRequest.Parser),
        CreateMarshaller(V1GetTeacherResponse.Parser));

    private readonly GrpcChannel _channel;

    public GetTeacherGrpcClient(GrpcChannel channel)
    {
        _channel = channel;
    }

    public async Task<V1GetTeacherResponse> V1GetTeacherAsync(
        V1GetTeacherRequest request,
        CancellationToken cancellationToken = default)
    {
        var call = _channel.CreateCallInvoker()
            .AsyncUnaryCall(
                s_getTeacherMethod,
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
