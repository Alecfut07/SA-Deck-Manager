using System.ComponentModel;

namespace SADeckManager;

public sealed class ModItem : INotifyPropertyChanged
    {
        private bool _isEnabled;

        /// <summary>
        /// Display / optional; may differ from RelPath.
        /// </summary>
        public string Id { get; init; } = string.Empty;

        /// <summary>
        /// Must match SA Mod Manager <c>EnabledMods</c> / <c>ModsList</c> entries.
        /// </summary>
        public string RelPath { get; init; } = string.Empty;
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