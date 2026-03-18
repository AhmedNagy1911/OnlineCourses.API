using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineCourses.API.Extensions;
using OnlineCourses.Application.DTOs;
using OnlineCourses.Application.Interfaces.Services;
using OnlineCourses.Domain.Constants;

namespace OnlineCourses.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CoursesController(ICourseService courseService) : ControllerBase
{
    private readonly ICourseService _courseService = courseService;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var courses = await _courseService.GetAllAsync();
        return Ok(courses);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _courseService.GetByIdAsync(id);
        return result.IsFailure ? result.ToProblem() : Ok(result.Value);
    }

    [HttpPost]
    //[Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> Create(CreateCourseRequest request)
    {
        var course = await _courseService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = course.Id }, course);
    }

    [HttpPut("{id}")]
    //[Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Instructor}")]
    public async Task<IActionResult> Update(int id, CreateCourseRequest request)
    {
        await _courseService.UpdateAsync(id, request);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _courseService.DeleteAsync(id);
        return NoContent();
    }
}
