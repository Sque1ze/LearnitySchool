using LearnitySchool.Domain.Enums;

namespace LearnitySchool.Web.ViewModels.Notifications;

public class NotificationsIndexVm
{
    public List<NotificationRowVm> Items { get; set; } = new();
}

public class NotificationRowVm
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string? Url { get; set; }
}
