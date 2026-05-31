using Peerly.Core.Models.Shared;

namespace Peerly.Core.ApplicationServices.Features.Validations;

internal static class RubricErrors
{
    public static ErrorMessage RubricNotFound => "Рубрика не найдена";
    public static ErrorMessage RubricReferencedByHomework => "Нельзя удалить рубрику, которая используется в домашнем задании";
    public static ErrorMessage RubricReferencedByPublishedHomework => "Нельзя изменить рубрику, которая используется в опубликованном домашнем задании";
}
