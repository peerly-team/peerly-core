using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using Peerly.Core.V1;

namespace Peerly.Core.IntegrationTests.Features.V1.Courses.GetStudentCourse.Infrastructure;

public sealed class GetStudentCourseGrpcClient
{
    private static readonly Method<V1GetStudentCourseRequest, V1GetStudentCourseResponse> s_getStudentCourseMethod = new(
        MethodType.Unary,
        "peerly.core.v1.CourseService",
        "V1GetStudentCourse",
        CreateMarshaller(V1GetStudentCourseRequest.Parser),
        CreateMarshaller(V1GetStudentCourseResponse.Parser));

    private readonly GrpcChannel _channel;

    public GetStudentCourseGrpcClient(GrpcChannel channel)
    {
        _channel = channel;
    }

    public async Task<V1GetStudentCourseResponse> V1GetStudentCourseAsync(
        V1GetStudentCourseRequest request,
        CancellationToken cancellationToken = default)
    {
        var call = _channel.CreateCallInvoker()
            .AsyncUnaryCall(
                s_getStudentCourseMethod,
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
