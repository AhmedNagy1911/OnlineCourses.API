using Microsoft.EntityFrameworkCore;
using OnlineCourses.Application.Interfaces.Repositories;
using OnlineCourses.Domain.Entities;
using OnlineCourses.Infrastructur.Data;

namespace OnlineCourses.Infrastructur.Repositories;

public class CourseRepository(ApplicationDbContext context) : ICourseRepository
{
    private readonly ApplicationDbContext _context = context;

    public async Task<List<Course>> GetAllAsync()
    {
       return await _context.Courses
            .Include(i => i.Instructor)
            .Include(l => l.Lessons)
            .ToListAsync();
    }

    public async Task<Course> GetByIdAsync(int id)
    {
        return await _context.Courses
            .Include(i => i.Instructor)
            .Include(l => l.Lessons)
            .FirstOrDefaultAsync(c => c.Id == id);
    }
    public async Task AddAsync(Course course)
    {
        _context.Courses.Add(course);
        await _context.SaveChangesAsync();

    }
    public async Task UpdateAsync(Course course)
    {
        _context.Courses.Update(course);
        await _context.SaveChangesAsync();
    }


    public async Task DeleteAsync(int id)
    {
        var course = await _context.Courses.FindAsync(id);
        if (course != null)
        {
            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();
        }
    }
}
