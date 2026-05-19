using System.Net.Http;
using CURSO_INTERCULTURALIDAD.Models;
using CURSO_INTERCULTURALIDAD.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromHours(8);
});

// Configuracion para inyectar ApiConfig desde appsettings.json
builder.Services.Configure<ApiConfig>(builder.Configuration.GetSection("ApiConfig"));
builder.Services.Configure<AdminPanelOptions>(builder.Configuration.GetSection("AdminPanel"));
builder.Services.Configure<CorreoConfirmacionOptions>(builder.Configuration.GetSection("CorreoConfirmacion"));
builder.Services.Configure<VerificacionCorreoOptions>(builder.Configuration.GetSection("VerificacionCorreo"));
builder.Services.Configure<PagoIzipayOptions>(builder.Configuration.GetSection("PagoIzipay"));

// Registrar servicios para inyeccion de dependencias
builder.Services.AddScoped<IDniService, DniService>();
builder.Services.AddScoped<ICursoInscripcionRepository, CursoInscripcionRepository>();
builder.Services.AddScoped<ICorreoConfirmacionService, CorreoConfirmacionService>();
builder.Services.AddScoped<IVerificacionCorreoService, VerificacionCorreoService>();
builder.Services.AddScoped<IPagoIzipayService, PagoIzipayService>();
builder.Services.AddHttpClient("PagoIzipay")
    .ConfigurePrimaryHttpMessageHandler(serviceProvider =>
    {
        var configuracion = serviceProvider.GetRequiredService<IConfiguration>();
        var permitirCertificadoInseguro = configuracion.GetValue<bool>("PagoIzipay:PermitirCertificadoInseguro");

        return new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = permitirCertificadoInseguro
                ? (_, _, _, _) => true
                : null
        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseSession();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=InscripcionCIS}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
