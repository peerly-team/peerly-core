namespace Peerly.Core.Persistence.Schemas;

internal static class PeerlyCommonScheme
{
    public static class TeacherTable
    {
        public const string TableName = "teachers";

        public const string Id = "id";
        public const string Email = "email";
        public const string Name = "name";
        public const string CreationTime = "creation_time";
        public const string UpdateTime = "update_time";
    }

    public static class StudentTable
    {
        public const string TableName = "students";

        public const string Id = "id";
        public const string Email = "email";
        public const string Name = "name";
        public const string CreationTime = "creation_time";
        public const string UpdateTime = "update_time";
    }

    public static class CourseTable
    {
        public const string TableName = "courses";

        public const string Id = "id";
        public const string Name = "name";
        public const string Status = "status";
        public const string Description = "description";
        public const string CreationTime = "creation_time";
        public const string UpdateTime = "update_time";
    }

    public static class CourseFileTable
    {
        public const string TableName = "course_files";

        public const string CourseId = "course_id";
        public const string FileId = "file_id";
        public const string TeacherId = "teacher_id";
    }

    public static class CourseTeacherTable
    {
        public const string TableName = "course_teachers";

        public const string CourseId = "course_id";
        public const string TeacherId = "teacher_id";
        public const string CreationTime = "creation_time";
    }

    public static class GroupTable
    {
        public const string TableName = "groups";

        public const string Id = "id";
        public const string CourseId = "course_id";
        public const string Name = "name";
        public const string CreationTime = "creation_time";
        public const string UpdateTime = "update_time";
    }

    public static class GroupTeacherTable
    {
        public const string TableName = "group_teachers";

        public const string GroupId = "group_id";
        public const string TeacherId = "teacher_id";
        public const string CreationTime = "creation_time";
    }

    public static class GroupStudentTable
    {
        public const string TableName = "group_students";

        public const string GroupId = "group_id";
        public const string StudentId = "student_id";
        public const string CreationTime = "creation_time";
    }

    public static class HomeworkTable
    {
        public const string TableName = "homeworks";

        public const string Id = "id";
        public const string CourseId = "course_id";
        public const string GroupId = "group_id";
        public const string TeacherId = "teacher_id";
        public const string Name = "name";
        public const string Status = "status";
        public const string AmountOfReviewers = "amount_of_reviewers";
        public const string Description = "description";
        public const string Checklist = "checklist";
        public const string Deadline = "deadline";
        public const string ReviewDeadline = "review_deadline";
        public const string CreationTime = "creation_time";
        public const string UpdateTime = "update_time";
    }

    public static class HomeworkFileTable
    {
        public const string TableName = "homework_files";

        public const string HomeworkId = "homework_id";
        public const string FileId = "file_id";
        public const string TeacherId = "teacher_id";
    }

    public static class SubmittedHomeworkTable
    {
        public const string TableName = "submitted_homeworks";

        public const string Id = "id";
        public const string HomeworkId = "homework_id";
        public const string StudentId = "student_id";
        public const string Comment = "comment";
        public const string CreationTime = "creation_time";
        public const string UpdateTime = "update_time";
    }

    public static class SubmittedHomeworkFileTable
    {
        public const string TableName = "submitted_homework_files";

        public const string SubmittedHomeworkId = "submitted_homework_id";
        public const string FileId = "file_id";
    }

    public static class HomeworkDistributionTable
    {
        public const string TableName = "homework_distributions";

        public const string HomeworkId = "homework_id";
        public const string DistributionTime = "distribution_time";
        public const string CreationTime = "creation_time";
        public const string ProcessStatus = "process_status";
        public const string FailCount = "fail_count";
        public const string ProcessTime = "process_time";
        public const string TakenTime = "taken_time";
        public const string Error = "error";
    }

    public static class DistributionReviewerTable
    {
        public const string TableName = "distribution_reviewers";

        public const string SubmittedHomeworkId = "submitted_homework_id";
        public const string StudentId = "student_id";
    }

    public static class SubmittedReviewTable
    {
        public const string TableName = "submitted_reviews";

        public const string Id = "id";
        public const string SubmittedHomeworkId = "submitted_homework_id";
        public const string StudentId = "student_id";
        public const string Mark = "mark";
        public const string Comment = "comment";
        public const string CreationTime = "creation_time";
    }

    public static class SubmittedHomeworkMarkTable
    {
        public const string TableName = "submitted_homework_marks";

        public const string SubmittedHomeworkId = "submitted_homework_id";
        public const string ReviewersMark = "reviewers_mark";
        public const string TeacherMark = "teacher_mark";
        public const string TeacherId = "teacher_id";
        public const string CreationTime = "creation_time";
        public const string UpdateTime = "update_time";
    }

    public static class FileTable
    {
        public const string TableName = "files";

        public const string Id = "id";
        public const string StorageId = "storage_id";
        public const string Name = "name";
        public const string Size = "size";
        public const string CreationTime = "creation_time";
    }
}
