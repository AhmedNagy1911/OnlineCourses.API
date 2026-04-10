using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using OnlineCourses.Application.Common.Interfaces;
using OnlineCourses.Application.Enrollments.DTOs;
using OnlineCourses.Application.Interfaces.Repositories;
using OnlineCourses.Application.Interfaces.Services;
using OnlineCourses.Domain.Common;
using OnlineCourses.Domain.Common.Errors;
using OnlineCourses.Domain.Entities;

namespace OnlineCourses.Application.Services;

// Application/Enrollments/EnrollmentService.cs
public class EnrollmentService(
    IEnrollmentRepository enrollmentRepository,
    ICourseRepository courseRepository,
    IValidator<EnrollmentRequest> validator,
    IUserService userService,            // ✅ بدل UserManager
    ILogger<EnrollmentService> logger) : IEnrollmentService
{
    public async Task<Result<EnrollmentResponse>> EnrollAsync(
        string studentId,
        EnrollmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var error = new Error(
                validationResult.Errors[0].PropertyName,
                validationResult.Errors[0].ErrorMessage,
                StatusCodes.Status400BadRequest);
            return Result.Failure<EnrollmentResponse>(error);
        }

        // ✅ بدل userManager.FindByIdAsync
        var studentExists = await userService.ExistsAsync(studentId, cancellationToken);
        if (!studentExists)
            return Result.Failure<EnrollmentResponse>(EnrollmentErrors.StudentNotFound);

        var course = await courseRepository.GetByIdAsync(request.CourseId);
        if (course is null)
            return Result.Failure<EnrollmentResponse>(EnrollmentErrors.CourseNotFound);

        var existing = await enrollmentRepository.GetByStudentAndCourseAsync(
            studentId, request.CourseId);
        if (existing is not null)
            return Result.Failure<EnrollmentResponse>(EnrollmentErrors.AlreadyEnrolled);

        var enrollment = new Enrollment
        {
            StudentId = studentId,
            CourseId = request.CourseId,
            EnrolledAt = DateTime.UtcNow
        };

        await enrollmentRepository.AddAsync(enrollment, cancellationToken);

        logger.LogInformation(
            "Student {StudentId} enrolled in Course {CourseId}",
            studentId, request.CourseId);

        // ✅ بدل student.FirstName + student.LastName
        var studentInfo = await userService.GetUserInfoAsync(studentId, cancellationToken);

        var response = new EnrollmentResponse(
            enrollment.Id,
            course.Id,
            course.Title,
            studentInfo!.Value.FullName,
            enrollment.EnrolledAt);

        return Result.Success(response);
    }

    public async Task<Result> UnenrollAsync(
        string studentId,
        int enrollmentId,
        CancellationToken cancellationToken = default)
    {
        var enrollment = await enrollmentRepository.GetByIdAsync(enrollmentId, cancellationToken);
        if (enrollment is null)
            return Result.Failure(EnrollmentErrors.NotFound);

        if (enrollment.StudentId != studentId)
            return Result.Failure(EnrollmentErrors.Unauthorized);

        enrollmentRepository.Remove(enrollment);
        return Result.Success();
    }

    public async Task<Result<IEnumerable<EnrollmentResponse>>> GetMyEnrollmentsAsync(
        string studentId,
        CancellationToken cancellationToken = default)
    {
        var studentExists = await userService.ExistsAsync(studentId, cancellationToken);
        if (!studentExists)
            return Result.Failure<IEnumerable<EnrollmentResponse>>(EnrollmentErrors.StudentNotFound);

        var enrollments = await enrollmentRepository.GetByStudentAsync(studentId, cancellationToken);
        var studentInfo = await userService.GetUserInfoAsync(studentId, cancellationToken);

        var responses = enrollments.Select(e => new EnrollmentResponse(
            e.Id,
            e.CourseId,
            e.Course.Title,
            studentInfo!.Value.FullName,
            e.EnrolledAt));

        return Result.Success(responses);
    }

    public async Task<Result<IEnumerable<EnrollmentResponse>>> GetCourseEnrollmentsAsync(
        int courseId,
        CancellationToken cancellationToken = default)
    {
        var course = await courseRepository.GetByIdAsync(courseId);
        if (course is null)
            return Result.Failure<IEnumerable<EnrollmentResponse>>(EnrollmentErrors.CourseNotFound);

        var enrollments = await enrollmentRepository.GetByCourseAsync(courseId, cancellationToken);

        var responses = new List<EnrollmentResponse>();
        foreach (var enrollment in enrollments)
        {
            var studentInfo = await userService.GetUserInfoAsync(enrollment.StudentId, cancellationToken);
            if (studentInfo is null) continue;

            responses.Add(new EnrollmentResponse(
                enrollment.Id,
                course.Id,
                course.Title,
                studentInfo.Value.FullName,
                enrollment.EnrolledAt));
        }

        return Result.Success<IEnumerable<EnrollmentResponse>>(responses);
    }
}