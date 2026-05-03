using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using Peerly.Core.V1;

namespace Peerly.Core.IntegrationTests.Features.V1.Courses.CreateCourseFile.Infrastructure;

public sealed class CreateCourseFileGrpcClient
{
    private static readonly Method<V1CreateCourseFileRequest, V1CreateCourseFileResponse> s_createCourseFileMethod = new(
        MethodType.Unary,
        "peerly.core.v1.CourseService",
        "V1CreateCourseFile",
        CreateMarshaller(V1CreateCourseFileRequest.Parser),
        CreateMarshaller(V1CreateCourseFileResponse.Parser));

    private readonly GrpcChannel _channel;

    public CreateCourseFileGrpcClient(GrpcChannel channel)
    {
        _channel = channel;
    }

    public async Task<V1CreateCourseFileResponse> V1CreateCourseFileAsync(
        V1CreateCourseFileRequest request,
        CancellationToken cancellationToken = default)
    {
        var call = _channel.CreateCallInvoker()
            .AsyncUnaryCall(
                s_createCourseFileMethod,
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
