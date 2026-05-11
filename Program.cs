using Microsoft.EntityFrameworkCore;
using entornoPolleria;
using System.Globalization;
using Microsoft.AspNetCore.Localization; // 1. Asegúrate de tener esta línea

using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

// --- PASO 1: REGISTRAR LAS REGLAS DE CULTURA ---
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    // Usamos "en-US" porque es una cultura que siempre usa '.' para los decimales.
    var supportedCultures = new[] { new CultureInfo("en-US") }; 
    
    // Establecemos esta cultura como la predeterminada para todas las peticiones.
    options.DefaultRequestCulture = new RequestCulture("en-US");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});
// --- FIN DEL PASO 1 ---

// Add services to the container.
builder.Services.AddDataProtection()
    .SetApplicationName("PolleriaGestion");

builder.Services.AddAntiforgery(options =>
{
    options.Cookie.Name = ".PolleriaGestion.Antiforgery";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.None;
});

builder.Services.AddControllersWithViews();

var conectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(conectionString));

builder.Services.AddScoped<IProveedoresRepository, ProveedoresRepository>();
builder.Services.AddScoped<IProductosRepository, ProductosRepository>();
builder.Services.AddScoped<IPromocionesRepository, PromocionesRepository>();
builder.Services.AddScoped<IVentaRepository, VentaRepository>();
builder.Services.AddScoped<IMetodosPagoRepository, MetodosPagoRepository>();
builder.Services.AddScoped<IComprasRepository, ComprasRepository>();
builder.Services.AddScoped<IConsumoRepository, ConsumoRepository>();
builder.Services.AddScoped<IProduccionRepository, ProduccionRepository>();
builder.Services.AddScoped<IGastosRepository, GastosRepository>();
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// --- PASO 2: APLICAR LAS REGLAS DE CULTURA (¡EL PASO CLAVE!) ---
// Esta línea activa la configuración que definimos arriba para cada petición.
app.UseRequestLocalization(); 
// --- FIN DEL PASO 2 ---

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Ventas}/{action=Alta}");
try
{
    // Solo intenta abrir el navegador si no es un entorno de desarrollo o si explícitamente se desea
    Console.WriteLine("Iniciando sistema en http://localhost:5146...");
    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("http://localhost:5146") { UseShellExecute = true });
}
catch (Exception ex)
{
    Console.WriteLine("No se pudo abrir el navegador automáticamente: " + ex.Message);
}

app.Run("http://localhost:5146");
