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
        public const string Description = "description";
        public const string Status = "status";
        public const string CreationTime = "creation_time";
        public const string UpdateTime = "update_time";
    }

    public static class GroupTable
    {
        public const string TableName = "groups";

        public const string Id = "id";
        public const string Name = "name";
        public const string CourseId = "course_id";
        public const string CreationTime = "creation_time";
        public const string UpdateTime = "update_time";
    }

    public static class CourseTeacherTable
    {
        public const string TableName = "course_teachers";

        public const string Id = "id";
        public const string CourseId = "course_id";
        public const string TeacherId = "teacher_id";
        public const string CreationTime = "creation_time";
    }

    public static class GroupTeacherTable
    {
        public const string TableName = "group_teachers";

        public const string Id = "id";
        public const string GroupId = "group_id";
        public const string TeacherId = "teacher_id";
        public const string CreationTime = "creation_time";
    }

    public static class GroupStudentTable
    {
        public const string TableName = "group_students";

        public const string Id = "id";
        public const string GroupId = "group_id";
        public const string StudentId = "student_id";
        public const string CreationTime = "creation_time";
    }

    public static class HomeworkTable
    {
        public const string TableName = "homeworks";

        public const string Id = "id";
        public const string CourseId = "course_id";
        public const string TeacherId = "teacher_id";
        public const string Name = "name";
        public const string Description = "description";
        public const string Checklist = "checklist";
        public const string Status = "status";
        public const string Deadline = "deadline";
        public const string ReviewDeadline = "review_deadline";
        public const string CreationTime = "creation_time";
        public const string UpdateTime = "update_time";
    }

    public static class GroupHomeworkTable
    {
        public const string TableName = "group_homeworks";

        public const string Id = "id";
        public const string GroupId = "group_id";
        public const string Name = "name";
        public const string Description = "description";
        public const string Checklist = "checklist";
        public const string Status = "status";
        public const string Deadline = "deadline";
        public const string ReviewDeadline = "review_deadline";
        public const string CreationTime = "creation_time";
        public const string UpdateTime = "update_time";
    }

    public static class StudentHomeworkTable
    {
        public const string TableName = "student_homeworks";

        public const string Id = "id";
        public const string StudentId = "student_id";
        public const string GroupHomeworkId = "group_homework_id";
        public const string Date = "date";
        public const string Status = "status";
        public const string Mark = "mark";
        public const string CreationTime = "creation_time";
        public const string UpdateTime = "update_time";
    }

    public static class StudentHomeworkMarkTable
    {
        public const string TableName = "student_homework_marks";

        public const string Id = "id";
        public const string StudentId = "student_id";
        public const string StudentHomeworkId = "student_homework_id";
        public const string Mark = "mark";
        public const string CreationTime = "creation_time";
        public const string UpdateTime = "update_time";
    }

    public static class HomeworkFileTable
    {
        public const string TableName = "homework_files";

        public const string Id = "id";
        public const string HomeworkId = "homework_id";
        public const string FileId = "file_id";
        public const string CreationTime = "creation_time";
    }

    public static class StudentHomeworkFileTable
    {
        public const string TableName = "student_homework_files";

        public const string Id = "id";
        public const string StudentHomeworkId = "student_homework_id";
        public const string FileId = "file_id";
        public const string CreationTime = "creation_time";
    }

    public static class FileTable
    {
        public const string TableName = "files";

        public const string Id = "id";
        public const string Name = "name";
        public const string Extension = "extension";
        public const string CreationTime = "creation_time";
    }

    public static class TeacherHomeworkApprovalTable
    {
        public const string TableName = "teacher_homework_approvals";

        public const string Id = "id";
        public const string TeacherId = "teacher_id";
        public const string GroupHomeworkId = "group_homework_id";
        public const string CreationTime = "creation_time";
    }
}
