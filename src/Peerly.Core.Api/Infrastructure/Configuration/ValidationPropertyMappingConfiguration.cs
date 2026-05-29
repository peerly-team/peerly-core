using Peerly.Core.ApplicationServices.Features.V1.Courses.CreateCourse;
using Peerly.Core.ApplicationServices.Features.V1.Rubrics.CreateRubric;
using Peerly.Core.ApplicationServices.Features.V1.Rubrics.DeleteRubric;
using Peerly.Core.ApplicationServices.Features.V1.Rubrics.GetStudentRubric;
using Peerly.Core.ApplicationServices.Features.V1.Rubrics.GetTeacherRubric;
using Peerly.Core.ApplicationServices.Features.V1.Rubrics.ListTeacherRubrics;
using Peerly.Core.ApplicationServices.Features.V1.Rubrics.UpdateRubric;
using Peerly.Core.ApplicationServices.Features.V1.Courses.CreateCourseFile;
using Peerly.Core.ApplicationServices.Features.V1.Courses.DeleteCourse;
using Peerly.Core.ApplicationServices.Features.V1.Courses.PublishCourse;
using Peerly.Core.ApplicationServices.Features.V1.Courses.UpdateCourse;
using Peerly.Core.ApplicationServices.Features.V1.Groups.CreateGroup;
using Peerly.Core.ApplicationServices.Features.V1.Groups.DeleteGroup;
using Peerly.Core.ApplicationServices.Features.V1.Groups.UpdateGroup;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.ConfirmHomework;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.CreateCourseHomework;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.CreateGroupHomework;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.CreateHomeworkFile;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.DeleteHomework;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.DeleteHomeworkFile;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.PostponeHomeworkDeadlines;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.PublishHomework;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.UpdateDraftHomework;
using Peerly.Core.ApplicationServices.Features.V1.Participants.AddGroupStudent;
using Peerly.Core.ApplicationServices.Features.V1.Participants.AddGroupTeacher;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.CorrectSubmittedHomeworkMark;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.CreateSubmittedHomework;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.CreateSubmittedHomeworkFile;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.CreateSubmittedReview;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.DeleteSubmittedHomework;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.DeleteSubmittedHomeworkFile;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.DeleteSubmittedReview;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.GetAssignedReview;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.GetSubmittedHomework;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.GetSubmittedReview;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.GetTeacherSubmittedHomework;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.ListAssignedReviews;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.ListSubmittedHomeworkOverview;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.UpdateSubmittedHomework;
using Peerly.Core.ApplicationServices.Features.V1.Students.UpdateStudent;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.UpdateSubmittedReview;
using Peerly.Core.ApplicationServices.Features.V1.Teachers.UpdateTeacher;
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
            .AddMapping<CreateCourseFileCommand, V1CreateCourseFileRequest>()
            .Build();

        ValidationPropertyMapping
            .AddMapping<DeleteCourseCommand, V1DeleteCourseRequest>()
            .Build();

        ValidationPropertyMapping
            .AddMapping<UpdateCourseCommand, V1UpdateCourseRequest>()
            .Build();

        ValidationPropertyMapping
            .AddMapping<PublishCourseCommand, V1PublishCourseRequest>()
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
            .AddMapping<UpdateGroupCommand, V1UpdateGroupRequest>()
            .Build();

        ValidationPropertyMapping
            .AddMapping<DeleteGroupCommand, V1DeleteGroupRequest>()
            .Build();

        ValidationPropertyMapping
            .AddMapping<AddGroupStudentCommand, V1AddGroupStudentRequest>()
            .Build();

        ValidationPropertyMapping
            .AddMapping<AddGroupTeacherCommand, V1AddGroupTeacherRequest>()
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

        ValidationPropertyMapping
            .AddMapping<DeleteHomeworkCommand, V1DeleteHomeworkRequest>()
            .Build();

        ValidationPropertyMapping
            .AddMapping<DeleteHomeworkFileCommand, V1DeleteHomeworkFileRequest>()
            .Build();

        ValidationPropertyMapping
            .AddMapping<UpdateSubmittedHomeworkCommand, V1UpdateSubmittedHomeworkRequest>()
            .Build();

        ValidationPropertyMapping
            .AddMapping<DeleteSubmittedHomeworkCommand, V1DeleteSubmittedHomeworkRequest>()
            .Build();

        ValidationPropertyMapping
            .AddMapping<DeleteSubmittedHomeworkFileCommand, V1DeleteSubmittedHomeworkFileRequest>()
            .Build();

        ValidationPropertyMapping
            .AddMapping<GetSubmittedHomeworkQuery, V1GetSubmittedHomeworkRequest>()
            .Build();

        ValidationPropertyMapping
            .AddMapping<ListAssignedReviewsQuery, V1ListAssignedReviewsRequest>()
            .Build();

        ValidationPropertyMapping
            .AddMapping<ListSubmittedHomeworkOverviewQuery, V1ListSubmittedHomeworkOverviewRequest>()
            .Build();

        ValidationPropertyMapping
            .AddMapping<GetAssignedReviewQuery, V1GetAssignedReviewRequest>()
            .Build();

        ValidationPropertyMapping
            .AddMapping<UpdateSubmittedReviewCommand, V1UpdateSubmittedReviewRequest>()
            .Build();

        ValidationPropertyMapping
            .AddMapping<DeleteSubmittedReviewCommand, V1DeleteSubmittedReviewRequest>()
            .Build();

        ValidationPropertyMapping
            .AddMapping<GetSubmittedReviewQuery, V1GetSubmittedReviewRequest>()
            .Build();

        ValidationPropertyMapping
            .AddMapping<GetTeacherSubmittedHomeworkQuery, V1GetTeacherSubmittedHomeworkRequest>()
            .Build();

        ValidationPropertyMapping
            .AddMapping<CorrectSubmittedHomeworkMarkCommand, V1CorrectSubmittedHomeworkMarkRequest>()
            .Build();

        ValidationPropertyMapping
            .AddMapping<UpdateStudentCommand, V1UpdateStudentRequest>()
            .Build();

        ValidationPropertyMapping
            .AddMapping<UpdateTeacherCommand, V1UpdateTeacherRequest>()
            .Build();

        ValidationPropertyMapping
            .AddMapping<CreateRubricCommand, V1CreateRubricRequest>()
            .Build();

        ValidationPropertyMapping
            .AddMapping<UpdateRubricCommand, V1UpdateRubricRequest>()
            .Build();

        ValidationPropertyMapping
            .AddMapping<DeleteRubricCommand, V1DeleteRubricRequest>()
            .Build();

        ValidationPropertyMapping
            .AddMapping<GetTeacherRubricQuery, V1GetTeacherRubricRequest>()
            .Build();

        ValidationPropertyMapping
            .AddMapping<GetStudentRubricQuery, V1GetStudentRubricRequest>()
            .Build();

        ValidationPropertyMapping
            .AddMapping<ListTeacherRubricsQuery, V1ListTeacherRubricsRequest>()
            .Build();
    }
}
