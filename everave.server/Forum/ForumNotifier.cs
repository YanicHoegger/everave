namespace everave.server.Forum
{
    public class ForumNotifier : IForumNotifier
    {
        public event Func<Post, Task>? PostAdded;
        public event Func<Post, Task>? PostDeleted;
        public event Func<Topic, Task>? TopicAdded;
        public event Func<Topic, Task>? TopicDeleted;

        public void OnPostAdded(Post post)
        {
            TriggerEvent(post, PostAdded);
        }

        public void OnPostDeleted(Post post)
        {
            TriggerEvent(post, PostDeleted);
        }

        public void OnTopicAdded(Topic topic)
        {
            TriggerEvent(topic, TopicAdded);
        }

        public void OnTopicDeleted(Topic topic)
        {
            TriggerEvent(topic, TopicDeleted);
        }

        public void TriggerEvent<T>(T arg, Func<T, Task>? asyncEvent)
        {
            if (asyncEvent != null)
            {
                _ = Task.Run(async () =>
                {
                    foreach (Func<T, Task> handler in asyncEvent.GetInvocationList())
                    {
                        await handler(arg).ConfigureAwait(false);
                    }
                });
            }
        }
    }
}
