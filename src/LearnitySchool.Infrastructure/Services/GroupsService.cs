using LearnitySchool.Application.Abstractions;
using LearnitySchool.Application.Features.Groups.Dtos;
using LearnitySchool.Domain.Entities;
using LearnitySchool.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LearnitySchool.Infrastructure.Services;

public class GroupsService : IGroupsService
{
    private readonly AppDbContext _db;

    public GroupsService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<GroupDto>> GetTeacherGroupsAsync(string teacherUserId, CancellationToken ct = default)
    {
        return await _db.Groups
            .Where(g => g.TeacherUserId == teacherUserId)
            .OrderBy(g => g.Name)
            .Select(g => new GroupDto(g.Id, g.Name))
            .ToListAsync(ct);
    }

    public async Task CreateGroupAsync(string teacherUserId, string name, CancellationToken ct = default)
    {
        var group = new Group { TeacherUserId = teacherUserId, Name = name.Trim() };
        _db.Groups.Add(group);
        await _db.SaveChangesAsync(ct);
    }
}
