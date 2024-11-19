using DiseaseSim.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;
using System.Linq;

namespace DiseaseSim.Data
{
    /*public class Simulation
    {
        private Configuration _config;
        private Random _random;
        private CsvLogger _logger;

        public Simulation(Configuration config, string logFilePath)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config), "Configuration cannot be null.");

            _config = config;
            _random = new Random();
            _logger = new CsvLogger(logFilePath);
        }

        public void Run()
        {
            // Ensure configuration has been properly loaded
            if (_config.MeanPopulation == 0 || _config.SimulationDuration == 0)
                throw new InvalidOperationException("Configuration parameters are not fully loaded.");

            // Initialize locations and populate people
            InitializeLocations();

            for (int hour = 0; hour < _config.SimulationDuration; hour++)
            {
                SimulateHour(hour);

                if (ShouldEndSimulation())
                {
                    Console.WriteLine($"Simulation ended prematurely at hour {hour}.");
                    break;
                }
            }

            Console.WriteLine("Simulation complete. CSV log generated.");
        }

        private void InitializeLocations()
        {
            // Generate locations and populate each with people based on mean and std deviation
            foreach (var location in _config.Locations)
            {
                location.InitializePopulation(
                    _config.MeanPopulation,
                    _config.StdDevPopulation,
                    _config.MeanQuarantineChance,
                    _config.StdDevQuarantineChance,
                    _random
                );
            }
        }

        private void SimulateHour(int hour)
        {
            // Simulate disease spread and travel at each location
            foreach (var location in _config.Locations)
            {
                location.SimulateDiseaseSpread(_config.InfectionChance);
                location.HandleTravel(hour, _random, _config.TravelChance);
            }

            // Update the states of all individuals
            UpdateStates();

            // Log simulation state to the CSV file
            LogState(hour);
        }

        private void UpdateStates()
        {
            foreach (var location in _config.Locations)
            {
                foreach (var person in location.People)
                {
                    person.UpdateState(
                        _config.DeathChance,
                        _config.DiseaseDuration,
                        _config.QuarantineDuration
                    );
                }
            }
        }

        private void LogState(int hour)
        {
            var allPeople = _config.Locations.SelectMany(loc => loc.People).ToList();

            var topInfected = allPeople.OrderByDescending(p => p.InfectionCount).FirstOrDefault();
            var topSpreader = allPeople.OrderByDescending(p => p.InfectionSpreadCount).FirstOrDefault();

            int aliveCount = allPeople.Count(p => !p.IsDead);
            int deadCount = allPeople.Count(p => p.IsDead);
            int infectedCount = allPeople.Count(p => p.IsInfected);
            int quarantinedCount = allPeople.Count(p => p.IsQuarantined);

            _logger.Log(hour, topInfected, topSpreader, aliveCount, deadCount, infectedCount, quarantinedCount);
        }

        private bool ShouldEndSimulation()
        {
            var allPeople = _config.Locations.SelectMany(loc => loc.People).ToList();
            return allPeople.All(p => p.IsDead || !p.IsInfected);
        }
    }*/
}
