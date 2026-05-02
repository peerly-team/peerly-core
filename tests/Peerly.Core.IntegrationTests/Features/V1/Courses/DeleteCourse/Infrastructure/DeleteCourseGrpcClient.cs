using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using Peerly.Core.V1;

namespace Peerly.Core.IntegrationTests.Features.V1.Courses.DeleteCourse.Infrastructure;

public sealed class DeleteCourseGrpcClient
{
    private static readonly Method<V1DeleteCourseRequest, V1DeleteCourseResponse> s_deleteCourseMethod = new(
        MethodType.Unary,
        "peerly.core.v1.CourseService",
        "V1DeleteCourse",
        CreateMarshaller(V1DeleteCourseRequest.Parser),
        CreateMarshaller(V1DeleteCourseResponse.Parser));

    private readonly GrpcChannel _channel;

    public DeleteCourseGrpcClient(GrpcChannel channel)
    {
        _channel = channel;
    }

    public async Task<V1DeleteCourseResponse> V1DeleteCourseAsync(
        V1DeleteCourseRequest request,
        CancellationToken cancellationToken = default)
    {
        var call = _channel.CreateCallInvoker()
            .AsyncUnaryCall(
                s_deleteCourseMethod,
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
