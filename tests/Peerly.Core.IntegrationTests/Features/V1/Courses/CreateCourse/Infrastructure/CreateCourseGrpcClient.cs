using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using Peerly.Core.V1;

namespace Peerly.Core.IntegrationTests.Features.V1.Courses.CreateCourse.Infrastructure;

public sealed class CreateCourseGrpcClient
{
    private static readonly Method<V1CreateCourseRequest, V1CreateCourseResponse> s_createCourseMethod = new(
        MethodType.Unary,
        "peerly.core.v1.CourseService",
        "V1CreateCourse",
        CreateMarshaller(V1CreateCourseRequest.Parser),
        CreateMarshaller(V1CreateCourseResponse.Parser));

    private readonly GrpcChannel _channel;

    public CreateCourseGrpcClient(GrpcChannel channel)
    {
        _channel = channel;
    }

    public async Task<V1CreateCourseResponse> V1CreateCourseAsync(
        V1CreateCourseRequest request,
        CancellationToken cancellationToken = default)
    {
        var call = _channel.CreateCallInvoker()
            .AsyncUnaryCall(
                s_createCourseMethod,
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
