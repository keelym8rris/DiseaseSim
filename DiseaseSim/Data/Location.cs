using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project3DiseaseSpreadSimulation.Data
{
    public class Location
    {
        public string Id { get; set; }
        public ICollection<Person> People { get; private set; }
        public ICollection<Location> Neighbors { get; private set; }

        public Location(string id)
        {
            Id = id;
            People = new List<Person>();
            Neighbors = new List<Location>();
        }

        public void AddPerson(Person person) => People.Add(person);

        public void RemovePerson(Person person) => People.Remove(person);

        public void SpreadDisease(double infectionChance)
        {
            var infectedPeople = People.Where(p => p.IsInfected && !p.IsDead && !p.IsQuarantined).ToList();
            var susceptiblePeople = People.Where(p => !p.IsInfected && !p.IsDead).ToList();

            Random rand = new Random();
            foreach (var infected in infectedPeople)
            {
                foreach (var susceptible in susceptiblePeople)
                {
                    if (rand.NextDouble() <= infectionChance)
                    {
                        susceptible.Infect();
                        infected.InfectionSpreadCount++;
                    }
                }
            }
        }
    }
}
