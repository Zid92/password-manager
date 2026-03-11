using Microsoft.AspNetCore.Mvc;
using PasswordManager.Contracts;
using PasswordManager.Core.Data;
using PasswordManager.Core.Models;
using PasswordManager.Core.Services;

var builder = WebApplication.CreateBuilder(args);

// Core services
builder.Services.AddSingleton<IDatabaseService, DatabaseService>();
builder.Services.AddSingleton<IEncryptionService, EncryptionService>();
builder.Services.AddSingleton<ICredentialService, CredentialService>();
builder.Services.AddSingleton<IBreachCheckService, BreachCheckService>();

// CORS for web and native clients
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowAnyOrigin();
    });
});

// OpenAPI
builder.Services.AddOpenApi();

// Controllers / Minimal API
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors();

// Vault management
app.MapPost("/api/vault/open", async ([FromBody] OpenVaultRequest request, IDatabaseService database, IEncryptionService encryption) =>
{
    await database.InitializeAsync(request.MasterPassword);
    encryption.Initialize(request.MasterPassword);
    return Results.Ok(new OpenVaultResponse { Success = true });
});

app.MapPost("/api/vault/lock", (IDatabaseService database, IEncryptionService encryption) =>
{
    database.Close();
    encryption.Clear();
    return Results.Ok();
});

// Credentials CRUD
app.MapGet("/api/credentials", async (IDatabaseService database) =>
{
    var items = await database.GetAllCredentialsAsync();
    return Results.Ok(items);
});

app.MapGet("/api/credentials/{id:int}", async (int id, IDatabaseService database) =>
{
    var item = await database.GetCredentialByIdAsync(id);
    return item is null ? Results.NotFound() : Results.Ok(item);
});

app.MapPost("/api/credentials", async ([FromBody] SaveCredentialRequest request, ICredentialService credentials) =>
{
    var credential = new Credential
    {
        Title = request.Title,
        Username = request.Username,
        Url = request.Url,
        Notes = request.Notes,
        Category = request.Category
    };

    var id = await credentials.SaveAsync(credential, request.PlainPassword);
    return Results.Ok(new { Id = id });
});

app.MapPut("/api/credentials/{id:int}", async (int id, [FromBody] SaveCredentialRequest request, IDatabaseService database, ICredentialService credentials) =>
{
    var existing = await database.GetCredentialByIdAsync(id);
    if (existing is null)
        return Results.NotFound();

    existing.Title = request.Title;
    existing.Username = request.Username;
    existing.Url = request.Url;
    existing.Notes = request.Notes;
    existing.Category = request.Category;

    await credentials.SaveAsync(existing, request.PlainPassword);
    return Results.NoContent();
});

app.MapDelete("/api/credentials/{id:int}", async (int id, ICredentialService credentials) =>
{
    var result = await credentials.DeleteAsync(id);
    return result == 0 ? Results.NotFound() : Results.NoContent();
});

app.MapGet("/api/credentials/search", async ([FromQuery] string q, ICredentialService credentials) =>
{
    var items = await credentials.SearchAsync(q);
    return Results.Ok(items);
});

// Breach check
app.MapPost("/api/breach-check", async ([FromBody] BreachCheckRequest request, IBreachCheckService breachCheck) =>
{
    var result = await breachCheck.CheckPasswordAsync(request.Password);
    return Results.Ok(result);
});

app.Run();
