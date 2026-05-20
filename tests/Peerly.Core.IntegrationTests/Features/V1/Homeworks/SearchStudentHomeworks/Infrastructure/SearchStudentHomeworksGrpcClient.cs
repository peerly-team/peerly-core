using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using Peerly.Core.V1;

namespace Peerly.Core.IntegrationTests.Features.V1.Homeworks.SearchStudentHomeworks.Infrastructure;

public sealed class SearchStudentHomeworksGrpcClient
{
    private static readonly Method<V1SearchStudentHomeworksRequest, V1SearchStudentHomeworksResponse> s_searchStudentHomeworksMethod = new(
        MethodType.Unary,
        "peerly.core.v1.HomeworkService",
        "V1SearchStudentHomeworks",
        CreateMarshaller(V1SearchStudentHomeworksRequest.Parser),
        CreateMarshaller(V1SearchStudentHomeworksResponse.Parser));

    private readonly GrpcChannel _channel;

    public SearchStudentHomeworksGrpcClient(GrpcChannel channel)
    {
        _channel = channel;
    }

    public async Task<V1SearchStudentHomeworksResponse> V1SearchStudentHomeworksAsync(
        V1SearchStudentHomeworksRequest request,
        CancellationToken cancellationToken = default)
    {
        var call = _channel.CreateCallInvoker()
            .AsyncUnaryCall(
                s_searchStudentHomeworksMethod,
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
