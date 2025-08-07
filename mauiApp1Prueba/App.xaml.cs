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
