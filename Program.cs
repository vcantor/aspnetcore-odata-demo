using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Batch;
using ODataDemo.Controllers;
using ODataDemo.Leaders;
using Swashbuckle.AspNetCore.Swagger;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpContextAccessor();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddSingleton
    <ISwaggerProvider, SwaggerProvider>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
}).AddOData((opt) =>
{
    opt.AddDemoApiV1Model();
});
var app = builder.Build();

app.UseODataBatching();
app.UseRouting();
//app.UseODataQueryRequest();
//app.UseODataRouteDebug();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
