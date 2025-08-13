using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace mauiApp1Prueba.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        private bool _isBusy;
        private string _pageTitle = string.Empty;
        private bool _isRefreshing;

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public string PageTitle
        {
            get => _pageTitle;
            set => SetProperty(ref _pageTitle, value);
        }

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set => SetProperty(ref _isRefreshing, value);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        public virtual Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        protected virtual void OnIsBusyChanged()
        {
            // Override in derived classes if needed
        }

        protected void SetBusyState(bool isBusy, string? loadingMessage = null)
        {
            IsBusy = isBusy;
            if (!string.IsNullOrEmpty(loadingMessage) && isBusy)
            {
                // Aquí podrías mostrar un mensaje de carga si tienes un sistema de notificaciones
            }
        }
    }
}