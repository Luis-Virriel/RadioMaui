using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using mauiApp1Prueba.Views;

namespace mauiApp1Prueba;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Registrar rutas para navegación con Shell
        Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
        Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
        Routing.RegisterRoute(nameof(EditUserPage), typeof(EditUserPage)); // ✅ NUEVA: Registrar ruta EditUserPage

        // 🎬 NUEVA: Registrar ruta para la página de trailer
        Routing.RegisterRoute("trailer", typeof(TrailerPage));

        // Si tienes otras páginas modales/detalle, regístralas aquí también
        // Routing.RegisterRoute("sponsordetail", typeof(SponsorDetailPage));
        // Routing.RegisterRoute("locationpicker", typeof(LocationPickerPage));

        ConstruirMenu();
    }

    public void ConstruirMenu()
    {
        Items.Clear();

        // Home siempre visible (IMPORTANTE: Route debe ser "main" para que funcione GoToAsync("//main"))
        Items.Add(new FlyoutItem
        {
            Title = "Home",
            Route = "main",
            Items =
            {
                new ShellContent
                {
                    ContentTemplate = new DataTemplate(typeof(MainPage)),
                    Route = "main"
                }
            }
        });

        // ✅ NUEVO: Mi Perfil siempre visible
        AgregarFlyoutItem("Mi Perfil", typeof(EditUserPage), "EditUserPage");

        if (Preferences.Get("MostrarNoticias", true))
            AgregarFlyoutItem("Noticias", typeof(PaginaNoticias), "PaginaNoticias");

        if (Preferences.Get("MostrarCine", true))
            AgregarFlyoutItem("Cine", typeof(PaginaCine), "PaginaCine");

        if (Preferences.Get("MostrarClima", true))
            AgregarFlyoutItem("Clima", typeof(PaginaClima), "PaginaClima");

        if (Preferences.Get("MostrarCotizaciones", true))
            AgregarFlyoutItem("Cotizaciones", typeof(PaginaCotizaciones), "PaginaCotizaciones");

        if (Preferences.Get("MostrarPatrocinadores", true))
            AgregarFlyoutItem("Patrocinadores", typeof(PaginaPatrocinadores), "PaginaPatrocinadores");

        // Preferencias siempre visible
        AgregarFlyoutItem("Preferencias", typeof(PaginaPreferencias), "PaginaPreferencias");
    }

    private void AgregarFlyoutItem(string titulo, Type tipoPagina, string ruta)
    {
        Items.Add(new FlyoutItem
        {
            Title = titulo,
            Route = ruta,
            Items =
            {
                new ShellContent
                {
                    ContentTemplate = new DataTemplate(tipoPagina),
                    Route = ruta
                }
            }
        });
    }
}