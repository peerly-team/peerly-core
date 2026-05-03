using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using Peerly.Core.V1;

namespace Peerly.Core.IntegrationTests.Features.V1.Courses.SearchTeacherCourses.Infrastructure;

public sealed class SearchTeacherCoursesGrpcClient
{
    private static readonly Method<V1SearchTeacherCoursesRequest, V1SearchTeacherCoursesResponse> s_searchTeacherCoursesMethod = new(
        MethodType.Unary,
        "peerly.core.v1.CourseService",
        "V1SearchTeacherCourses",
        CreateMarshaller(V1SearchTeacherCoursesRequest.Parser),
        CreateMarshaller(V1SearchTeacherCoursesResponse.Parser));

    private readonly GrpcChannel _channel;

    public SearchTeacherCoursesGrpcClient(GrpcChannel channel)
    {
        _channel = channel;
    }

    public async Task<V1SearchTeacherCoursesResponse> V1SearchTeacherCoursesAsync(
        V1SearchTeacherCoursesRequest request,
        CancellationToken cancellationToken = default)
    {
        var call = _channel.CreateCallInvoker()
            .AsyncUnaryCall(
                s_searchTeacherCoursesMethod,
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
