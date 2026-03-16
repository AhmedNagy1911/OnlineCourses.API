namespace OnlineCourses.Domain.Common.Errors;

public static class CourseErrors
{
    public static readonly Error NotFound =
        new("Course.NotFound", "Course was not found.", 404);

    public static readonly Error AlreadyExists =
        new("Course.AlreadyExists", "Course already exists.", 409);
}