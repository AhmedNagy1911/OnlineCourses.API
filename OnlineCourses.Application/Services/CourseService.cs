using Mapster;
using OnlineCourses.Application.DTOs;
using OnlineCourses.Application.Interfaces.Repositories;
using OnlineCourses.Application.Interfaces.Services;
using OnlineCourses.Domain.Common;
using OnlineCourses.Domain.Common.Errors;
using OnlineCourses.Domain.Entities;

namespace OnlineCourses.Application.Services;

public class CourseService(ICourseRepository courseRepository) : ICourseService
{
    private readonly ICourseRepository _courseRepository = courseRepository;

    public async Task<IEnumerable<CourseResponse>> GetAllAsync()
    {
        var courses = await _courseRepository.GetAllAsync();
        return courses.Adapt<IEnumerable<CourseResponse>>();
    }

    public async Task<Result<CourseResponse>> GetByIdAsync(int id)
    {
        var course = await _courseRepository.GetByIdAsync(id);
        if (course is null)
            return Result.Failure<CourseResponse>(CourseErrors.NotFound);

        return Result.Success(course.Adapt<CourseResponse>());
    }

    public async Task<CourseResponse> CreateAsync(CreateCourseRequest request)
    {
        var course = request.Adapt<Course>();
        await _courseRepository.AddAsync(course);
        return course.Adapt<CourseResponse>();
    }

    public async Task UpdateAsync(int id, CreateCourseRequest request)
    {
        var course = await _courseRepository.GetByIdAsync(id);
        if (course is null) return;

        request.Adapt(course);

        await _courseRepository.UpdateAsync(course);
    }

    public async Task DeleteAsync(int id)
    {
        var course = await _courseRepository.GetByIdAsync(id);
        if (course is null) return;
        await _courseRepository.DeleteAsync(course);
    }
}