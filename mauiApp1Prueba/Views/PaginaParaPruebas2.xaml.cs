namespace mauiApp1Prueba;

public partial class PaginaParaPruebas2 : ContentPage
{
	public PaginaParaPruebas2()
	{
		InitializeComponent();
	}

    private void btnIrAClima_Clicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new PaginaClima());
    }

    private void btnIrACotizaciones_Clicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new PaginaCotizaciones());
    }

    private void btnIrANoticias_Clicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new PaginaNoticias());
    }

    private void btnIrACine_Clicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new PaginaCine());
    }

    private void btnIrAPatrocinadores_Clicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new PaginaPatrocinadores());
    }

    private void btnIrAPreferencias_Clicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new PaginaPreferencias());
    }
}