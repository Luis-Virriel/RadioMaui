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

            // Iniciar con LoginPage
            var loginPage = _serviceProvider.GetRequiredService<LoginPage>();
            MainPage = new NavigationPage(loginPage);
        }
    }
}