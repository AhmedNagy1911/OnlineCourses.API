namespace OnlineCourses.Application.Enrollments.DTOs;

public record EnrollmentResponse(
    int Id,
    int CourseId,
    string CourseTitle,
    string StudentFullName,
    DateTime EnrolledAt
);