using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using Peerly.Core.V1;

namespace Peerly.Core.IntegrationTests.Features.V1.Users.UpdateStudent.Infrastructure;

public sealed class UpdateStudentGrpcClient
{
    private static readonly Method<V1UpdateStudentRequest, V1UpdateStudentResponse> s_updateStudentMethod = new(
        MethodType.Unary,
        "peerly.core.v1.UserService",
        "V1UpdateStudent",
        CreateMarshaller(V1UpdateStudentRequest.Parser),
        CreateMarshaller(V1UpdateStudentResponse.Parser));

    private readonly GrpcChannel _channel;

    public UpdateStudentGrpcClient(GrpcChannel channel)
    {
        _channel = channel;
    }

    public async Task<V1UpdateStudentResponse> V1UpdateStudentAsync(
        V1UpdateStudentRequest request,
        CancellationToken cancellationToken = default)
    {
        var call = _channel.CreateCallInvoker()
            .AsyncUnaryCall(
                s_updateStudentMethod,
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
