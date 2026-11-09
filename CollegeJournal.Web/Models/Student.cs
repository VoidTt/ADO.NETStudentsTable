namespace CollegeJournal.Web.Models;

public class Student
{
    public int Id { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public int Age { get; set; }
    public int GroupId { get; set; }

    public Group? Group { get; set; }
    public override string ToString() => $"{LastName} {FirstName}, {Age} лет";
}

public class StudentInput
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public int Age { get; set; }
    public int GroupId { get; set; }
}
