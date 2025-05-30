using Microsoft.EntityFrameworkCore;
using aspNetCoreMvc.Data;
using aspNetCoreMvc.Models;
using Rotativa.AspNetCore;
using aspNetCoreMvc.Interfaces;
using aspNetCoreMvc.Repository;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<aspNetCoreMvcContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("aspNetCoreMvcContext") ?? throw new InvalidOperationException("Connection string 'aspNetCoreMvcContext' not found.")));

builder.Services.AddScoped<IMovieRepository, MovieRepository>();
// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    SeedData.Initialize(services);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.UseRotativa();

app.Run();
