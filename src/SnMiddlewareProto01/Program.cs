using SenseNet.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// [sensenet]
builder.Services.AddSenseNetClient()
    .ConfigureSenseNetRepository(repositoryOptions =>
    {
        // Load configuration from a path of your choice.
        // This configuration should contain at least the repository url
        // and optionally authentication values.
        builder.Configuration.GetSection("sensenet:repository").Bind(repositoryOptions);
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
