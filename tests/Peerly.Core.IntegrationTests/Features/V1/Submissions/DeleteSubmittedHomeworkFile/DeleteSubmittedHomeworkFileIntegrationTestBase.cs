using Peerly.Core.IntegrationTests.Features.V1.Submissions.DeleteSubmittedHomework;
using Peerly.Core.IntegrationTests.Features.V1.Submissions.DeleteSubmittedHomeworkFile.Infrastructure;
using Peerly.Core.IntegrationTests.Infrastructure;

namespace Peerly.Core.IntegrationTests.Features.V1.Submissions.DeleteSubmittedHomeworkFile;

public abstract class DeleteSubmittedHomeworkFileIntegrationTestBase : DeleteSubmittedHomeworkIntegrationTestBase
{
    protected DeleteSubmittedHomeworkFileIntegrationTestBase(IntegrationTestFixture fixture)
        : base(fixture)
    {
    }

    protected DeleteSubmittedHomeworkFileGrpcClient DeleteSubmittedHomeworkFileClient => Fixture.DeleteSubmittedHomeworkFileClient;
}
