using Peerly.Core.Models.Shared;

namespace Peerly.Core.ApplicationServices.Features.Validations;

internal static class SubmittedReviewErrors
{
    public static ErrorMessage SubmittedReviewNotFound => "Отправленная рецензия не найдена";
    public static ErrorMessage ScoresMismatchCriteria => "Оценки не соответствуют критериям рубрики";
    public static ErrorMessage ScoreOutOfRange => "Оценка выходит за допустимый диапазон критерия";
    public static ErrorMessage CriterionCommentRequired => "Комментарий обязателен для данного критерия";
}
