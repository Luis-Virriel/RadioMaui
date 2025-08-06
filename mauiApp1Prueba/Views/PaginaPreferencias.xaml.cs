using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System;

namespace mauiApp1Prueba.Views
{
    public partial class PaginaPreferencias : ContentPage
    {
        public PaginaPreferencias()
        {
            InitializeComponent();
            CargarPreferencias();
        }

        private void CargarPreferencias()
        {
            NoticiasCheck.IsChecked = Preferences.Get("MostrarNoticias", true);
            CineCheck.IsChecked = Preferences.Get("MostrarCine", true);
            ClimaCheck.IsChecked = Preferences.Get("MostrarClima", true);
            CotizacionesCheck.IsChecked = Preferences.Get("MostrarCotizaciones", true);
            PatrocinadoresCheck.IsChecked = Preferences.Get("MostrarPatrocinadores", true);
        }

        private async void OnGuardarClicked(object sender, EventArgs e)
        {
            Preferences.Set("MostrarNoticias", NoticiasCheck.IsChecked);
            Preferences.Set("MostrarCine", CineCheck.IsChecked);
            Preferences.Set("MostrarClima", ClimaCheck.IsChecked);
            Preferences.Set("MostrarCotizaciones", CotizacionesCheck.IsChecked);
            Preferences.Set("MostrarPatrocinadores", PatrocinadoresCheck.IsChecked);

            await DisplayAlert("Guardado", "Tus preferencias fueron guardadas.", "OK");

            if (Shell.Current is AppShell shell)
            {
                shell.ConstruirMenu();
            }
        }
    }
}
