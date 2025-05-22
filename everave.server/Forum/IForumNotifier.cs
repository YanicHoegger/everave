namespace everave.server.Forum
{
    public interface IForumNotifier
    {
        event Func<Post, Task> PostAdded;
        event Func<Post, Task> PostDeleted;
        event Func<Topic, Task> TopicAdded;
        event Func<Topic, Task> TopicDeleted;
    }
}
