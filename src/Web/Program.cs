using CleanArchitectureBase.Application;
using CleanArchitectureBase.Infrastructure;
using CleanArchitectureBase.Infrastructure.Data;
using CleanArchitectureBase.Web;
using CleanArchitectureBase.Web.Attributes;
using Hangfire;
using Hangfire.PostgreSql;


var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
   
    //Đây là thời gian tối đa Kestrel chờ client gửi toàn bộ request headers
    options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(1);    
});

DotNetEnv.Env.Load("../../.env");
builder.Configuration.AddEnvironmentVariables();
// Add services to the container.
builder.Services.AddKeyVaultIfConfigured(builder.Configuration);

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddWebServices();
builder.Services.AddScoped<PaymentAuthEndpointFilter>();

builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(options =>
    {
        options.UseNpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection"));
    }));

builder.Services.AddHangfireServer();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    await app.InitialiseDatabaseAsync();
}
else
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    // app.UseHsts();
}

app.UseHealthChecks("/health");
// app.UseHttpsRedirection();

// CORS MIDDLEWARE
// if (app.Environment.IsDevelopment())
//     app.UseCors("AllowAll");
// else
    app.UseCors("AllowSpecificOrigins");


app.UseStaticFiles();
app.UseAuthentication();
app.UseHangfireDashboard("/hangfire");

app.UseSwaggerUi(settings =>
{
    settings.Path = "/api";
    settings.DocumentPath = "/api/specification.json";
    settings.DocExpansion = "list"; //none/list/full
});


app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.UseExceptionHandler(options => { });    

app.Map("/", () => Results.Redirect("/api"));


app.MapEndpoints();

app.Run();


public partial class Program
{
}
