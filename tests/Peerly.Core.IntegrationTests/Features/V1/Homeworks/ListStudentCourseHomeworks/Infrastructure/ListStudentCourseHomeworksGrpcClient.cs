using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using Peerly.Core.V1;

namespace Peerly.Core.IntegrationTests.Features.V1.Homeworks.ListStudentCourseHomeworks.Infrastructure;

public sealed class ListStudentCourseHomeworksGrpcClient
{
    private static readonly Method<V1ListStudentCourseHomeworksRequest, V1ListStudentCourseHomeworksResponse> s_listStudentCourseHomeworksMethod = new(
        MethodType.Unary,
        "peerly.core.v1.HomeworkService",
        "V1ListStudentCourseHomeworks",
        CreateMarshaller(V1ListStudentCourseHomeworksRequest.Parser),
        CreateMarshaller(V1ListStudentCourseHomeworksResponse.Parser));

    private readonly GrpcChannel _channel;

    public ListStudentCourseHomeworksGrpcClient(GrpcChannel channel)
    {
        _channel = channel;
    }

    public async Task<V1ListStudentCourseHomeworksResponse> V1ListStudentCourseHomeworksAsync(
        V1ListStudentCourseHomeworksRequest request,
        CancellationToken cancellationToken = default)
    {
        var call = _channel.CreateCallInvoker()
            .AsyncUnaryCall(
                s_listStudentCourseHomeworksMethod,
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
