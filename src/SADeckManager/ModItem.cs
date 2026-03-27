using System.ComponentModel;

namespace SADeckManager;

public sealed class ModItem : INotifyPropertyChanged
    {
        private bool _isEnabled;

        public string Id { get; init; } = string.Empty;
        public string Label { get; init; } = string.Empty;

        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (_isEnabled == value) return;
                _isEnabled = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsEnabled)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }