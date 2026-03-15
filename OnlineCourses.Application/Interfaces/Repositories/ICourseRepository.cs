using OnlineCourses.Domain.Entities;

namespace OnlineCourses.Application.Interfaces.Repositories;

public interface ICourseRepository
{
    Task<List<Course>> GetAllAsync();
    Task<Course> GetByIdAsync(int id);
    Task AddAsync(Course course);
    Task UpdateAsync(Course course);
    Task DeleteAsync(int id);
}
