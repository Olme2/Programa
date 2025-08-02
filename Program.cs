using Microsoft.EntityFrameworkCore;
using entornoPolleria;
using System.Globalization;
using Microsoft.AspNetCore.Localization; // 1. Asegúrate de tener esta línea

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
builder.Services.AddControllersWithViews();

var conectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(conectionString));

builder.Services.AddScoped<IProveedoresRepository, ProveedoresRepository>();
builder.Services.AddScoped<IProductosRepository, ProductosRepository>();
builder.Services.AddScoped<IPromocionesRepository, PromocionesRepository>();
builder.Services.AddScoped<IVentaRepository, VentaRepository>();
builder.Services.AddScoped<IMetodosPagoRepository, MetodosPagoRepository>();

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
    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("http://localhost:5000") { UseShellExecute = true });
}
catch (Exception ex)
{
    // Opcional: Manejar el caso en que no se pueda abrir el navegador.
    Console.WriteLine("No se pudo abrir el navegador automáticamente: " + ex.Message);
}
app.Run();