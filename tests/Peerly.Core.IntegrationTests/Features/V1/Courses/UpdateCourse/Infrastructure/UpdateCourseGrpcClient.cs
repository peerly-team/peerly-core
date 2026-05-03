using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using Peerly.Core.V1;

namespace Peerly.Core.IntegrationTests.Features.V1.Courses.UpdateCourse.Infrastructure;

public sealed class UpdateCourseGrpcClient
{
    private static readonly Method<V1UpdateCourseRequest, V1UpdateCourseResponse> s_updateCourseMethod = new(
        MethodType.Unary,
        "peerly.core.v1.CourseService",
        "V1UpdateCourse",
        CreateMarshaller(V1UpdateCourseRequest.Parser),
        CreateMarshaller(V1UpdateCourseResponse.Parser));

    private readonly GrpcChannel _channel;

    public UpdateCourseGrpcClient(GrpcChannel channel)
    {
        _channel = channel;
    }

    public async Task<V1UpdateCourseResponse> V1UpdateCourseAsync(
        V1UpdateCourseRequest request,
        CancellationToken cancellationToken = default)
    {
        var call = _channel.CreateCallInvoker()
            .AsyncUnaryCall(
                s_updateCourseMethod,
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
