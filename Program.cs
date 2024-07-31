using Neo4j.Driver;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.




builder.Services.AddControllers();



builder.Services.AddSingleton(GraphDatabase.Driver(
            Environment.GetEnvironmentVariable("NEO4J_URI") ?? "neo4j+s://demo.neo4jlabs.com",
            AuthTokens.Basic(
                Environment.GetEnvironmentVariable("NEO4J_USER") ?? "movies",
                Environment.GetEnvironmentVariable("NEO4J_PASSWORD") ?? "movies"
            )
        ));
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(x => x
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
