using OnlineCourses.Application.Enrollments.DTOs;
using OnlineCourses.Domain.Common;


namespace OnlineCourses.Application.Interfaces.Services;

public interface IEnrollmentService
{
    Task<Result<EnrollmentResponse>> EnrollAsync(
        string studentId,
        EnrollmentRequest request,
        CancellationToken cancellationToken = default);

    Task<Result> UnenrollAsync(
        string studentId,
        int enrollmentId,
        CancellationToken cancellationToken = default);

    Task<Result<IEnumerable<EnrollmentResponse>>> GetMyEnrollmentsAsync(
        string studentId,
        CancellationToken cancellationToken = default);

    Task<Result<IEnumerable<EnrollmentResponse>>> GetCourseEnrollmentsAsync(
        int courseId,
        CancellationToken cancellationToken = default);
}