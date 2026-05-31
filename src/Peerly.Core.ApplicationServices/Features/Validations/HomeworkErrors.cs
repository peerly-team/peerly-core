using Peerly.Core.Models.Shared;

namespace Peerly.Core.ApplicationServices.Features.Validations;

internal static class HomeworkErrors
{
    public static ErrorMessage HomeworkNotFound => "Домашнее задание не найдено";
    public static ErrorMessage HomeworkNotAcceptingSubmissions => "Отправка ответов для домашнего задания закрыта";
    public static ErrorMessage HomeworkNotAcceptingReviews => "Проверка домашнего задания закрыта";
    public static ErrorMessage HomeworkNotInConfirmationStatus => "Домашнее задание не находится в статусе подтверждения";
    public static ErrorMessage IncorrectHomeworkStatusForDelete => "Удалить домашнее задание можно только в статусе \"Черновик\"";
    public static ErrorMessage IncorrectHomeworkStatusForDeleteFile => "Открепить файл можно когда домашнее задание в статусе \"Черновик\"";
    public static ErrorMessage HomeworkRubricNotFound => "У домашнего задания отсутствует рубрика с критериями оценивания";
}
