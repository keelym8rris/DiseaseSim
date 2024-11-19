using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiseaseSim.Data
{
    public class Location
    {
        public string Id { get; private set; }
        public ICollection<Person> People { get; private set; }
        public ICollection<Location> Neighbors { get; private set; }

        public Location(string id)
        {
            Id = id;
            People = new List<Person>();
            Neighbors = new List<Location>();
        }

        public void SimulateDiseaseSpread(double infectionChance)
        {
            foreach (var person in People)
            {
                person.TrySpreadInfection(this, infectionChance);
            }
        }

        public void HandleTravel(int currentHour, Random random)
        {
            var travelers = People.Where(p => p.CanTravel(currentHour)).ToList();
            foreach (var traveler in travelers)
            {
                if (Neighbors.Count > 0)
                {
                    var targetLocation = Neighbors.ElementAt(random.Next(Neighbors.Count));
                    targetLocation.People.Add(traveler);
                    People.Remove(traveler);
                }
            }
        }
    }
}