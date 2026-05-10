using Peerly.Core.IntegrationTests.Features.V1.Submissions.DeleteSubmittedHomework.Infrastructure;
using Peerly.Core.IntegrationTests.Infrastructure;

namespace Peerly.Core.IntegrationTests.Features.V1.Submissions.DeleteSubmittedHomework;

public abstract class DeleteSubmittedHomeworkIntegrationTestBase : SubmissionIntegrationTestBase
{
    protected DeleteSubmittedHomeworkIntegrationTestBase(IntegrationTestFixture fixture)
        : base(fixture)
    {
    }

    protected DeleteSubmittedHomeworkGrpcClient DeleteSubmittedHomeworkClient => Fixture.DeleteSubmittedHomeworkClient;
}
