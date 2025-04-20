using everave.server.Import;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace everave.server.Components.Controllers;

[ApiController]
[Route("api/import")]
public class ImportController(IConfiguration configuration, IServiceScopeFactory serviceScopeFactory) : ControllerBase
{
    [HttpPost]
    //TODO: Could this be a potential thread? Would this be a good target for DDos?
    [DisableRequestSizeLimit]
    public async Task<IActionResult> ImportData([FromHeader(Name = "X-Api-Key")] string apiKey)
    {
        var secretKey = configuration["ImportApiKey"];

        if (string.IsNullOrEmpty(secretKey))
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, "Import is currently unavailable.");
        }

        if (apiKey != secretKey)
        {
            return Unauthorized("Invalid API key.");
        }

        var importData = await DeserializeFromStreamAsync(Request.Body).ConfigureAwait(false);
        _ = Task.Run(async () =>
        {
            try
            {
                using var scope = serviceScopeFactory.CreateScope();
                var importer = scope.ServiceProvider.GetRequiredService<Importer>();

                importer.Import(importData).GetAwaiter().GetResult();
                await importer.Import(importData).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        });
        

        return Ok("Started import");
    }

    private async Task<ImportData> DeserializeFromStreamAsync(Stream stream)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        return await JsonSerializer.DeserializeAsync<ImportData>(stream, options)
               ?? throw new InvalidOperationException("Failed to deserialize ImportData.");
    }
}