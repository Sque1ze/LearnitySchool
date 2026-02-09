using System;
using System.Collections.Generic;

namespace LearnitySchool.Web.ViewModels.Teacher.Groups;

public class TeacherGroupSuccessVm
{
    public Guid CourseId { get; set; }
    public string CourseTitle { get; set; } = "";

    public Guid? SelectedLessonId { get; set; }

    public List<LessonSuccessVm> Lessons { get; set; } = new();
    public List<StudentSuccessRowVm> Students { get; set; } = new();
}

public class LessonSuccessVm
{
    public Guid LessonId { get; set; }
    public string Title { get; set; } = "";
    public int Percent { get; set; } // 0..100
    public int TasksCount { get; set; }
}

public class StudentSuccessRowVm
{
    public string StudentId { get; set; } = "";
    public string StudentName { get; set; } = "";
    public int Percent { get; set; } // 0..100
}
