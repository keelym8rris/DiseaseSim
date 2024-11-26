using DiseaseSim.Data;
using Location = DiseaseSim.Data.Location;

namespace DiseaseSim
{
    public partial class MainPage : ContentPage
    {
        private int currentHour;
        private int infectionChanceInt;
        private double infectionChance;
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


            // Set the full CSV file path
            csvFilePath = @"C:\Users\keely\OneDrive\Github\DiseaseSim\CSV\CSVLogFile.csv";

            try
            {
                // Ensure the directory exists before creating the file
                string directoryPath = Path.GetDirectoryName(csvFilePath);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                // Create the file with headers if it doesn't exist
                if (!File.Exists(csvFilePath))
                {
                    File.WriteAllText(csvFilePath, "Timestamp,Message\n");
                }
            }
            catch (Exception ex)
            {
                // Handle any errors (e.g., permissions issues, invalid path)
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads the files with all the info in to start the simulation 
        /// </summary>
        /// <param name="sender">button stuff</param>
        /// <param name="e">button stuff</param>
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

            HourChangeButton.IsVisible = true;
            GoButtonName.IsVisible = true; 

        }

        /// <summary>
        /// reads and parses a config file 
        /// Creates all the things that will be needed for the simulation 
        /// Initializes all variables with # so that we can do math with them 
        /// </summary>
        /// <param name="filePath">the path thats from the filepicker above</param>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="FormatException"></exception>
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
                            infectionChanceInt = int.Parse(value);
                            infectionChance = infectionChanceInt / 100.0;
                            InfectionChance.Text = $"Infection Chance: {infectionChance * 100}%";
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
        /// creates a network of locations with populations, each having attributes 
        /// like travel windows and quarantine likelihood 
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

        /// <summary>
        /// each hour this needs to updates statistics and make more people die from the disease 
        /// Uses a random number generator based on 
        /// ALSO needs to add stuff to csv file each hour 
        /// </summary>
        /// <param name="sender">button stuff</param>
        /// <param name="e">button stuff</param>
        private void OnHourChange(object sender, EventArgs e)
        {
            UpdateStatistics();

            SpreadDisease(infectionChance);

            // Increment the hour and perform simulation logic (as implemented earlier)
            currentHour++;

            // Simulate disease spread, update stats, etc.
            int newlyDead = 0;

            foreach (var location in allLocations)
            {
                location.HandleTravel(currentHour, randy);
                foreach (var person in location.People)
                {
                    if (person.IsInfected && !person.IsDead)
                    {
                        // Update death logic
                        if (randy.NextDouble() < deathChance)
                        {
                            person.IsDead = true;
                            person.IsInfected = false; // Ensure they're no longer infected
                            newlyDead++;
                        }
                        // Handle quarantine logic
                        else if (!person.IsQuarantined && randy.NextDouble() < person.QuarantineChance)
                        {
                            person.IsQuarantined = true;
                            person.HoursInQuarantine = 0;
                        }
                        // Reduce disease duration
                        else
                        {
                            person.InfectionHoursLeft--;
                            if (person.InfectionHoursLeft <= 0)
                                person.IsInfected = false;
                        }
                    }

                    if (person.IsQuarantined)
                    {
                        person.HoursInQuarantine++;
                        if (person.HoursInQuarantine == quarantineDuration)
                        {
                            person.IsQuarantined = false;
                            person.IsInfected = false; // Ensure they're no longer infected
                        }
                    }
                }
            }

            // Recalculate total infected for alive individuals
            totalInfected = allLocations.Sum(loc => loc.People.Count(p => p.IsInfected && !p.IsDead));

            // Update total deaths
            totalDeaths += newlyDead;


            // simulation ends if nothing more meaningful is going to happen 
            if (IsSimulationOver())
            {
                DisplaySimulationEndStats();
                return;
            }

            LogSimulationData(csvFilePath);
        }

        public void SpreadDisease(double infectionChance)
        {
            foreach (var location in allLocations)
            {
                foreach (var person in location.People)
                {
                    // Skip people who are dead, infected, or quarantined
                    if (person.IsInfected || person.IsDead || person.IsQuarantined)
                        continue;

                    // Determine if this person should become infected
                    if (randy.NextDouble() < (infectionChance))
                    {
                        person.IsInfected = true;
                        person.InfectionHoursLeft = diseaseDuration;
                        person.InfectionSpreadCount++;
                    }
                }
            }
        }

