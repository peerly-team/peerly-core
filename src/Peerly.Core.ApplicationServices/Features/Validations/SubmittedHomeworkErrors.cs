using Peerly.Core.Models.Shared;

namespace Peerly.Core.ApplicationServices.Features.Validations;

internal static class SubmittedHomeworkErrors
{
    public static ErrorMessage SubmittedHomeworkNotFound => "Отправленный ответ к домашнему заданию не найден";
    public static ErrorMessage SubmittedHomeworkFileNotFound => "Файл отправленного ответа к домашнему заданию не найден";
    public static ErrorMessage SubmittedHomeworkAlreadySubmitted => "Домашнее задание уже было отправлено";
}
