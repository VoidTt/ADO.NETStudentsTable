-- Создание новой базы. При повторном запуске данные не изменяются.
USE master;
IF DB_ID(N'CollegeDB') IS NOT NULL
    THROW 50001, N'CollegeDB уже существует. Данные сохранены; повторное создание не требуется.', 1;
EXEC(N'CREATE DATABASE CollegeDB');
EXEC(N'USE CollegeDB;
CREATE TABLE dbo.Groups (
    Id INT IDENTITY PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL,
    CONSTRAINT UQ_Groups_Name UNIQUE (Name)
);
CREATE TABLE dbo.Teachers (
    Id INT IDENTITY PRIMARY KEY,
    FullName NVARCHAR(100) NOT NULL
);
CREATE TABLE dbo.Subjects (
    Id INT IDENTITY PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    TeacherId INT NOT NULL REFERENCES dbo.Teachers(Id)
);
CREATE TABLE dbo.Students (
    Id INT IDENTITY PRIMARY KEY,
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    Age INT NOT NULL CONSTRAINT CK_Students_Age CHECK (Age BETWEEN 14 AND 100),
    GroupId INT NOT NULL REFERENCES dbo.Groups(Id)
);
CREATE TABLE dbo.Grades (
    Id INT IDENTITY PRIMARY KEY,
    StudentId INT NOT NULL REFERENCES dbo.Students(Id),
    SubjectId INT NOT NULL REFERENCES dbo.Subjects(Id),
    Value INT NOT NULL CONSTRAINT CK_Grades_Value CHECK (Value BETWEEN 1 AND 5),
    GradeDate DATE NOT NULL
);
CREATE INDEX IX_Students_GroupId ON dbo.Students(GroupId);
CREATE INDEX IX_Subjects_TeacherId ON dbo.Subjects(TeacherId);
CREATE INDEX IX_Grades_StudentId ON dbo.Grades(StudentId);
CREATE INDEX IX_Grades_SubjectId ON dbo.Grades(SubjectId);

INSERT INTO dbo.Groups (Name) VALUES (N''ПР-21''), (N''ИС-22''), (N''ПР-23'');
INSERT INTO dbo.Teachers (FullName) VALUES (N''Крылова Марина Олеговна''), (N''Белов Андрей Сергеевич'');
INSERT INTO dbo.Subjects (Name, TeacherId) VALUES (N''Доступ к базам данных'', 1), (N''Программирование'', 2);
INSERT INTO dbo.Students (FirstName, LastName, Age, GroupId) VALUES
    (N''Алина'', N''Морозова'', 18, 1), (N''Максим'', N''Соколов'', 19, 1),
    (N''Дарья'', N''Лебедева'', 18, 2), (N''Артём'', N''Волков'', 20, 2),
    (N''Ирина'', N''Зайцева'', 17, 3), (N''Павел'', N''Орлов'', 18, 3);
INSERT INTO dbo.Grades (StudentId, SubjectId, Value, GradeDate) VALUES
    (1, 1, 5, ''20261101''), (2, 2, 4, ''20261102'');

');

