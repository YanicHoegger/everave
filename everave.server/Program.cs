using AspNetCore.Identity.Mongo;
using AspNetCore.Identity.Mongo.Model;
using Azure.Storage.Blobs;
using everave.server.Components;
using everave.server.Forum;
using everave.server.Import;
using everave.server.Services;
using everave.server.UserManagement;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddControllers();
builder.Services.AddHttpClient();


var useAzureBlobStorage = builder.Configuration.GetValue<bool>("UseAzureBlobStorage");

if (useAzureBlobStorage)
{
    var blobConnectionString = builder.Configuration["BlobStorageConnectionString"];
    builder.Services.AddSingleton(new BlobServiceClient(blobConnectionString));
    builder.Services.AddSingleton<IImageStorageService, AzureBlobStorageService>();
}
else
{
    builder.Services.AddHttpClient("ImageService", client =>
    {
        client.BaseAddress = new Uri("http://imagehandler:8080");
    });

    builder.Services.AddSingleton<IImageStorageService, LocalImageStorageService>();
}

builder.Services.AddSingleton<IForumService, ForumService>();
builder.Services.AddScoped<Importer>();

const string mongodb = "MongoDb";
builder.Services.AddSingleton(sp =>
{
    var config = sp.GetService<IConfiguration>() ?? throw new InvalidOperationException($"{nameof(IConfiguration)} could not be found");
    var client = new MongoClient(config.GetConnectionString(mongodb));
    return client.GetDatabase("everave");
});

builder.Services.AddIdentityMongoDbProvider<ApplicationUser, MongoRole>(identityOptions =>
{
    identityOptions.Password.RequireDigit = false;
    identityOptions.Password.RequireNonAlphanumeric = false;
    identityOptions.Password.RequiredLength = 6;
}, mongoIdentityOptions =>
{
    mongoIdentityOptions.ConnectionString = builder.Configuration.GetConnectionString(mongodb);
})
.AddRoles<MongoRole>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
});
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddScoped<AuthenticationStateProvider, MongoAuthenticationStateProvider>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddHostedService<CreateMasterAccountHostedService>();

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
