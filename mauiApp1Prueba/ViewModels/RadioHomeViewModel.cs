using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Plugin.Maui.Audio;

namespace mauiApp1Prueba.ViewModels;

public partial class RadioHomeViewModel : ObservableObject
{
    private readonly IAudioManager _audioManager;
    private IAudioPlayer _player;

    [ObservableProperty]
    private bool isPlaying;

    [ObservableProperty]
    private string playPauseIcon = "▶️";

    [ObservableProperty]
    private bool isLive = true;

    [ObservableProperty]
    private string currentShow = "Bienvenido a Radio Punta del Este";

    [ObservableProperty]
    private string currentTime = DateTime.Now.ToString("HH:mm");

    [ObservableProperty]
    private bool isLoading;

    private readonly string[] mp3Files = { "cancion1.mp3", "cancion2.mp3", "cancion3.mp3" };

    public RadioHomeViewModel(IAudioManager audioManager)
    {
        _audioManager = audioManager;
    }

    [RelayCommand]
    private async void PlayPause()
    {
        if (_player != null && _player.IsPlaying)
        {
            _player.Pause();
            PlayPauseIcon = "▶️";
            IsPlaying = false;
        }
        else
        {
            await PlayRandomMp3Async();
        }
    }

    private async Task PlayRandomMp3Async()
    {
        IsLoading = true;

        try
        {
            var random = new Random();
            string chosenFile = mp3Files[random.Next(mp3Files.Length)];

            _player?.Stop();

            using var stream = await FileSystem.OpenAppPackageFileAsync(chosenFile);

            _player = _audioManager.CreatePlayer(stream);
            _player.Play();

            PlayPauseIcon = "⏸️";
            IsPlaying = true;

            // Cambiado para mostrar siempre "Radio del Este"
            CurrentShow = "Radio del Este";
            CurrentTime = DateTime.Now.ToString("HH:mm");
        }
        finally
        {
            IsLoading = false;
        }
    }

}
