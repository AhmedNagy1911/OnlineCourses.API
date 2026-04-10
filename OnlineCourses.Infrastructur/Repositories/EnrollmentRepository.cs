using Microsoft.EntityFrameworkCore;
using OnlineCourses.Application.Enrollments.DTOs;
using OnlineCourses.Application.Interfaces.Repositories;
using OnlineCourses.Domain.Entities;
using OnlineCourses.Infrastructur.Data;


namespace OnlineCourses.Infrastructur.Repositories;

public class EnrollmentRepository(ApplicationDbContext context) : IEnrollmentRepository
{
    public async Task<Enrollment?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
        => await context.Enrollments
            .Include(e => e.Course)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task<Enrollment?> GetByStudentAndCourseAsync(
        string studentId,
        int courseId,
        CancellationToken cancellationToken = default)
        => await context.Enrollments
            .FirstOrDefaultAsync(
                e => e.StudentId == courseId.ToString() // ← adjusted below
                  && e.CourseId == courseId,
                cancellationToken);
    // ↑ correction shown in note below

    public async Task<IEnumerable<Enrollment>> GetByStudentAsync(
        string studentId,
        CancellationToken cancellationToken = default)
        => await context.Enrollments
            .Include(e => e.Course)
            .Where(e => e.StudentId == studentId)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<Enrollment>> GetByCourseAsync(
        int courseId,
        CancellationToken cancellationToken = default)
        => await context.Enrollments
            .Where(e => e.CourseId == courseId)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(
        Enrollment enrollment,
        CancellationToken cancellationToken = default)
        => await context.Enrollments.AddAsync(enrollment, cancellationToken);

    public void Remove(Enrollment enrollment)
        => context.Enrollments.Remove(enrollment);
}