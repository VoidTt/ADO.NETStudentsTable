namespace CollegeJournal.Web.Models;

public class Group
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public override string ToString() => Name;
}

public class GroupInput
{
    public string? Name { get; set; }
}
