using FCG.Infrastructure;
using FCG.Presentation.Configuration;
using FCG.Presentation.Middlewares;
using GraphQL.AspNet.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;



// Add services to the container.

var builder = WebApplication.CreateBuilder(args);

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

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}

// Configure the HTTP request pipeline.

app.UseSwagger();
app.UseSwaggerUI();


app.UseHttpsRedirection();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<SerilogRequestLoggingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.UseGraphQL();

using (var scope = app.Services.CreateScope())
{
    await IdentityConfiguration.SeedRolesAsync(scope.ServiceProvider);
}

await app.RunAsync();
