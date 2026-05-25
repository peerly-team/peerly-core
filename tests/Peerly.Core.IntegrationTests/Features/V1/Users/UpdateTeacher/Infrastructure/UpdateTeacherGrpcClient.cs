using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using Peerly.Core.V1;

namespace Peerly.Core.IntegrationTests.Features.V1.Users.UpdateTeacher.Infrastructure;

public sealed class UpdateTeacherGrpcClient
{
    private static readonly Method<V1UpdateTeacherRequest, V1UpdateTeacherResponse> s_updateTeacherMethod = new(
        MethodType.Unary,
        "peerly.core.v1.UserService",
        "V1UpdateTeacher",
        CreateMarshaller(V1UpdateTeacherRequest.Parser),
        CreateMarshaller(V1UpdateTeacherResponse.Parser));

    private readonly GrpcChannel _channel;

    public UpdateTeacherGrpcClient(GrpcChannel channel)
    {
        _channel = channel;
    }

    public async Task<V1UpdateTeacherResponse> V1UpdateTeacherAsync(
        V1UpdateTeacherRequest request,
        CancellationToken cancellationToken = default)
    {
        var call = _channel.CreateCallInvoker()
            .AsyncUnaryCall(
                s_updateTeacherMethod,
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
