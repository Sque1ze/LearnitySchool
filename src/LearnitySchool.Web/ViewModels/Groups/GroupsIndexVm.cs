using LearnitySchool.Application.Features.Groups.Dtos;

namespace LearnitySchool.Web.ViewModels.Groups;

public class GroupsIndexVm
{
    public IReadOnlyList<GroupDto> Groups { get; init; } = Array.Empty<GroupDto>();
}
