using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Profile;

public class StudentProfileDto : BaseProfileDto
{
    public string? LearningGoals { get; set; }
    public int EnrolledCourses { get; set; }
    public int Achievements { get; set; }
    public int TotalHoursLearned { get; set; }
}

