namespace OnlineCourses.Domain.Entities;

public class Course
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public int  InstructorId { get; set; }
    public User Instructor { get; set; }

    public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();

}
