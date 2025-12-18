using FCG.Infrastructure;
using FCG.Presentation.Configuration;
using FCG.Presentation.Middlewares;
using GraphQL.AspNet.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Adicionar serviços ao contêiner.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerDocumentation();

// Application Insights - APM
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
    options.EnableAdaptiveSampling = true;
    options.EnableDependencyTrackingTelemetryModule = true;
    options.EnableRequestTrackingTelemetryModule = true;
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorization();
builder.Services.AddAppServices();
builder.Host.ConfigureSerilog();

var app = builder.Build();

// Configurar o pipeline de requisições HTTP.
// Esta seção agora é executada imediatamente, sem bloqueios.
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<SerilogRequestLoggingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.UseGraphQL();

// --- SEÇÃO CORRIGIDA ---
// Executa as tarefas de inicialização do banco de dados de forma assíncrona
// antes de iniciar o servidor web.
await InitializeDatabaseAsync(app);

// Inicia a aplicação para escutar por requisições
await app.RunAsync();

// Função auxiliar para encapsular a lógica de inicialização de forma segura
static async Task InitializeDatabaseAsync(IHost app)
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("Applying database migrations...");
        var dbContext = services.GetRequiredService<AppDbContext>();
        
        // Verifica se há migrations pendentes antes de aplicar
        var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
        if (pendingMigrations.Any())
        {
            logger.LogInformation("Found {Count} pending migrations. Applying...", pendingMigrations.Count());
            await dbContext.Database.MigrateAsync();
            logger.LogInformation("Database migrations applied successfully.");
        }
        else
        {
            logger.LogInformation("No pending migrations. Database is up to date.");
        }

        logger.LogInformation("Seeding database roles...");
        await IdentityConfiguration.SeedRolesAsync(services);
        logger.LogInformation("Database roles seeded successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred during database initialization.");
        // Lançar a exceção aqui fará com que a aplicação pare se o banco de dados
        // não puder ser inicializado, o que é um comportamento desejável.
        throw;
    }
}