using CollegeJournal.Web.Data;
using CollegeJournal.Web.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<CollegeDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CollegeDB")));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.Use(async (context, next) =>
{
    try { await next(context); }
    catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 547 or 2601 or 2627 })
    {
        context.Response.StatusCode = 409;
        await context.Response.WriteAsJsonAsync(new { message = "Запись связана с другими данными или такое название уже существует." });
    }
    catch (Exception ex) when (ex is SqlException or DbUpdateException)
    {
        app.Logger.LogError(ex, "Ошибка обращения к CollegeDB");
        context.Response.StatusCode = 503;
        await context.Response.WriteAsJsonAsync(new { message = "Не удалось выполнить запрос к базе. Проверьте SQL Server и настройку подключения." });
    }
});

app.UseSwagger();
app.UseSwaggerUI();
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/students/all", async (string? name, CollegeDbContext db) =>
{
    var query = db.Students.AsNoTracking();
    if (!string.IsNullOrWhiteSpace(name))
    {
        string search = name.Trim();
        query = query.Where(s => s.FirstName.Contains(search) || s.LastName.Contains(search)
            || (s.LastName + " " + s.FirstName).Contains(search));
    }
    return Results.Ok(await query.OrderBy(s => s.LastName).ThenBy(s => s.FirstName).ThenBy(s => s.Id)
        .Select(s => new { s.Id, s.FirstName, s.LastName, s.Age, s.GroupId, GroupName = s.Group!.Name })
        .ToListAsync());
}).WithTags("Студенты");

app.MapGet("/students/{id:int}", async (int id, CollegeDbContext db) =>
{
    var student = await db.Students.AsNoTracking().Where(s => s.Id == id)
        .Select(s => new { s.Id, s.FirstName, s.LastName, s.Age, s.GroupId, GroupName = s.Group!.Name })
        .FirstOrDefaultAsync();
    return student is null ? Results.NotFound(new { message = "Студент не найден." }) : Results.Ok(student);
}).WithTags("Студенты");

app.MapPost("/students/add", async (StudentInput input, CollegeDbContext db) =>
{
    string? error = await ValidateStudent(input, db);
    if (error != null) return Results.BadRequest(new { message = error });
    var student = new Student
    {
        FirstName = input.FirstName!.Trim(), LastName = input.LastName!.Trim(),
        Age = input.Age, GroupId = input.GroupId
    };
    db.Students.Add(student);
    await db.SaveChangesAsync();
    return Results.Created($"/students/{student.Id}", new { student.Id });
}).WithTags("Студенты");

app.MapPut("/students/{id:int}", async (int id, StudentInput input, CollegeDbContext db) =>
{
    var student = await db.Students.FindAsync(id);
    if (student is null) return Results.NotFound(new { message = "Студент не найден." });
    string? error = await ValidateStudent(input, db);
    if (error != null) return Results.BadRequest(new { message = error });
    student.FirstName = input.FirstName!.Trim();
    student.LastName = input.LastName!.Trim();
    student.Age = input.Age;
    student.GroupId = input.GroupId;
    await db.SaveChangesAsync();
    return Results.Ok(new { student.Id });
}).WithTags("Студенты");

app.MapDelete("/students/{id:int}", async (int id, CollegeDbContext db) =>
{
    var student = await db.Students.FindAsync(id);
    if (student is null) return Results.NotFound(new { message = "Студент не найден." });
    if (await db.Grades.AnyAsync(g => g.StudentId == id))
        return Results.Conflict(new { message = "У студента есть оценки. Удаление запрещено, чтобы сохранить успеваемость." });
    db.Students.Remove(student);
    await db.SaveChangesAsync();
    return Results.NoContent();
}).WithTags("Студенты");

app.MapGet("/groups/all", async (CollegeDbContext db) =>
    Results.Ok(await db.Groups.AsNoTracking().OrderBy(g => g.Name)
        .Select(g => new { g.Id, g.Name, StudentCount = db.Students.Count(s => s.GroupId == g.Id) }).ToListAsync()))
    .WithTags("Группы");

app.MapPost("/groups/add", async (GroupInput input, CollegeDbContext db) =>
{
    string name = input.Name?.Trim() ?? "";
    if (name.Length == 0 || name.Length > 50)
        return Results.BadRequest(new { message = "Название группы: от 1 до 50 символов." });
    if (await db.Groups.AnyAsync(g => g.Name == name))
        return Results.Conflict(new { message = "Такая группа уже есть." });
    var group = new Group { Name = name };
    db.Groups.Add(group);
    await db.SaveChangesAsync();
    return Results.Created("/groups/all", new { group.Id, group.Name });
}).WithTags("Группы");

app.Run();

static async Task<string?> ValidateStudent(StudentInput input, CollegeDbContext db)
{
    if (string.IsNullOrWhiteSpace(input.FirstName) || string.IsNullOrWhiteSpace(input.LastName))
        return "Введите имя и фамилию.";
    if (input.FirstName.Trim().Length > 50 || input.LastName.Trim().Length > 50)
        return "Имя и фамилия не должны превышать 50 символов.";
    if (input.Age < 14 || input.Age > 100) return "Возраст должен быть от 14 до 100 лет.";
    if (!await db.Groups.AnyAsync(g => g.Id == input.GroupId)) return "Выберите существующую группу.";
    return null;
}
