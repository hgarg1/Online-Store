using Online_Store.Filters;
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;
using Microsoft.Extensions.Azure;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<AuthFilter>();
});
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ApiFilter>();
});
builder.Services.AddCors();
builder.Services.AddMvc();
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(5);
    options.Cookie.HttpOnly = true;
});
builder.Services.AddAzureClients(clientBuilder =>
{
    clientBuilder.AddBlobServiceClient(builder.Configuration["Storage:AZBlob:blob"], preferMsi: true);
    clientBuilder.AddQueueServiceClient(builder.Configuration["Storage:AZBlob:queue"], preferMsi: true);
});

var app = builder.Build();
app.UseHsts();
app.UseStaticFiles();
app.MapControllers();
app.UseRouting();
app.UseSession();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapRazorPages();
});
app.Run();
