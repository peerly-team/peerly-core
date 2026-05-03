using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using Peerly.Core.V1;

namespace Peerly.Core.IntegrationTests.Features.V1.Courses.PublishCourse.Infrastructure;

public sealed class PublishCourseGrpcClient
{
    private static readonly Method<V1PublishCourseRequest, V1PublishCourseResponse> s_publishCourseMethod = new(
        MethodType.Unary,
        "peerly.core.v1.CourseService",
        "V1PublishCourse",
        CreateMarshaller(V1PublishCourseRequest.Parser),
        CreateMarshaller(V1PublishCourseResponse.Parser));

    private readonly GrpcChannel _channel;

    public PublishCourseGrpcClient(GrpcChannel channel)
    {
        _channel = channel;
    }

    public async Task<V1PublishCourseResponse> V1PublishCourseAsync(
        V1PublishCourseRequest request,
        CancellationToken cancellationToken = default)
    {
        var call = _channel.CreateCallInvoker()
            .AsyncUnaryCall(
                s_publishCourseMethod,
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
