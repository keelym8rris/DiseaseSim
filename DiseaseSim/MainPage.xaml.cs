using Microsoft.Maui.Controls;
using Project3DiseaseSpreadSimulation.Data;

namespace DiseaseSim
{
    public partial class MainPage : ContentPage
    {
        private Configuration _config;

        public MainPage()
        {
            InitializeComponent();
            _config = new Configuration();
        }

        private async void LoadConfigButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                var result = await FilePicker.Default.PickAsync(new PickOptions
                {
                    PickerTitle = "Select a Configuration File"
                });

                if (result != null)
                {
                    string filePath = result.FullPath;

                    _config.LoadConfiguration(filePath);
                    GameMessage.Text = "Configuration loaded successfully. Click 'Start Simulation' to begin.";
                    StartButton.IsVisible = true;
                }
                else
                {
                    GameMessage.Text = "File selection was canceled.";
                }
            }
            catch (Exception ex)
            {
                GameMessage.Text = $"Error loading configuration: {ex.Message}";
            }
        }

        private void StartButton_Clicked(object sender, EventArgs e)
        {
            // Start the simulation logic with the loaded configuration
            GameMessage.Text = "Simulation started. Processing...";
            // Simulate with _config
        }
    }

}
