using OnlineCourses.Domain.Entities;

namespace OnlineCourses.Application.Interfaces.Repositories;

// Application/Enrollments/IEnrollmentRepository.cs
public interface IEnrollmentRepository
{
    Task<Enrollment?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Enrollment?> GetByStudentAndCourseAsync(
        string studentId,
        int courseId,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<Enrollment>> GetByStudentAsync(
        string studentId,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<Enrollment>> GetByCourseAsync(
        int courseId,
        CancellationToken cancellationToken = default);

    Task AddAsync(Enrollment enrollment, CancellationToken cancellationToken = default);

    void Remove(Enrollment enrollment);
}