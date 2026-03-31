using System.Collections.Generic;
using Peerly.Core.ApplicationServices.BackgroundServices.HomeworkDistribution.Abstractions;
using Peerly.Core.Exceptions;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Homeworks;

namespace Peerly.Core.ApplicationServices.BackgroundServices.HomeworkDistribution;

internal sealed class HomeworkDistributionJobValidator : IHomeworkDistributionJobValidator
{
    public void Validate(IReadOnlyCollection<SubmittedHomeworkStudent> submittedHomeworks, int amountOfReviewers)
    {
        if (amountOfReviewers < 0)
        {
            throw new BusinessValidationException("Amount of reviewers cannot be negative.");
        }

        if (submittedHomeworks.Count <= 1 || amountOfReviewers == 0)
        {
            return;
        }

        EnsureUniqueStudents(submittedHomeworks);
    }

    private static void EnsureUniqueStudents(IReadOnlyCollection<SubmittedHomeworkStudent> submittedHomeworks)
    {
        var studentIds = new HashSet<StudentId>();

        foreach (var submittedHomework in submittedHomeworks)
        {
            if (!studentIds.Add(submittedHomework.StudentId))
            {
                throw new BusinessValidationException(
                    $"Student {submittedHomework.StudentId} has multiple submitted homeworks in a single distribution.");
            }
        }
    }
}
