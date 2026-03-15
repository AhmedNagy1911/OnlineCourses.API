using OnlineCourses.Application.Interfaces.Repositories;
using OnlineCourses.Domain.Entities;

namespace OnlineCourses.Application.Services;

public class CourseService(ICourseRepository courseRepository) : ICourseService
{
    private readonly ICourseRepository _courseRepository = courseRepository;

    public async Task<List<Course>> GetAllAsync()
    {
      return await _courseRepository.GetAllAsync();
    }

    public async Task<Course> GetByIdAsync(int id)
    {
      return await _courseRepository.GetByIdAsync(id); 
    }

    public Task AddAsync(Course course)
    {
       return _courseRepository.AddAsync(course);
    }
    public async Task UpdateAsync(Course course)
    {
       await _courseRepository.UpdateAsync(course);
    }
    public async Task DeleteAsync(int id)
    {
       await _courseRepository.DeleteAsync(id);
    }
    
}
