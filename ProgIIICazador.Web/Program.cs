using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using ProgIIICazador.Web.Data;
using ProgIIICazador.Web.Services.Captura;
using ProgIIICazador.Web.Services.Procesamiento;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<OfertasDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("OfertasDb")));
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
{
    options.LoginPath = "/Admin/Login";
    options.AccessDeniedPath = "/Admin/Login";
    options.Cookie.Name = "Cazador.Admin";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(4);
});
builder.Services.AddAuthorization();
builder.Services.AddHttpClient<IExtractorFuente, ExtractorWebOficial>(client => client.Timeout = TimeSpan.FromSeconds(25));
builder.Services.AddScoped<ServicioCaptura>();
builder.Services.AddScoped<IProveedorOCR, OcrNoConfigurado>();
builder.Services.AddScoped<IClasificadorOfertas, ClasificadorReglas>();
if (builder.Configuration.GetValue<bool>("Captura:Habilitada"))
    builder.Services.AddHostedService<CapturaProgramadaService>();

var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();