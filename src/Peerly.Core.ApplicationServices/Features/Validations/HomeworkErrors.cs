using Peerly.Core.Models.Shared;

namespace Peerly.Core.ApplicationServices.Features.Validations;

internal static class HomeworkErrors
{
    public static ErrorMessage HomeworkNotFound => "Домашнее задание не найдено";
    public static ErrorMessage HomeworkNotAcceptingSubmissions => "Отправка ответов для домашнего задания закрыта";
    public static ErrorMessage HomeworkDeadlinePassed => "Срок сдачи домашнего задания истёк";
    public static ErrorMessage HomeworkNotInConfirmationStatus => "Домашнее задание не находится в статусе подтверждения";
    public static ErrorMessage IncorrectHomeworkStatusForDelete => "Удалить домашнее задание можно только в статусе \"Черновик\"";
}
