namespace everave.server.Components.Layout;

public static class PageUrlRegex
{
    // AddTopic.razor
    public const string AddTopicPattern = @"^/forum/(?<ForumId>[^/]+)/add-topic$";
    public const string AddTopicForumIdGroup = "ForumId";

    // Admin.razor
    public const string AdminPattern = @"^/admin$";

    // EditUser.razor
    public const string EditUserPattern = @"^/admin/edit-user/(?<UserId>[^/]+)$";
    public const string EditUserUserIdGroup = "UserId";

    // ForumOverview.razor
    public const string ForumOverviewPattern = @"^/forums$";

    // ForumTopics.razor
    public const string ForumTopicsPattern = @"^/forum/(?<Id>[^/]+)$";
    public const string ForumTopicsIdGroup = "Id";

    // Register.razor
    public const string RegisterPattern = @"^/register$";

    // TopicPage.razor
    public const string TopicPagePattern = @"^/topic/(?<Id>[^/]+)";
    public const string TopicPageIdGroup = "Id";

    // UserDetails.razor
    public const string UserDetailsPattern = @"^/user-details/(?<UserId>[^/]+)$";
    public const string UserDetailsUserIdGroup = "UserId";
}