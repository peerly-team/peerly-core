using Peerly.Core.ApplicationServices.Features.V1.Courses.CreateCourse;
using Peerly.Core.ApplicationServices.Features.V1.Courses.DeleteCourse;
using Peerly.Core.ApplicationServices.Features.V1.Courses.UpdateCourse;
using Peerly.Core.ApplicationServices.Features.V1.Groups.AddGroupStudent;
using Peerly.Core.ApplicationServices.Features.V1.Groups.CreateGroup;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.ConfirmHomework;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.CreateCourseHomework;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.CreateGroupHomework;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.CreateHomeworkFile;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.PostponeHomeworkDeadlines;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.PublishHomework;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.UpdateDraftHomework;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.CreateSubmittedHomework;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.CreateSubmittedHomeworkFile;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.CreateSubmittedReview;
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
            .AddMapping<CreateCourseHomeworkCommand, V1CreateCourseHomeworkRequest>()
            .Build();

        ValidationPropertyMapping
            .AddMapping<CreateGroupHomeworkCommand, V1CreateGroupHomeworkRequest>()
            .Build();

        ValidationPropertyMapping
            .AddMapping<CreateSubmittedHomeworkCommand, V1CreateSubmittedHomeworkRequest>()
            .Build();

        ValidationPropertyMapping
            .AddMapping<CreateHomeworkFileCommand, V1CreateHomeworkFileRequest>()
            .Build();

        ValidationPropertyMapping
            .AddMapping<CreateSubmittedHomeworkFileCommand, V1CreateSubmittedHomeworkFileRequest>()
            .Build();

        ValidationPropertyMapping
            .AddMapping<CreateGroupCommand, V1CreateGroupRequest>()
            .Build();

        ValidationPropertyMapping
            .AddMapping<AddGroupStudentCommand, V1AddGroupStudentRequest>()
            .Build();

        ValidationPropertyMapping
            .AddMapping<CreateSubmittedReviewCommand, V1CreateSubmittedReviewRequest>()
            .Build();

        ValidationPropertyMapping
            .AddMapping<PublishHomeworkCommand, V1PublishHomeworkRequest>()
            .Build();

        ValidationPropertyMapping
            .AddMapping<ConfirmHomeworkCommand, V1ConfirmHomeworkRequest>()
            .Build();

        ValidationPropertyMapping
            .AddMapping<UpdateDraftHomeworkCommand, V1UpdateDraftHomeworkRequest>()
            .Build();

        ValidationPropertyMapping
            .AddMapping<PostponeHomeworkDeadlinesCommand, V1PostponeHomeworkDeadlinesRequest>()
            .Build();
    }
}
