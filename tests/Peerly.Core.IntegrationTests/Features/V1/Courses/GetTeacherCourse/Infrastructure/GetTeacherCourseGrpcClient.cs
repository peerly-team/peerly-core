using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using Peerly.Core.V1;

namespace Peerly.Core.IntegrationTests.Features.V1.Courses.GetTeacherCourse.Infrastructure;

public sealed class GetTeacherCourseGrpcClient
{
    private static readonly Method<V1GetTeacherCourseRequest, V1GetTeacherCourseResponse> s_getTeacherCourseMethod = new(
        MethodType.Unary,
        "peerly.core.v1.CourseService",
        "V1GetTeacherCourse",
        CreateMarshaller(V1GetTeacherCourseRequest.Parser),
        CreateMarshaller(V1GetTeacherCourseResponse.Parser));

    private readonly GrpcChannel _channel;

    public GetTeacherCourseGrpcClient(GrpcChannel channel)
    {
        _channel = channel;
    }

    public async Task<V1GetTeacherCourseResponse> V1GetTeacherCourseAsync(
        V1GetTeacherCourseRequest request,
        CancellationToken cancellationToken = default)
    {
        var call = _channel.CreateCallInvoker()
            .AsyncUnaryCall(
                s_getTeacherCourseMethod,
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
