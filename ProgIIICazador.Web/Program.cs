using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Microsoft.EntityFrameworkCore;
using ProgIIICazador.Web.Data;
using ProgIIICazador.Web.Modules.CapturaOfertas;
using ProgIIICazador.Web.Modules.GestionOfertas;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<OfertasDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("OfertasDb")));
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
{
    options.LoginPath = "/Admin/Login"; options.AccessDeniedPath = "/Admin/Login";
    options.Cookie.Name = "Cazador.Admin"; options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; options.Cookie.SameSite = SameSiteMode.Lax;
    options.SlidingExpiration = true; options.ExpireTimeSpan = TimeSpan.FromHours(4);
});
builder.Services.AddAuthorization();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("login", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(5),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
});
builder.Services.AddCapturaOfertasModule(builder.Configuration);
builder.Services.AddProcesamientoOfertasModule();
var app = builder.Build();
if (!app.Environment.IsDevelopment()) { app.UseExceptionHandler("/Home/Error"); app.UseHsts(); }
app.UseHttpsRedirection(); app.UseStaticFiles(); app.UseRouting(); app.UseRateLimiter(); app.UseAuthentication(); app.UseAuthorization();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OfertasDbContext>();
    await db.Database.MigrateAsync();
}
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();