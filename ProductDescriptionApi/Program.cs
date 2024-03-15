using System.Net.Http.Headers;
using ProductDescriptionApi.Services;

// Create a WebApplicationBuilder object
// to configure the how the ASP.NET service runs
// **********************************************
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Add and Configure Services
// **********************************************


// Configure ASP.NET to use the Controller model
builder.Services.AddControllers();

// Configure Open API (Swagger)
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register OpenAIService with HttpClient and API Key dependency
builder.Services.AddHttpClient<OpenAIService>();
builder.Services.AddSingleton<CsvHandler>();
// builder.Services.AddScoped<CsvHandler>();

builder.Services.Configure<OpenAIServiceOptions>(builder.Configuration.GetSection("OpenAI"));


    // Other client configuration if necessary




// **********************************************
// After adding and configuring services
// Create an instance of a WebApplication object
// **********************************************
var app = builder.Build();

// Configure the HTTP request pipeline.
// ***********************************

// When in Development mode
// Enable the Open API (Swagger) page
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Enable Authorization Middleware
app.UseAuthorization();

// Enable the endpoints of Controller Action Methods
app.MapControllers();

// Run the Application
app.Run();
