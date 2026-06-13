using SecureUploadDemo.Helpers;
using SecureUploadDemo.Services;
using SecureUploadDemo.Settings;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<CloudinarySettings>(
    builder.Configuration.GetSection("CloudinarySettings"));

builder.Services.Configure<UploadSettings>(
    builder.Configuration.GetSection("UploadSettings"));

builder.Services.AddSingleton<FileValidator>();
builder.Services.AddSingleton<ICloudinaryService, CloudinaryService>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
