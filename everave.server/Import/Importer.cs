using everave.server.Forum;
using everave.server.Services;
using everave.server.UserManagement;
using Microsoft.AspNetCore.Identity;
using System.Web;

namespace everave.server.Import;

public class Importer(IForumService forumService, 
    UserManager<ApplicationUser> userManager, 
    IConfiguration configuration,
    AvatarCreationService avatarCreationService,
    ILogger<Importer> logger)
{
    public async Task Import(ImportData importData)
    {
        var passphrase = configuration["ImportApiKey"]
                         ?? throw new InvalidOperationException($"Can not use {nameof(Import)} without a key");

        foreach (var user in importData.Users)
        {
            await ImportUser(user, passphrase);
        }

        foreach (var forumGroup in importData.ForumGroups)
        {
            await ImportForumGroup(forumGroup);
        }
    }

    private async Task ImportForumGroup(ImportData.ForumGroup forumGroup)
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
                var author = await userManager.FindByNameAsync(topic.Author);
                if (author == null)
                {
                    throw new Exception($"Author {topic.Author} not found.");
                }

                var newTopic = new Topic
                {
                    Title = topic.Title,
                    ForumId = newForum.Id,
                    CreatedByUserId = author.Id,
                    CreatedAt = topic.CreatedAt
                };

                await forumService.AddTopicAsync(newTopic);

                foreach (var entry in topic.Posts)
                {
                    var entryAuthor = await userManager.FindByNameAsync(entry.Author);
                    if (entryAuthor == null)
                    {
                        throw new Exception($"Post author {entry.Author} not found.");
                    }

                    var newPost = new Post
                    {
                        HtmlContent = entry.Content,
                        TopicId = newTopic.Id,
                        UserId = entryAuthor.Id,
                        CreatedAt = entry.CreatedAt,
                    };

                    await forumService.AddPostAsync(newPost);
                }
            }
        }
    }

    private async Task ImportUser(ImportData.User user, string passphrase)
    {
        var decryptedPassword = StringEncryption.Decrypt(user.EncryptedPassword, passphrase);

        var existingUser = await userManager.FindByNameAsync(user.UserName);
        if (existingUser == null)
        {
            var applicationUser = new ApplicationUser
            {
                UserName = user.UserName
            };

            await FillApplicationUser(applicationUser, user);

            var result = await userManager.CreateAsync(applicationUser, decryptedPassword);
            if (!result.Succeeded)
            {
                throw new Exception($"Failed to create user {user.UserName}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
        else
        {
            await FillApplicationUser(existingUser, user);
            await userManager.UpdateAsync(existingUser);

            await userManager.RemovePasswordAsync(existingUser);
            await userManager.AddPasswordAsync(existingUser, decryptedPassword);
        }
    }

    private async Task FillApplicationUser(ApplicationUser applicationUser, ImportData.User user)
    {
        await UploadImage(applicationUser, user.ProfilePictureUrl);

        applicationUser.Signature = user.Signature;
        applicationUser.CreatedAt = user.CreatedAt;
        applicationUser.UserDetails = user.UserDetails;
    }

    private async Task UploadImage(ApplicationUser applicationUser, string imageUrl)
    {
        if (string.IsNullOrEmpty(imageUrl))
        {
            return;
        }

        var httpResponseMessage = await new HttpClient().GetAsync(imageUrl);

        if (!httpResponseMessage.IsSuccessStatusCode)
        {
            logger.LogWarning($"Failed to download image from {imageUrl}: {httpResponseMessage.StatusCode}");
            return;
        }

        var stream = await httpResponseMessage.Content.ReadAsStreamAsync();

        try
        {
            await avatarCreationService.Create(stream, applicationUser, CreateFileName(imageUrl));
        }
        catch (Exception e)
        {
            logger.LogError(e, $"Failed to create avatar from {imageUrl}");
        }
    }

    private static string CreateFileName(string imageUrl)
    {
        var uri = new Uri(imageUrl);

        var query = HttpUtility.ParseQueryString(uri.Query);
        var fileName = query["avatar"];

        if (string.IsNullOrEmpty(fileName))
        {
            fileName = Path.GetFileName(uri.LocalPath);

            if(fileName.Contains("?"))
                throw new Exception($"File name {fileName} is not valid");
        }

        return fileName;
    }
}