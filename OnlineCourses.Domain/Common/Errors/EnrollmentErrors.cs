namespace OnlineCourses.Domain.Common.Errors;

public static class EnrollmentErrors
{
    public static readonly Error AlreadyEnrolled =
        new("Enrollment.AlreadyEnrolled", "Student is already enrolled in this course.",409);

    public static readonly Error NotFound =
        new("Enrollment.NotFound", "Enrollment not found.", 404);

    public static readonly Error CourseNotFound =
        new("Enrollment.CourseNotFound", "Course not found.", 404);

    public static readonly Error StudentNotFound =
        new("Enrollment.StudentNotFound", "Student not found.", 404);

    public static readonly Error Unauthorized =
        new("Enrollment.Unauthorized", "You are not authorized to perform this action.", 403);
}