using Peerly.Core.ApplicationServices.Features.V1.Courses.CreateCourse;
using Peerly.Core.ApplicationServices.Features.V1.Courses.DeleteCourse;
using Peerly.Core.ApplicationServices.Features.V1.Courses.UpdateCourse;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.CreateHomework;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.UpdateHomeworkStatus;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.CreateHomeworkSubmission;
using Peerly.Core.V1;

namespace Peerly.Core.Api.Infrastructure.Configuration;

public static class ValidationPropertyMappingConfiguration
{
    /// <summary>
    /// Register validation property mapping.
    /// As we do validation on business types it means that we should map types in the following way:
    /// <![CDATA[ AddMapping<SourceType, DestinationType>() ]]>
    /// Where SourceType is our business model type
    /// and DestinationType is proto request type
    /// </summary>
    public static void Configure()
    {
        ValidationPropertyMapping
            .AddMapping<CreateCourseCommand, V1CreateCourseRequest>()
            .Build();

        ValidationPropertyMapping
            .AddMapping<DeleteCourseCommand, V1DeleteCourseRequest>()
            .Build();

        ValidationPropertyMapping
            .AddMapping<UpdateCourseCommand, V1UpdateCourseRequest>()
            .Build();

        ValidationPropertyMapping
            .AddMapping<CreateHomeworkCommand, V1CreateHomeworkRequest>()
            .Build();

        ValidationPropertyMapping
            .AddMapping<UpdateHomeworkStatusCommand, V1UpdateHomeworkStatusRequest>()
            .Build();

        ValidationPropertyMapping
            .AddMapping<CreateHomeworkSubmissionCommand, V1CreateHomeworkSubmissionRequest>()
            .Build();
    }
}
