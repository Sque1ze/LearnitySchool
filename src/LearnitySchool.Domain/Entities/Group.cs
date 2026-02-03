using LearnitySchool.Domain.Common;

namespace LearnitySchool.Domain.Entities;

public class Group : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    // Example: Teacher who owns this group (Identity User Id)
    public string TeacherUserId { get; set; } = string.Empty;
}
