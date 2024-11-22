using DiseaseSim.Data;
using Location = DiseaseSim.Data.Location;

namespace DiseaseSim
{
    public partial class MainPage : ContentPage
    {
        private int currentHour;
        private int infectionChance;
        private double deathChance;
        private int diseaseDuration;
        private int quarantineDuration;
        private double travelChance;
        private double meanPopulation;
        private double stdDevPopulation;
        private double meanQuarantineChance;
        private double stdDevQuarantineChance;

        private int totalInfected;
        private int totalDeaths;

        private List<Location> allLocations;
        private Random randy;

        private string csvFilePath; 

        public MainPage()
        {
            InitializeComponent();
            allLocations = new List<Location>();
            randy = new Random();
            currentHour = 0;
            totalInfected = 0;
            totalDeaths = 0;

            // Initialize the CSV file for hourly data
            csvFilePath = Path.Combine(FileSystem.AppDataDirectory, "DiseaseSimulationLog.csv");
            File.WriteAllText(csvFilePath, "Hour,Alive,Dead,Infected,Quarantined,MostInfectedPerson,TopSpreader\n");
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
                    InitializeInfected();
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
                        case "InfectionChance":
                            infectionChance = int.Parse(value);
                            InfectionChance.Text = $"Infection Chance: {infectionChance}%";
                            break;
                        case "DeathChance":
                            deathChance = double.Parse(value);
                            DeathChance.Text = $"Death Chance: {deathChance * 100}%";
                            break;
                        case "DiseaseDuration":
                            diseaseDuration = int.Parse(value);
                            DiseaseDuration.Text = $"Disease Duration: {diseaseDuration} hours";
                            break;
                        case "QuarantineDuration":
                            quarantineDuration = int.Parse(value);
                            QuarantineDuration.Text = $"Quarantine Duration: {quarantineDuration} hours";
                            break;
                        case "TravelChance":
                            travelChance = double.Parse(value);
                            TravelChance.Text = $"Travel Chance: {travelChance * 100}%";
                            break;
                        case "MeanPopulation":
                            meanPopulation = double.Parse(value);
                            MeanPopulation.Text = $"Mean Population: {meanPopulation}";
                            break;
                        case "StdDevPopulation":
                            stdDevPopulation = double.Parse(value);
                            StdDevPopulation.Text = $"Population Std Dev: {stdDevPopulation}";
                            break;
                        case "MeanQuarantineChance":
                            meanQuarantineChance = double.Parse(value);
                            MeanQuarantineChance.Text = $"Mean Quarantine Chance: {meanQuarantineChance * 100}%";
                            break;
                        case "StdDevQuarantineChance":
                            stdDevQuarantineChance = double.Parse(value);
                            StdDevQuarantineChance.Text = $"Quarantine Std Dev: {stdDevQuarantineChance * 100}%";
                            break;
                        default:
                            throw new FormatException($"Unknown configuration key: {key}");
                    }
                }
                // Handle location information
                else if (trimmedLine.Contains(","))
                {
                    var locationParts = trimmedLine.Split(',');
                    if (locationParts.Length != 3)
                        throw new FormatException($"Invalid location line: {line}");

                    var locationId = locationParts[0].Trim();
                    var population = int.Parse(locationParts[1].Trim());
                    var travelWindow = int.Parse(locationParts[2].Trim());

                    var location = new Location(locationId);
                    for (int i = 0; i < population; i++)
                    {
                        var person = new Person(
                            Guid.NewGuid().ToString(),
                            randy.Next(0, 24),
                            randy.Next(travelWindow, 24),
                            Math.Clamp(randy.NextDouble(), 0.0, 1.0));
                        location.People.Add(person);
                    }
                    allLocations.Add(location);
                }
            }

            // Update the LocationsLabel text
            var locationDetails = allLocations.Select(location =>
                $"{location.Id}: Population {location.People.Count}");

            LocationsLabel.Text = "Locations:\n" + string.Join("\n", locationDetails);

            HideButtonName.IsVisible = true;
        }

        
        /// <summary>
        /// each hour this needs to update all of the numbers and make more people die from the disease 
        /// also call all the functions that change numbers 
        /// ALSO needs to add stuff to csv file each hour 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnHourChange(object sender, EventArgs e)
        {
            // Increment the hour and perform simulation logic (as implemented earlier)
            currentHour++;

            // Simulate disease spread, update stats, etc.
            int newlyInfected = 0;
            int newlyDead = 0;

            foreach (var location in allLocations)
            {
                location.SimulateDiseaseSpread(infectionChance / 100); 
                location.HandleTravel(currentHour, randy); 

                foreach (var person in location.People)
                {
                    if (person.IsInfected && !person.IsDead)
                    {
                        if (randy.NextDouble() < deathChance)
                        {
                            person.IsDead = true;
                            newlyDead++;
                        }
                        else if (!person.IsQuarantined && randy.NextDouble() < person.QuarantineChance)
                        {
                            person.IsQuarantined = true;
                        }
                        else
                        {
                            person.InfectionCount--;
                            if (person.InfectionCount <= 0)
                                person.IsInfected = false;
                        }
                    }

                    if (person.IsInfected && !person.IsDead)
                        newlyInfected++;
                }
            }

            totalInfected += newlyInfected;
            totalDeaths += newlyDead;

            // simulation ends if nothing more meaningful is going to happen 
            if (IsSimulationOver())
            {
                DisplaySimulationEndStats();
                return;
            }

            UpdateStatistics();

        }

        /// <summary>
        /// get locations created and to be diseased 
        /// </summary>
        private void InitializeLocations()
        {
            // Connect neighbors for travel simulation
            foreach (var location in allLocations)
            {
                foreach (var neighbor in allLocations.Where(l => l != location))
                {
                    location.Neighbors.Add(neighbor);
                }
            }
        }

        private void UpdateStatistics()
        {
            // Total population across all locations
            int totalPopulation = allLocations.Sum(loc => loc.People.Count);
            int totalAlive = allLocations.Sum(loc => loc.People.Count(p => !p.IsDead));
            int totalDead = totalDeaths;
            int totalInfected = allLocations.Sum(loc => loc.People.Count(p => p.IsInfected));
            int totalQuarantined = allLocations.Sum(loc => loc.People.Count(p => p.IsQuarantined));

            // Mean and Standard Deviation for Population
            double meanPopulation = allLocations.Average(loc => loc.People.Count);
            double stdDevPopulation = Math.Sqrt(
                allLocations.Sum(loc => Math.Pow(loc.People.Count - meanPopulation, 2)) / allLocations.Count);

            // Percentages
            double infectedPercentage = (double)totalInfected / totalPopulation * 100;
            double deadPercentage = (double)totalDead / totalPopulation * 100;
            double averageInfectedPercentage = allLocations.Average(loc =>
                (double)loc.People.Count(p => p.IsInfected) / loc.People.Count * 100);

            // Average and Maximum Infections Spread
            double averageInfectionSpread = totalInfected == 0
                ? 0
                : allLocations.Sum(loc => loc.People.Sum(p => p.InfectionSpreadCount)) / (double)totalInfected;
            int maxInfectionSpread = allLocations.Max(loc => loc.People.Max(p => p.InfectionSpreadCount));

            // Update Labels
            Hour.Text = $"Current Hour: {currentHour}";
            AliveCount.Text = $"Alive Count: {totalAlive}";
            DeadCount.Text = $"Dead Count: {totalDead}";
            TotalInfectedCount.Text = $"Total Infected Count: {totalInfected}";
            TotalInfectedPercentage.Text = $"Total Infected Percentage: {infectedPercentage:F2}%";
            TotalDeadPercentage.Text = $"Total Dead Percentage: {deadPercentage:F2}%";
            AverageInfectedPercentage.Text = $"Average Infected Percentage: {averageInfectedPercentage:F2}%";
            QuarantinedCount.Text = $"Quarantined Count: {totalQuarantined}";
            AverageInfected.Text = $"Average Person Spread Disease This Many Times: {averageInfectionSpread:F2}";
            TopSpreader.Text = $"Max Number of People Somebody Spread Disease To: {maxInfectionSpread}";

            // Make Labels Visible
            SetLabelsVisibility(true);
        }

        //we need some initial infected people so that it actually spreads 
        private void InitializeInfected()
        {
            foreach (var location in allLocations)
            {
                // Randomly infect some individuals in each location
                int initialInfected = Math.Max(1, (infectionChance / 100) * location.People.Count);
                var infectedPeople = location.People.OrderBy(_ => randy.Next()).Take(initialInfected);

                foreach (var person in infectedPeople)
                {
                    person.IsInfected = true;
                    person.InfectionCount = diseaseDuration; // Set infection duration
                }
            }
        }


        private void SetLabelsVisibility(bool isVisible)
        {
            Hour.IsVisible = isVisible;
            TotalInfectedCount.IsVisible = isVisible;
            DeadCount.IsVisible = isVisible;
            AliveCount.IsVisible = isVisible;
            TotalInfectedPercentage.IsVisible = isVisible;
            TotalDeadPercentage.IsVisible = isVisible;
            AverageInfectedPercentage.IsVisible = isVisible;
            QuarantinedCount.IsVisible = isVisible;
            AverageInfected.IsVisible = isVisible;
            TopSpreader.IsVisible = isVisible;
        }

        private async void HideButton(object sender, EventArgs e)
        {
            SetLabelsInvisibility(false);
        }

        private void SetLabelsInvisibility(bool isVisible)
        {
            InfectionChance.IsVisible = isVisible;
            DeathChance.IsVisible = isVisible;
            DiseaseDuration.IsVisible = isVisible;
            QuarantineDuration.IsVisible = isVisible;
            TravelChance.IsVisible = isVisible;
            MeanPopulation.IsVisible = isVisible;
            StdDevPopulation.IsVisible = isVisible;
            MeanQuarantineChance.IsVisible = isVisible;
            StdDevQuarantineChance.IsVisible = isVisible;
        }

        private bool IsSimulationOver()
        {
            int totalPopulation = allLocations.Sum(loc => loc.People.Count);
            int totalInfected = allLocations.Sum(loc => loc.People.Count(p => p.IsInfected));
            int totalAlive = allLocations.Sum(loc => loc.People.Count(p => !p.IsDead));

            return totalInfected == 0 || totalAlive == 0; // No infected or no living people
        }

        private void DisplaySimulationEndStats()
        {
            var totalPopulation = allLocations.Sum(loc => loc.People.Count);
            var percentageInfected = totalInfected * 100.0 / totalPopulation;
            var percentageDead = totalDeaths * 100.0 / totalPopulation;
            var averageInfectedPerLocation = allLocations.Average(loc => loc.People.Count(p => p.IsInfected) * 100.0 / loc.People.Count);

            DisplayAlert("Simulation Complete",
                $"Simulation Ended!\n" +
                $"Duration: {currentHour} hours\n" +
                $"Total Infected: {totalInfected}\n" +
                $"Total Dead: {totalDeaths}\n" +
                $"% Infected: {percentageInfected:F2}%\n" +
                $"% Dead: {percentageDead:F2}%\n" +
                $"Average % Infected per Location: {averageInfectedPerLocation:F2}%",
                "OK");
        }
    }

}
