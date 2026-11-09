namespace CollegeJournal.Web.Models;
public class Teacher
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
}

public class Subject
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int TeacherId { get; set; }
}

public class Grade
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int SubjectId { get; set; }
    public int Value { get; set; }
    public DateTime GradeDate { get; set; }
}
