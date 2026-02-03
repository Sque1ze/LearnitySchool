using LearnitySchool.Application.Features.Groups.Dtos;

namespace LearnitySchool.Application.Abstractions;

public interface IGroupsService
{
    Task<IReadOnlyList<GroupDto>> GetTeacherGroupsAsync(string teacherUserId, CancellationToken ct = default);
    Task CreateGroupAsync(string teacherUserId, string name, CancellationToken ct = default);
}
