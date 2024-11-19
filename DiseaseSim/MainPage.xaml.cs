using Microsoft.Maui.Controls;
using DiseaseSim.Data; 
using System.Timers;
using Timer = System.Timers.Timer;
using Location = DiseaseSim.Data.Location;

namespace DiseaseSim
{
    public partial class MainPage : ContentPage
    {
        //private Timer timer;
        private int _currentHour;
        private int _simulationDuration;
        private double _infectionChance;
        private double _deathChance;
        private int _diseaseDuration;
        private int _quarantineDuration;
        private double _travelChance;
        private double _meanPopulation;
        private double _stdDevPopulation;
        private double _meanQuarantineChance;
        private double _stdDevQuarantineChance;

        private List<Location> _locations;
        private Random _random;

        public MainPage()
        {
            InitializeComponent();
            _locations = new List<Location>();
            _random = new Random();
        }

        private async void OnLoadFileButtonClicked(object sender, EventArgs e)
        {
            try
            {
                // Prompt user to select a file
                var result = await FilePicker.Default.PickAsync();
                if (result != null)
                {
                    string filePath = result.FullPath;

                    // Load configuration and initialize simulation
                    LoadConfiguration(filePath);
                    InitializeLocations();

                    // Start simulation timer
                    //timer.Start();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load file: {ex.Message}", "OK");
            }
        }

        private void LoadConfiguration(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                throw new FileNotFoundException("Configuration file not found.");

            var lines = File.ReadAllLines(filePath);

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();

                // Skip comments or empty lines
                if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith("#"))
                    continue;

                // Handle configuration key-value pairs
                if (trimmedLine.Contains("="))
                {
                    var parts = trimmedLine.Split('=');
                    if (parts.Length != 2)
                        throw new FormatException($"Invalid configuration line: {line}");

                    var key = parts[0].Trim();
                    var value = parts[1].Trim();

                    switch (key)
                    {
                        case "SimulationDuration":
                            _simulationDuration = int.Parse(value); // Ensure value is an integer
                            break;
                        case "InfectionChance":
                            _infectionChance = double.Parse(value); // Ensure value is a double
                            break;
                        case "DeathChance":
                            _deathChance = double.Parse(value);
                            break;
                        case "DiseaseDuration":
                            _diseaseDuration = int.Parse(value);
                            break;
                        case "QuarantineDuration":
                            _quarantineDuration = int.Parse(value);
                            break;
                        case "TravelChance":
                            _travelChance = double.Parse(value);
                            break;
                        case "MeanPopulation":
                            _meanPopulation = double.Parse(value);
                            break;
                        case "StdDevPopulation":
                            _stdDevPopulation = double.Parse(value);
                            break;
                        case "MeanQuarantineChance":
                            _meanQuarantineChance = double.Parse(value);
                            break;
                        case "StdDevQuarantineChance":
                            _stdDevQuarantineChance = double.Parse(value);
                            break;
                        default:
                            throw new FormatException($"Unknown configuration key: {key}");
                    }
                }
                // Handle location definitions
                else if (trimmedLine.Contains(","))
                {
                    var locationParts = trimmedLine.Split(',');
                    if (locationParts.Length != 3)
                        throw new FormatException($"Invalid location line: {line}");

                    var locationId = locationParts[0].Trim();
                    var population = int.Parse(locationParts[1].Trim()); // Ensure value is an integer
                    var travelWindow = int.Parse(locationParts[2].Trim());

                    var location = new Location(locationId);
                    for (int i = 0; i < population; i++)
                    {
                        var person = new Person(
                            Guid.NewGuid().ToString(),
                            _random.Next(0, 24),
                            _random.Next(travelWindow, 24),
                            Math.Clamp(_random.NextDouble(), 0.0, 1.0));
                        location.People.Add(person);
                    }
                    _locations.Add(location);
                }
            }
        }

        private void InitializeLocations()
        {
            // Connect neighbors for travel simulation
            foreach (var location in _locations)
            {
                foreach (var neighbor in _locations.Where(l => l != location))
                {
                    location.Neighbors.Add(neighbor);
                }
            }
        }

        private async void OnSimulationTick(object sender, EventArgs e)
        {
            _currentHour++;

            foreach (var location in _locations)
            {
                // Spread infection
                location.SimulateDiseaseSpread(_infectionChance);

                // Handle travel
                location.HandleTravel(_currentHour, _random);
            }

            // Calculate statistics
            var aliveCount = _locations.Sum(l => l.People.Count(p => !p.IsDead));
            var deadCount = _locations.Sum(l => l.People.Count(p => p.IsDead));
            var infectedCount = _locations.Sum(l => l.People.Count(p => p.IsInfected));
            var quarantinedCount = _locations.Sum(l => l.People.Count(p => p.IsQuarantined));
            var topInfected = _locations.SelectMany(l => l.People)
                                        .Where(p => p.IsInfected)
                                        .OrderByDescending(p => p.InfectionCount)
                                        .FirstOrDefault();
            var topSpreader = _locations.SelectMany(l => l.People)
                                        .OrderByDescending(p => p.InfectionSpreadCount)
                                        .FirstOrDefault();

            // Update UI on the main thread
            Dispatcher.Dispatch(() =>
            {
                HourLabel.Text = $"Current Hour: {_currentHour}";
                AliveCountLabel.Text = $"Alive Count: {aliveCount}";
                DeadCountLabel.Text = $"Dead Count: {deadCount}";
                InfectedCountLabel.Text = $"Infected Count: {infectedCount}";
                QuarantinedCountLabel.Text = $"Quarantined Count: {quarantinedCount}";
                TopInfectedLabel.Text = $"Top Infected Person: {topInfected?.Id ?? "N/A"}";
                TopSpreaderLabel.Text = $"Top Spreader Person: {topSpreader?.Id ?? "N/A"}";
            });

        }
    }
}
