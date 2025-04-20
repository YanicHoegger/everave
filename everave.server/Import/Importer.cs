using everave.server.Forum;
using everave.server.UserManagement;
using Microsoft.AspNetCore.Identity;

namespace everave.server.Import;

public class Importer(IForumService forumService, UserManager<ApplicationUser> userManager, IConfiguration configuration)
{
    public async Task Import(ImportData importData)
    {
        var passphrase = configuration["ImportApiKey"]
                         ?? throw new InvalidOperationException($"Can not use {nameof(Import)} without a key");

        foreach (var user in importData.Users)
        {
            var applicationUser = new ApplicationUser
            {
                UserName = user.Username,
                DisplayName = user.Username
            };

            var decryptedPassword = StringEncryption.Decrypt(user.EncryptedPassword, passphrase);

            var result = await userManager.CreateAsync(applicationUser, decryptedPassword);
            if (!result.Succeeded)
            {
                throw new Exception($"Failed to create user {user.Username}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }

        if (importData.ForumGroups != null)
        {
            foreach (var forumGroup in importData.ForumGroups)
            {
                var newForumGroup = new ForumGroup
                {
                    Description = forumGroup.Description
                };

                await forumService.AddForumGroupAsync(newForumGroup);

                foreach (var forum in forumGroup.Forums)
                {
                    var newForum = new Forum.Forum
                    {
                        Name = forum.Name,
                        Description = forum.Description,
                        GroupId = newForumGroup.Id
                    };

                    await forumService.AddForumAsync(newForum);

                    foreach (var topic in forum.Topics)
                    {
                        var author = await userManager.FindByNameAsync(topic.Author.Username);
                        if (author == null)
                        {
                            throw new Exception($"Author {topic.Author.Username} not found.");
                        }

                        var newTopic = new Topic
                        {
                            Title = topic.Title,
                            ForumId = newForum.Id,
                            CreatedByUserId = author.Id,
                            CreatedAt = topic.CreatedAt
                        };

                        await forumService.AddTopicAsync(newTopic);

                        foreach (var entry in topic.Entries)
                        {
                            var entryAuthor = await userManager.FindByNameAsync(entry.Author.Username);
                            if (entryAuthor == null)
                            {
                                throw new Exception($"Entry author {entry.Author.Username} not found.");
                            }

                            var newEntry = new Entry
                            {
                                HtmlContent = entry.Content,
                                TopicId = newTopic.Id,
                                UserId = entryAuthor.Id,
                                CreatedAt = entry.CreatedAt,
                            };

                            await forumService.AddEntryAsync(newEntry);
                        }
                    }
                }
            }
        }
    }
}