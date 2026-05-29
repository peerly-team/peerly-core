using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using Peerly.Core.V1;

namespace Peerly.Core.IntegrationTests.Features.V1.Rubrics.GetTeacherRubric.Infrastructure;

public sealed class GetTeacherRubricGrpcClient
{
    private static readonly Method<V1GetTeacherRubricRequest, V1GetTeacherRubricResponse> s_getTeacherRubricMethod = new(
        MethodType.Unary,
        "peerly.core.v1.RubricService",
        "V1GetTeacherRubric",
        CreateMarshaller(V1GetTeacherRubricRequest.Parser),
        CreateMarshaller(V1GetTeacherRubricResponse.Parser));

    private readonly GrpcChannel _channel;

    public GetTeacherRubricGrpcClient(GrpcChannel channel)
    {
        _channel = channel;
    }

    public async Task<V1GetTeacherRubricResponse> V1GetTeacherRubricAsync(
        V1GetTeacherRubricRequest request,
        CancellationToken cancellationToken = default)
    {
        var call = _channel.CreateCallInvoker()
            .AsyncUnaryCall(
                s_getTeacherRubricMethod,
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
