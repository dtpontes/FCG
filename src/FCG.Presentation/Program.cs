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
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorization();
builder.Services.AddAppServices();
builder.Host.ConfigureSerilog();

// Adiciona um health check mais robusto que também verifica a conexão com o banco
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>();

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

// O endpoint de health check agora está disponível quase que instantaneamente.
app.MapHealthChecks("/health");

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
        await dbContext.Database.MigrateAsync(); // Usa a versão assíncrona
        logger.LogInformation("Database migrations applied successfully.");

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