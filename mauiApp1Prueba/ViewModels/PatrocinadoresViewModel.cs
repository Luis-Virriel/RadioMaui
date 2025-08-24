using mauiApp1Prueba.Models;
using mauiApp1Prueba.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace mauiApp1Prueba.ViewModels
{
    public class PatrocinadoresViewModel : BaseViewModel
    {
        private readonly ISponsorService _sponsorService;
        private ObservableCollection<Sponsor> _sponsors = new();
        private Sponsor? _selectedSponsor;
        private string _searchText = string.Empty;
        private bool _isEmptyStateVisible;

        public ObservableCollection<Sponsor> Sponsors
        {
            get => _sponsors;
            set => SetProperty(ref _sponsors, value);
        }

        public Sponsor? SelectedSponsor
        {
            get => _selectedSponsor;
            set => SetProperty(ref _selectedSponsor, value);
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                    OnSearchTextChanged(value);
            }
        }

        public bool IsEmptyStateVisible
        {
            get => _isEmptyStateVisible;
            set => SetProperty(ref _isEmptyStateVisible, value);
        }

        // Commands
        public ICommand LoadSponsorsCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand SelectSponsorCommand { get; }
        public ICommand AddSponsorCommand { get; }
        public ICommand DeleteSponsorCommand { get; }
        public ICommand SearchSponsorsCommand { get; }
        public ICommand ViewMapCommand { get; }

        public PatrocinadoresViewModel(ISponsorService sponsorService)
        {
            _sponsorService = sponsorService;
            PageTitle = "Patrocinadores";

            // Inicializar comandos
            LoadSponsorsCommand = new Command(async () => await LoadSponsorsAsync());
            RefreshCommand = new Command(async () => await RefreshAsync());
            SelectSponsorCommand = new Command<Sponsor>(async (sponsor) => await SelectSponsorAsync(sponsor));
            AddSponsorCommand = new Command(async () => await AddSponsorAsync());
            DeleteSponsorCommand = new Command<Sponsor>(async (sponsor) => await DeleteSponsorAsync(sponsor));
            SearchSponsorsCommand = new Command(async () => await SearchSponsorsAsync());

            // Comando para abrir mapa
            // Nuevo: abrir página de mapa
            ViewMapCommand = new Command(async () =>
            {
                try
                {
                    // Navegar usando la ruta registrada en AppShell
                    await Shell.Current.GoToAsync("SponsorsMapPage");
                }
                catch (Exception ex)
                {
                    await Application.Current.MainPage.DisplayAlert("Error",
                        $"No se pudo abrir el mapa: {ex.Message}", "OK");
                }
            });

        }

        public override async Task InitializeAsync()
        {
            await LoadSponsorsAsync();
        }

        private async Task LoadSponsorsAsync()
        {
            if (IsBusy) return;

            try
            {
                SetBusyState(true, "Cargando patrocinadores...");

                var sponsors = await _sponsorService.GetAllSponsorsAsync();

                Sponsors.Clear();
                foreach (var sponsor in sponsors)
                    Sponsors.Add(sponsor);

                IsEmptyStateVisible = !Sponsors.Any();
            }
            catch (Exception ex)
            {
                await Application.Current?.MainPage?.DisplayAlert("Error",
                    $"No se pudieron cargar los patrocinadores: {ex.Message}", "OK");
            }
            finally
            {
                SetBusyState(false);
                IsRefreshing = false;
            }
        }

        private async Task RefreshAsync()
        {
            IsRefreshing = true;
            await LoadSponsorsAsync();
        }

        private async Task SelectSponsorAsync(Sponsor sponsor)
        {
            if (sponsor == null) return;

            SelectedSponsor = sponsor;

            await Shell.Current.GoToAsync($"sponsordetail?id={sponsor.Id}");
        }

        private async Task AddSponsorAsync()
        {
            await Shell.Current.GoToAsync("sponsordetail");
        }

        private async Task DeleteSponsorAsync(Sponsor sponsor)
        {
            if (sponsor == null) return;

            var result = await Application.Current?.MainPage?.DisplayAlert(
                "Confirmar eliminación",
                $"¿Estás seguro de que deseas eliminar el patrocinador '{sponsor.Name}'?",
                "Eliminar", "Cancelar");

            if (result == true)
            {
                try
                {
                    SetBusyState(true, "Eliminando patrocinador...");

                    var deleted = await _sponsorService.DeleteSponsorAsync(sponsor.Id);
                    if (deleted)
                    {
                        Sponsors.Remove(sponsor);
                        IsEmptyStateVisible = !Sponsors.Any();

                        await Application.Current?.MainPage?.DisplayAlert("Éxito",
                            "Patrocinador eliminado correctamente", "OK");
                    }
                    else
                    {
                        await Application.Current?.MainPage?.DisplayAlert("Error",
                            "No se pudo eliminar el patrocinador", "OK");
                    }
                }
                catch (Exception ex)
                {
                    await Application.Current?.MainPage?.DisplayAlert("Error",
                        $"Error al eliminar el patrocinador: {ex.Message}", "OK");
                }
                finally
                {
                    SetBusyState(false);
                }
            }
        }

        private async Task SearchSponsorsAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                await LoadSponsorsAsync();
                return;
            }

            try
            {
                SetBusyState(true, "Buscando...");

                var allSponsors = await _sponsorService.GetAllSponsorsAsync();
                var filtered = allSponsors.Where(s =>
                    s.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    (!string.IsNullOrEmpty(s.Description) && s.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                    s.Address.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                ).ToList();

                Sponsors.Clear();
                foreach (var sponsor in filtered)
                    Sponsors.Add(sponsor);

                IsEmptyStateVisible = !Sponsors.Any();
            }
            catch (Exception ex)
            {
                await Application.Current?.MainPage?.DisplayAlert("Error",
                    $"Error en la búsqueda: {ex.Message}", "OK");
            }
            finally
            {
                SetBusyState(false);
            }
        }

        private void OnSearchTextChanged(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                _ = LoadSponsorsAsync();
        }
    }
}
