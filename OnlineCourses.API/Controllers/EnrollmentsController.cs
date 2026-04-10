using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlineCourses.API.Extensions;
using OnlineCourses.Application.Enrollments.DTOs;
using OnlineCourses.Application.Interfaces.Services;
using OnlineCourses.Domain.Constants;
using System.Security.Claims;

namespace OnlineCourses.API.Controllers;

//[Authorize]
[Route("api/[controller]")]
[ApiController]
public class EnrollmentsController(IEnrollmentService enrollmentService) : ControllerBase
{
    // POST api/enrollments
    [HttpPost]
    public async Task<IActionResult> Enroll(
        [FromBody] EnrollmentRequest request,
        CancellationToken cancellationToken)
    {
        var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await enrollmentService.EnrollAsync(studentId, request, cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetMyEnrollments), result.Value)
            : result.ToProblem();
    }

    // DELETE api/enrollments/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Unenroll(
        int id,
        CancellationToken cancellationToken)
    {
        var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await enrollmentService.UnenrollAsync(studentId, id, cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    // GET api/enrollments/my
    [HttpGet("my")]
    public async Task<IActionResult> GetMyEnrollments(CancellationToken cancellationToken)
    {
        var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await enrollmentService.GetMyEnrollmentsAsync(studentId, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    // GET api/enrollments/course/{courseId}  ← Admin only
    [HttpGet("course/{courseId:int}")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> GetCourseEnrollments(
        int courseId,
        CancellationToken cancellationToken)
    {
        var result = await enrollmentService.GetCourseEnrollmentsAsync(courseId, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }
}