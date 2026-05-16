using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using Peerly.Core.V1;

namespace Peerly.Core.IntegrationTests.Features.V1.Homeworks.SearchTeacherHomeworks.Infrastructure;

public sealed class SearchTeacherHomeworksGrpcClient
{
    private static readonly Method<V1SearchTeacherHomeworksRequest, V1SearchTeacherHomeworksResponse> s_searchTeacherHomeworksMethod = new(
        MethodType.Unary,
        "peerly.core.v1.HomeworkService",
        "V1SearchTeacherHomeworks",
        CreateMarshaller(V1SearchTeacherHomeworksRequest.Parser),
        CreateMarshaller(V1SearchTeacherHomeworksResponse.Parser));

    private readonly GrpcChannel _channel;

    public SearchTeacherHomeworksGrpcClient(GrpcChannel channel)
    {
        _channel = channel;
    }

    public async Task<V1SearchTeacherHomeworksResponse> V1SearchTeacherHomeworksAsync(
        V1SearchTeacherHomeworksRequest request,
        CancellationToken cancellationToken = default)
    {
        var call = _channel.CreateCallInvoker()
            .AsyncUnaryCall(
                s_searchTeacherHomeworksMethod,
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
