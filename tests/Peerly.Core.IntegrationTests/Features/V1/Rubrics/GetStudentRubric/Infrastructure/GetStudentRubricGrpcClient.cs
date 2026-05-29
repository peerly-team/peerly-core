using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using Peerly.Core.V1;

namespace Peerly.Core.IntegrationTests.Features.V1.Rubrics.GetStudentRubric.Infrastructure;

public sealed class GetStudentRubricGrpcClient
{
    private static readonly Method<V1GetStudentRubricRequest, V1GetStudentRubricResponse> s_getStudentRubricMethod = new(
        MethodType.Unary,
        "peerly.core.v1.RubricService",
        "V1GetStudentRubric",
        CreateMarshaller(V1GetStudentRubricRequest.Parser),
        CreateMarshaller(V1GetStudentRubricResponse.Parser));

    private readonly GrpcChannel _channel;

    public GetStudentRubricGrpcClient(GrpcChannel channel)
    {
        _channel = channel;
    }

    public async Task<V1GetStudentRubricResponse> V1GetStudentRubricAsync(
        V1GetStudentRubricRequest request,
        CancellationToken cancellationToken = default)
    {
        var call = _channel.CreateCallInvoker()
            .AsyncUnaryCall(
                s_getStudentRubricMethod,
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
