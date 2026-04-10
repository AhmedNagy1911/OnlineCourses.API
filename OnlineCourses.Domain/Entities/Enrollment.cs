namespace OnlineCourses.Domain.Entities;

public class Enrollment
{
    public int Id { get; set; }
    public string StudentId { get; set; } = string.Empty; // string مش Guid
    public int CourseId { get; set; }
    public DateTime EnrolledAt { get; set; }

    public Course? Course { get; set; }

}
