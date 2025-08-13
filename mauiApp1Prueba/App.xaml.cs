using mauiApp1Prueba.Views;

namespace mauiApp1Prueba
{
    public partial class App : Application
    {
        private readonly IServiceProvider _serviceProvider;

        public App(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;

            // Registrar rutas para páginas que usan inyección de dependencias
            Routing.RegisterRoute("patrocinadores", typeof(PaginaPatrocinadores));
            Routing.RegisterRoute("sponsordetail", typeof(SponsorDetailPage));
            Routing.RegisterRoute("locationpicker", typeof(LocationPickerPage));

            // Inicia con LoginPage fuera del Shell
            MainPage = new NavigationPage(_serviceProvider.GetRequiredService<LoginPage>());
        }

        // Método público para cambiar a AppShell después del login
        public void IniciarShell()
        {
            MainPage = _serviceProvider.GetRequiredService<AppShell>();
        }
    }
}