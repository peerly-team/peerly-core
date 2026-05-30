using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using Peerly.Core.V1;

namespace Peerly.Core.IntegrationTests.Features.V1.Participants.BulkAddGroupStudents.Infrastructure;

public sealed class BulkAddGroupStudentsGrpcClient
{
    private static readonly Method<V1BulkAddGroupStudentsRequest, V1BulkAddGroupStudentsResponse> s_bulkAddGroupStudentsMethod = new(
        MethodType.Unary,
        "peerly.core.v1.ParticipantService",
        "V1BulkAddGroupStudents",
        CreateMarshaller(V1BulkAddGroupStudentsRequest.Parser),
        CreateMarshaller(V1BulkAddGroupStudentsResponse.Parser));

    private readonly GrpcChannel _channel;

    public BulkAddGroupStudentsGrpcClient(GrpcChannel channel)
    {
        _channel = channel;
    }

    public async Task<V1BulkAddGroupStudentsResponse> V1BulkAddGroupStudentsAsync(
        V1BulkAddGroupStudentsRequest request,
        CancellationToken cancellationToken = default)
    {
        var call = _channel.CreateCallInvoker()
            .AsyncUnaryCall(
                s_bulkAddGroupStudentsMethod,
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
