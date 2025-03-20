using APIProject.API.Extensions;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddApplicationServices(builder.Configuration);

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

//await SeedData.InicializarBancoDeDados(app);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Project v1");
        c.RoutePrefix = string.Empty;
    });
}

// Adicionar middleware de exceção
app.UseExceptionMiddleware();

app.UseHttpsRedirection();

// Adicionar middleware de autenticação
app.UseAuthentication();
app.UseAuthorization();

// Mapear os controladores
app.MapControllers();



app.Run();
