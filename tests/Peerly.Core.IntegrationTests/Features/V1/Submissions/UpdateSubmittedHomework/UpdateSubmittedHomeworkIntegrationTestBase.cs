using Peerly.Core.IntegrationTests.Features.V1.Submissions.UpdateSubmittedHomework.Infrastructure;
using Peerly.Core.IntegrationTests.Infrastructure;

namespace Peerly.Core.IntegrationTests.Features.V1.Submissions.UpdateSubmittedHomework;

public abstract class UpdateSubmittedHomeworkIntegrationTestBase : SubmissionIntegrationTestBase
{
    protected UpdateSubmittedHomeworkIntegrationTestBase(IntegrationTestFixture fixture)
        : base(fixture)
    {
    }

    protected UpdateSubmittedHomeworkGrpcClient UpdateSubmittedHomeworkClient => Fixture.UpdateSubmittedHomeworkClient;
}
