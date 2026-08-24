using CorexBrowser.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CorexBrowser
{
    public partial class SettingsWindow : Window, INotifyPropertyChanged
    {
        private SettingsSection _currentSection = SettingsSection.General;
        public SettingsSection CurrentSection
        {
            get => _currentSection;
            set
            {
                _currentSection = value;
                OnPropertyChanged();
            }
        }

        public SettingsWindow()
        {

            InitializeComponent();
            DataContext = this;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            HomePageTextBox.Text = SettingsService.Current.HomePage;

            foreach (ComboBoxItem item in SearchEngineComboBox.Items)
            {
                if ((string)item.Tag == SettingsService.Current.SearchEngine)
                {
                    SearchEngineComboBox.SelectedItem = item;
                    break;
                }
            }
        }
        private void HomePageTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var text = HomePageTextBox.Text.Trim();

            if (!string.IsNullOrWhiteSpace(text))
            {
                SettingsService.Current.HomePage = text;
                SettingsService.Save();
            }
        }


        private void SearchEngineComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SearchEngineComboBox.SelectedItem is ComboBoxItem item)
            {
                var engine = (string)item.Tag;
                SettingsService.Current.SearchEngine = engine;

                // 🔁 İSTEĞE BAĞLI: HomePage’i de eşle
                SettingsService.Current.HomePage = engine switch
                {
                    "bing" => "https://www.bing.com",
                    "duckduckgo" => "https://duckduckgo.com",
                    _ => "https://www.google.com"
                };

                SettingsService.Save();
            }

        }


        public enum SettingsSection
        {
            General,
            Privacy,
            About
        }


        private void NavGeneral_Click(object sender, MouseButtonEventArgs e)
        {
            CurrentSection = SettingsSection.General;
            DataContext = null;
            DataContext = this;
        }

        private void NavPrivacy_Click(object sender, MouseButtonEventArgs e)
        {
            CurrentSection = SettingsSection.Privacy;
            DataContext = null;
            DataContext = this;
        }

        private void NavAbout_Click(object sender, MouseButtonEventArgs e)
        {
            CurrentSection = SettingsSection.About;
            DataContext = null;
            DataContext = this;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void Navigate(SettingsSection section)
        {
            CurrentSection = section;
        }

        

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
