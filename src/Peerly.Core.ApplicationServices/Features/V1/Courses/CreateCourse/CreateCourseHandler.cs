using System.Threading;
using System.Threading.Tasks;
using OneOf.Types;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Courses.CreateCourse.Abstractions;
using Peerly.Core.ApplicationServices.Models.Common;

namespace Peerly.Core.ApplicationServices.Features.V1.Courses.CreateCourse;

internal sealed class CreateCourseHandler : ICommandHandler<CreateCourseCommand, Success>
{
    private readonly ICommonUnitOfWorkFactory _unitOfWorkFactory;
    private readonly ICreateCourseHandlerMapper _mapper;

    public CreateCourseHandler(ICommonUnitOfWorkFactory unitOfWorkFactory, ICreateCourseHandlerMapper mapper)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
        _mapper = mapper;
    }

    public async Task<CommandResponse<Success>> ExecuteAsync(CreateCourseCommand command, CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _unitOfWorkFactory.CreateAsync(cancellationToken);

        var courseAddItem = _mapper.ToCourseAddItem(command);
        var courseId = await unitOfWork.CourseRepository.AddAsync(courseAddItem, cancellationToken);

        var courseTeacherAddItem = _mapper.ToCourseTeacherAddItem(command, courseId);
        _ = await unitOfWork.CourseTeacherRepository.AddAsync(courseTeacherAddItem, cancellationToken);

        return new Success();
    }
}
