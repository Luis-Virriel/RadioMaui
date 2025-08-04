using Plugin.Fingerprint;
using Plugin.Fingerprint.Abstractions;
using System.Threading.Tasks;

namespace mauiApp1Prueba
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
        }

        private void OnCounterClicked(object sender, EventArgs e)
        {
            count++;

            if (count == 1)
                CounterBtn.Text = $"Clicked {count} time";
            else
                CounterBtn.Text = $"Clicked {count} times";

            SemanticScreenReader.Announce(CounterBtn.Text);
        }

        private void CounterBtn2_Clicked(object sender, EventArgs e)
        {
            Persona  newPersona = new Persona();
            newPersona.Nombre = "Fran";
            newPersona.Apellido = "Perez";
            newPersona.Edad = 21;
            Navigation.PushAsync(new pagina1prueba(newPersona));
        }

        private async void btnHuella_Clicked(object sender, EventArgs e)
        {
            var request = new AuthenticationRequestConfiguration("obligatorio", "para probar huella");
            var result = await CrossFingerprint.Current.AuthenticateAsync(request);
            if (result.Authenticated)
            {
                btnFotoVideo.Background = new Color(100, 280, 400);
            }
            else {
                await DisplayAlert("Error de autenticacion", "tiene huella", "cerrar");
            }
        }

        private async void btnFotoVideo_Clicked(object sender, EventArgs e)
        {
            //para filtrar en que dispositivos quiero que se vea algo
            if (DeviceInfo.Current.Platform == DevicePlatform.Android) 
            {
            
            }
            //Preferencias
            try 
            {
                var foto = await MediaPicker.CapturePhotoAsync();
                if (foto != null)
                {
                    var stream = await foto.OpenReadAsync();
                    await DisplayAlert("GENIAL", "FOTO RECIBIDA", "OK");
                }
            }
            catch(Exception ex)
            {
                DisplayAlert("ERROR", "ERROR AL ABRIR LA CAMARA", "CERRAR");

            }
        }

        private async void btnGPS_Clicked(object sender, EventArgs e)
        {
            try 
            {
            var location = await Geolocation.GetLastKnownLocationAsync();
                if (location != null) 
                {
                    await DisplayAlert("Localizacion:", "Estoy en latitud: " + location.Latitude+ "y longitud: "+ location.Longitude, "CERRAR" );
                }
            } 
            catch(Exception ex) 
            {
                DisplayAlert("ERROR", "ERROR AL ABRIR LA UBICACION", "CERRAR");
            }
        }

        private void btnirAPruebas2_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new PaginaParaPruebas2());
        }
    }

}
