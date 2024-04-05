using Asp.Versioning;
using Microsoft.AspNetCore.Http.Json;
using OpenApiPolymorphism;
using OpenApiPolymorphism.V1;
using OpenApiPolymorphism.V2;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddEndpointsApiExplorer()
    .AddApiVersioning(options =>
    {
        options.ReportApiVersions = true;
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.DefaultApiVersion = new ApiVersion(1, 0);

        options.ApiVersionReader = ApiVersionReader.Combine(
            new HeaderApiVersionReader("x-api-version"),
            new UrlSegmentApiVersionReader());
    })
    .AddApiExplorer(options =>
    {
        options.SubstituteApiVersionInUrl = true;
        options.GroupNameFormat = "'v'VVV";
    })
    .EnableApiVersionBinding();

services.AddSwaggerGen(options =>
{
    options.UseOneOfForPolymorphism();
    options.UseAllOfForInheritance();

    options.SwaggerDoc("v1", new() { Title = "Composite V1", Version = "v1" });
    options.SwaggerDoc("v2", new() { Title = "Composite V2", Version = "v2" });

    options.OperationFilter<SwaggerDefaultValues>();
});

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.WriteIndented = true;
    options.SerializerOptions.TypeInfoResolver = new PolymorphicTypeResolver();
});

services.AddProblemDetails(options => options.CustomizeProblemDetails = ctx =>
{
    ctx.ProblemDetails.Extensions.Add("trace-id", ctx.HttpContext.TraceIdentifier);
    ctx.ProblemDetails.Instance = $"{ctx.HttpContext.Request.Method} {ctx.HttpContext.Request.Path}";
});

var app = builder.Build();

app.UseStatusCodePages();
app.UseExceptionHandler();

var versionSet = app
    .NewApiVersionSet()
    .ReportApiVersions()
    .HasApiVersion(new(1.0))
    .HasApiVersion(new(2.0))
    .Build();

app.MapBasedOnAttribute(versionSet);
app.MapCompositeBasedOnTypeResolver(versionSet);

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    foreach (var description in app.DescribeApiVersions())
    {
        options.InjectStylesheet(
            "https://cdn.jsdelivr.net/gh/ajatkj/swagger-ui-improved-theme/css/swagger-ui-improved.css");

        var url = $"/swagger/{description.GroupName}/swagger.json";
        var name = description.GroupName.ToUpperInvariant();
        options.SwaggerEndpoint(url, name);
    }
});

app.Run();
