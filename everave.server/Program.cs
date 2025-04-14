using everave.server.Components;
using everave.server.Forum;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddControllers();
builder.Services.AddHttpClient();

builder.Services.AddHttpClient("ImageService", client =>
{
    client.BaseAddress = new Uri("http://imagehandler:8080");
});

builder.Services.AddSingleton<IForumService, ForumService>();
builder.Services.AddSingleton(sp =>
{
    var config = sp.GetService<IConfiguration>() ?? throw new InvalidOperationException($"{nameof(IConfiguration)} could not be found");
    var client = new MongoClient(config.GetConnectionString("MongoDb"));
    return client.GetDatabase("everave");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.MapControllers();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
