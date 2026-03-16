namespace OnlineCourses.Application.DTOs;

public record CreateCourseRequest(
    string Title,
    string Description,
    decimal Price
);

public record CourseResponse(
    int Id,
    string Title,
    string Description,
    decimal Price
);