using Peerly.Core.Models.Shared;

namespace Peerly.Core.ApplicationServices.Features.Validations;

internal static class CourseErrors
{
    public static ErrorMessage CourseNotFound => "Курс не найден";
    public static ErrorMessage IncorrectCourseStatusForDelete => "Удалить курс можно только в статусе \"Черновик\"";
    public static ErrorMessage IncorrectCourseStatusForUpdate => "Изменить информацию о курсе можно в статусах  \"Черновик\" и \"В процессе\"";
    public static ErrorMessage ForbiddenUpdateCourseStatusToDelete => "Перевести статус курса на \"Удален\" невозможно через обновление - используйте ручку удаления";
}
