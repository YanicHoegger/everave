using AspNetCore.Identity.Mongo;
using AspNetCore.Identity.Mongo.Model;
using Azure.Storage.Blobs;
using everave.server.Components;
using everave.server.Components.AzureDeployment;
using everave.server.Components.GitHub;
using everave.server.Forum;
using everave.server.Import;
using everave.server.Services;
using everave.server.UserManagement;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;

Console.WriteLine("Starting eve&rave server");

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddConsole();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddControllers();
builder.Services.AddHttpClient();

var useAzureBlobStorage = builder.Configuration.GetValue<bool>("UseAzureBlobStorage");

if (useAzureBlobStorage)
{
    Console.WriteLine("Using azure blob storage");
    var blobConnectionString = builder.Configuration["BlobStorageConnectionString"];
    builder.Services.AddSingleton(new BlobServiceClient(blobConnectionString));
    builder.Services.AddSingleton<IImageStorageService, AzureBlobStorageService>();
}
else
{
    Console.WriteLine("Using image service");
    builder.Services.AddHttpClient("ImageService", client =>
    {
        client.BaseAddress = new Uri("http://imagehandler:8080");
    });

    builder.Services.AddSingleton<IImageStorageService, LocalImageStorageService>();
}

builder.Services.AddSingleton<ForumNotifier>();
builder.Services.AddSingleton<IForumNotifier>(x => x.GetService<ForumNotifier>());
builder.Services.AddScoped<FileReferenceHandler>();
builder.Services.AddScoped<UserFinderService>();
builder.Services.AddScoped<IForumService, ForumService>();
builder.Services.AddScoped<Importer>();
builder.Services.AddScoped<AvatarCreationService>();

var connectionString = builder.Configuration["MongoDbConnectionString"];
builder.Services.AddSingleton(_ =>
{
    var client = new MongoClient(connectionString);
    return client.GetDatabase("everave");
});

builder.Services.AddIdentityMongoDbProvider<ApplicationUser, MongoRole>(identityOptions =>
{
    identityOptions.Password.RequireDigit = false;
    identityOptions.Password.RequireNonAlphanumeric = false;
    identityOptions.Password.RequiredLength = 6;
    identityOptions.User.AllowedUserNameCharacters = null;
}, mongoIdentityOptions =>
{
    mongoIdentityOptions.ConnectionString = connectionString;
})
//.AddUserValidator<PhpBBUsernameValidator>()
.AddRoles<MongoRole>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
});
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddScoped<AuthenticationStateProvider, MongoAuthenticationStateProvider>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddHostedService<CreateMasterAccountHostedService>();

if (builder.Configuration.GetValue<bool>("UseAzureDeployment"))
{
    builder.Services.AddScoped<IAzureDeploymentService, AzureWebAppService>();
    builder.Services.AddScoped<IGitHubAccess, GitHubAccess>();
}
else
{
    builder.Services.AddScoped<IAzureDeploymentService, EmptyAzureDeploymentService>();
    builder.Services.AddScoped<IGitHubAccess, NoGitHubAccess>();
}

if (builder.Configuration.GetValue<bool>("UseElasticSearch"))
{
    builder.Services.AddHostedService<ElasticSearchHostedService>();
    builder.Services.AddHostedService<ElasticIndexer>();
    builder.Services.AddScoped<ISearchService, ElasticSearch>();
}
else
{
    builder.Services.AddScoped<ISearchService, EmptySearch>();
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles();
app.MapControllers();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
