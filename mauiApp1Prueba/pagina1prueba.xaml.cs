namespace mauiApp1Prueba;

public partial class pagina1prueba : ContentPage
{
	public pagina1prueba(Persona miPersona)
	{
		InitializeComponent();
		CounterBtn3.Text = miPersona.Nombre;
	}

    private void CounterBtn3_Clicked(object sender, EventArgs e)
    {
		Navigation.PopAsync();
    }
}