using Autofac.Extensions.DependencyInjection;
using Autofac;
using FileFlowServer.Configuration;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost",
        builder => builder
            .WithOrigins("http://localhost:5173") // Add the client origin
            .AllowAnyMethod()
            .AllowAnyHeader()
    );
});
builder.Services.AddControllers()
                .AddJsonOptions(x => x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

// Configure Autofac
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
    string connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    containerBuilder.RegisterModule(new ServiceAutofacModule(connectionString));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.UseCors("AllowLocalhost");
app.MapControllers();
app.Run();
