using HtTemplate.Configuration;
using Issues.Api.Catalog;
using Issues.Api.Vendors;
using Marten;

var builder = WebApplication.CreateBuilder(args);

builder.AddCustomFeatureManagement();

builder.Services.AddCustomServices();
builder.Services.AddCustomOasGeneration();

builder.Services.AddControllers();

builder.Services.AddScoped<VendorData>();
builder.Services.AddScoped<ILookupVendors, VendorData>();

var connectionString = builder.Configuration.GetConnectionString("issues") ?? throw new Exception("No Connection String in Environment");
builder.Services.AddMarten(options =>
{
    options.Connection(connectionString);
}).UseLightweightSessions();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();