using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using Peerly.Core.V1;

namespace Peerly.Core.IntegrationTests.Features.V1.Homeworks.ListTeacherCourseHomeworks.Infrastructure;

public sealed class ListTeacherCourseHomeworksGrpcClient
{
    private static readonly Method<V1ListTeacherCourseHomeworksRequest, V1ListTeacherCourseHomeworksResponse> s_listTeacherCourseHomeworksMethod = new(
        MethodType.Unary,
        "peerly.core.v1.HomeworkService",
        "V1ListTeacherCourseHomeworks",
        CreateMarshaller(V1ListTeacherCourseHomeworksRequest.Parser),
        CreateMarshaller(V1ListTeacherCourseHomeworksResponse.Parser));

    private readonly GrpcChannel _channel;

    public ListTeacherCourseHomeworksGrpcClient(GrpcChannel channel)
    {
        _channel = channel;
    }

    public async Task<V1ListTeacherCourseHomeworksResponse> V1ListTeacherCourseHomeworksAsync(
        V1ListTeacherCourseHomeworksRequest request,
        CancellationToken cancellationToken = default)
    {
        var call = _channel.CreateCallInvoker()
            .AsyncUnaryCall(
                s_listTeacherCourseHomeworksMethod,
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
