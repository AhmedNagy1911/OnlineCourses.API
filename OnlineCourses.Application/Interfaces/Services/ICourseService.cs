using OnlineCourses.Application.DTOs;

namespace OnlineCourses.Application.Interfaces.Services;

public interface ICourseService
{
    Task<IEnumerable<CourseResponse>> GetAllAsync();
    Task<CourseResponse?> GetByIdAsync(int id);
    Task<CourseResponse> CreateAsync(CreateCourseRequest request);
    Task UpdateAsync(int id, CreateCourseRequest request);
    Task DeleteAsync(int id);
}