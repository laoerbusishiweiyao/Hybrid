using System.Text;
using System.Text.Json.Serialization;
using Asp.Versioning;
using EdgeServer;
using EdgeServer.Filters;
using EdgeServer.Middleware;
using EdgeServer.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Scalar.AspNetCore;
using Serilog;

Directory.SetCurrentDirectory(AppContext.BaseDirectory);
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSerilog(configuration => { configuration.ReadFrom.Configuration(builder.Configuration); });
builder.Services.Configure<AppSettings>(builder.Configuration);
builder.Services.AddOpenApi();
builder.Services.AddApiVersioning(options =>
{
    options.ReportApiVersions = true;
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        // /api/v1/user
        new UrlSegmentApiVersionReader(),
        // /api/user?version=1
        new QueryStringApiVersionReader("version"),
        new HeaderApiVersionReader("X-API-Version"),
        // application/json;version=2
        new MediaTypeApiVersionReader("version")
    );
});
builder.Services.AddControllers(options => options.Filters.Add<ApiResponseResultFilter>())
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .SelectMany(pair => pair.Value?.Errors ?? [])
                .Select(error => error.ErrorMessage);
            return new BadRequestObjectResult(new ApiResponse
            {
                StatusCode = 400,
                Message = "验证失败",
                Payload = errors
            });
        };
    });
// builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

Log.Information("进程Id: {path}", Environment.ProcessId);

app.UseGlobalExceptionHandling();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options => options.Title = "D26_001");

    app.UseExceptionHandler("/exception");
}

app.UseSerilogRequestLogging();

#region FileServer - Bucket

{
    var physicalPath = app.Configuration["FileServer:BucketPath"] ?? throw new Exception("FileServer:BucketPath is null");
    Log.Information("bucket path: {path}", physicalPath);
    var bucketPath = Path.Combine(builder.Environment.ContentRootPath, physicalPath);
    if (!Directory.Exists(bucketPath))
    {
        Directory.CreateDirectory(bucketPath);
    }

    var options = new FileServerOptions
    {
        RequestPath = new PathString("/bucket"),
        FileProvider = new PhysicalFileProvider(bucketPath),
        EnableDirectoryBrowsing = app.Environment.IsDevelopment(),
        StaticFileOptions =
        {
            ContentTypeProvider = new FileExtensionContentTypeProvider()
        },
    };
    app.UseFileServer(options);
}

#endregion

#region FileServer - Vue

{
    var physicalPath = app.Configuration["FileServer:VuePath"] ?? throw new Exception("FileServer:VuePath is null");
    Log.Information("vue path: {path}", physicalPath);
    var vuePath = Path.Combine(builder.Environment.ContentRootPath, physicalPath);
    if (!Directory.Exists(vuePath))
    {
        Directory.CreateDirectory(vuePath);
    }

    var options = new FileServerOptions
    {
        RequestPath = new PathString(string.Empty),
        FileProvider = new PhysicalFileProvider(vuePath),
        EnableDirectoryBrowsing = app.Environment.IsDevelopment(),
        StaticFileOptions =
        {
            ContentTypeProvider = new FileExtensionContentTypeProvider()
        },
        EnableDefaultFiles = true,
        DefaultFilesOptions =
        {
            DefaultFileNames = ["index.html"],
        }
    };
    app.UseFileServer(options);
}

#endregion

#region Static

{
    var physicalPath = app.Configuration["StaticPath"] ?? throw new Exception("StaticPath is null");
    Log.Information("Static path: {path}", physicalPath);
    var staticPath = Path.Combine(builder.Environment.ContentRootPath, physicalPath);
    if (!Directory.Exists(staticPath))
    {
        Directory.CreateDirectory(staticPath);
    }

    var contentTypeProvider = new FileExtensionContentTypeProvider
    {
        Mappings =
        {
            [".txt"] = "text/plain; charset=utf-8"
        }
    };
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(Path.Combine(builder.Environment.ContentRootPath, physicalPath)),
        RequestPath = new PathString("/static"),
        ContentTypeProvider = contentTypeProvider,
        ServeUnknownFileTypes = true,
        DefaultContentType = "application/octet-stream",
    });
}

#endregion

#region cdn

{
    var physicalPath = "cdn";
    Log.Information("Cdn path: {path}", physicalPath);
    var cdnPath = Path.Combine(builder.Environment.ContentRootPath, physicalPath);
    if (!Directory.Exists(cdnPath))
    {
        Directory.CreateDirectory(cdnPath);
    }

    var contentTypeProvider = new FileExtensionContentTypeProvider();
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(Path.Combine(builder.Environment.ContentRootPath, physicalPath)),
        RequestPath = new PathString("/cdn"),
        ContentTypeProvider = contentTypeProvider,
        ServeUnknownFileTypes = true,
        DefaultContentType = "application/octet-stream",
    });
}

#endregion

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// app.MapReverseProxy();

app.Run();