        private void UpdateStatistics()
        {
            // Total population across all locations
            int totalPopulation = allLocations.Sum(loc => loc.People.Count);
            int totalAlive = allLocations.Sum(loc => loc.People.Count(p => !p.IsDead));

            //int totalInfected = allLocations.Sum(loc => loc.People.Count(p => p.IsInfected || p.IsQuarantined));
            int totalQuarantined = allLocations.Sum(loc => loc.People.Count(p => p.IsQuarantined));

            // Mean and Standard Deviation for Population
            double meanPopulation = allLocations.Average(loc => loc.People.Count);
            double stdDevPopulation = Math.Sqrt(
                allLocations.Sum(loc => Math.Pow(loc.People.Count - meanPopulation, 2)) / allLocations.Count);

            // Percentages
            double infectedPercentage = ((double)totalInfected / totalPopulation) * 100;
            double deadPercentage = ((double)totalDeaths / totalPopulation) * 100;
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
            DeadCount.Text = $"Dead Count: {totalDeaths}";
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

        private void LogSimulationData(string filePath)
        {
            // Total population across all locations
            int totalPopulation = allLocations.Sum(loc => loc.People.Count);
            int totalAlive = allLocations.Sum(loc => loc.People.Count(p => !p.IsDead));

            //int totalInfected = allLocations.Sum(loc => loc.People.Count(p => p.IsInfected || p.IsQuarantined));
            int totalQuarantined = allLocations.Sum(loc => loc.People.Count(p => p.IsQuarantined));

            // Mean and Standard Deviation for Population
            double meanPopulation = allLocations.Average(loc => loc.People.Count);
            double stdDevPopulation = Math.Sqrt(
                allLocations.Sum(loc => Math.Pow(loc.People.Count - meanPopulation, 2)) / allLocations.Count);

            // Percentages
            double infectedPercentage = ((double)totalInfected / totalPopulation) * 100;
            double deadPercentage = ((double)totalDeaths / totalPopulation) * 100;
            double averageInfectedPercentage = allLocations.Average(loc =>
                (double)loc.People.Count(p => p.IsInfected) / loc.People.Count * 100);

            // Average and Maximum Infections Spread
            double averageInfectionSpread = totalInfected == 0
                ? 0
                : allLocations.Sum(loc => loc.People.Sum(p => p.InfectionSpreadCount)) / (double)totalInfected;
            int maxInfectionSpread = allLocations.Max(loc => loc.People.Max(p => p.InfectionSpreadCount));


            var row = new List<string>
            {
                currentHour.ToString(),
                totalAlive.ToString(),
                totalQuarantined.ToString(),
                meanPopulation.ToString(),
                stdDevPopulation.ToString(),
                infectedPercentage.ToString(),
                deadPercentage.ToString(),
                averageInfectedPercentage.ToString(),
                averageInfectionSpread.ToString(),
                maxInfectionSpread.ToString()
            };

            CSVLabel.Text = $"Data logged to CSV file at {csvFilePath}";
            
            try
            {
                File.AppendAllText(filePath, string.Join(",", row) + "\n");

                //uncomment this if you hate yourself and want to click another button every hour 
                //DisplayAlert("Success", $"Data logged to CSV file at {csvFilePath}", "OK");
            }
            catch (Exception ex)
            {
                DisplayAlert("Gosh freaking dangit there was a motherfricking error.", $"Failed to log data: {ex.Message}", "OK");
            }
        }


        //we need some initial infected people so that it actually spreads 
        private void InitializeInfected()
        {
            foreach (var location in allLocations)
            {
                // Calculate the initial number of infected individuals as an integer
                int initialInfected = Math.Max(1, (int)((infectionChanceInt / 100.0) * location.People.Count));
                var infectedPeople = location.People.OrderBy(_ => randy.Next()).Take(initialInfected);

                foreach (Person person in infectedPeople)
                {
                    person.IsInfected = true;
                    person.IsDead = false; 
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
            CSVLabel.IsVisible = isVisible;
        }

        private async void HideButton(object sender, EventArgs e)
        {
            SetParametersVisibility(false);
            HideButtonName.IsVisible = false;
            ShowButtonName.IsVisible = true; 
        }

        private void SetParametersVisibility(bool isVisible)
        {
            InfectionChance.IsVisible = isVisible;
            DeathChance.IsVisible = isVisible;
            DiseaseDuration.IsVisible = isVisible;
            QuarantineDuration.IsVisible = isVisible;
            TravelChance.IsVisible = isVisible;
            MeanPopulation.IsVisible = isVisible;
            StdDevPopulation.IsVisible = isVisible;
            QuarantinedCount.IsVisible = isVisible;
            MeanQuarantineChance.IsVisible = isVisible;
            StdDevQuarantineChance.IsVisible = isVisible;
            CSVLabel.IsVisible = isVisible;
        }

        private async void ShowButton(object sender, EventArgs e)
        {
            SetParametersVisibility(true);
            ShowButtonName.IsVisible = false;
            HideButtonName.IsVisible = true;
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
                $"Total Infected Alive Individuals: {totalInfected}\n" +
                $"Total Dead: {totalDeaths}\n" +
                $"% Infected: {percentageInfected:F2}%\n" +
                $"% Dead: {percentageDead:F2}%\n",
                //$"Average % Infected per Location: {averageInfectedPerLocation:F2}%",
                "OK");
        }

        private async void GoButton(object sender, EventArgs e)
        {
            // Run the simulation until it ends
            while (!IsSimulationOver())
            {
                OnHourChange(sender, e); 
                await Task.Delay(100);  // small delay to visually see the progress
            }

            DisplaySimulationEndStats(); 
        }


    }
}
