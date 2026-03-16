namespace OnlineCourses.Domain.Entities;

public class Enrollment
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public Course Course { get; set; } = default!;

    public int StudentId { get; set; }
    public Student Student { get; set; } = default!;

    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
}
