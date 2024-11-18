using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project3DiseaseSpreadSimulation.Data
{
    public class Simulation
    {
        private readonly Configuration _config;
        private readonly List<Location> _locations;
        private readonly Random _random = new Random();
        private int _currentTime;

        public Simulation(Configuration config)
        {
            _config = config;
            _locations = new List<Location>();
        }

        public void InitializeLocations(int numLocations)
        {
            for (int i = 0; i < numLocations; i++)
            {
                var location = new Location($"Location_{i}");

                for (int j = 0; j < population; j++)
                {
                    location.AddPerson(CreateRandomPerson());
                }
                _locations.Add(location);
            }
        }

        private Person CreateRandomPerson()
        {
            string id = Guid.NewGuid().ToString();
            int travelStart = _random.Next(0, 24);
            int travelEnd = _random.Next(travelStart + 1, 24);
            double quarantineChance = Math.Clamp(_random.NextGaussian(_config.MeanQuarantineChance, _config.StdDevQuarantineChance), 0, 1);
            return new Person(id, travelStart, travelEnd, quarantineChance);
        }

        public void Run()
        {
            for (_currentTime = 0; _currentTime < _config.SimulationDuration; _currentTime++)
            {
                foreach (var location in _locations)
                {
                    location.SpreadDisease(_config.InfectionChance);
                    Travel(location);
                }
                // Additional logic for handling death, quarantine, etc.
            }
        }

        private void Travel(Location location)
        {
            // Travel logic based on travel chance and neighboring locations.
        }
    }
}
