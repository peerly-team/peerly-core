using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using Peerly.Core.V1;

namespace Peerly.Core.IntegrationTests.Features.V1.Rubrics.ListTeacherRubrics.Infrastructure;

public sealed class ListTeacherRubricsGrpcClient
{
    private static readonly Method<V1ListTeacherRubricsRequest, V1ListTeacherRubricsResponse> s_listTeacherRubricsMethod = new(
        MethodType.Unary,
        "peerly.core.v1.RubricService",
        "V1ListTeacherRubrics",
        CreateMarshaller(V1ListTeacherRubricsRequest.Parser),
        CreateMarshaller(V1ListTeacherRubricsResponse.Parser));

    private readonly GrpcChannel _channel;

    public ListTeacherRubricsGrpcClient(GrpcChannel channel)
    {
        _channel = channel;
    }

    public async Task<V1ListTeacherRubricsResponse> V1ListTeacherRubricsAsync(
        V1ListTeacherRubricsRequest request,
        CancellationToken cancellationToken = default)
    {
        var call = _channel.CreateCallInvoker()
            .AsyncUnaryCall(
                s_listTeacherRubricsMethod,
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
