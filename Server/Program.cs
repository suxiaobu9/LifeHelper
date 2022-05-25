using LifeHelper.Server.Extension;
using LifeHelper.Server.Middleware;
using LifeHelper.Shared.Models.AppSettings;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddDbContext<LifeHelperContext>(option => option.UseSqlServer(builder.Configuration["ConnectionStrings:DefaultConnection"]));

builder.Services.Configure<LineChatBotSetting>(builder.Configuration.GetSection("LineChatBot"));
builder.Services.Configure<LIFFSetting>(builder.Configuration.GetSection("LIFF"));

builder.Services.AddServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();

    app.UseHttpsRedirection();
}

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.UseLIFFMiddleware();

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
