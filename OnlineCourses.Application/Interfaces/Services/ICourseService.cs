using OnlineCourses.Application.DTOs;
using OnlineCourses.Domain.Common;

namespace OnlineCourses.Application.Interfaces.Services;

public interface ICourseService
{
    Task<IEnumerable<CourseResponse>> GetAllAsync();
    Task<Result<CourseResponse>> GetByIdAsync(int id);
    Task<CourseResponse> CreateAsync(CreateCourseRequest request);
    Task UpdateAsync(int id, CreateCourseRequest request);
    Task DeleteAsync(int id);
}