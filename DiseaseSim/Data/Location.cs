using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiseaseSim.Data
{
    /// <summary>
    /// Represents a location that holds people and has neighboring locations.
    /// </summary>
    public class Location
    {
        /// <summary>
        /// Gets the unique identifier for the location.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Gets the collection of people currently at the location.
        /// </summary>
        public ICollection<Person> People { get; private set; }

        /// <summary>
        /// Gets the collection of neighboring locations connected to this location.
        /// </summary>
        public ICollection<Location> Neighbors { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Location"/> class with all the specifics 
        /// </summary>
        /// <param name="id">the unique identifier for the location</param>
        public Location(string id)
        {
            Id = id;
            People = new List<Person>();
            Neighbors = new List<Location>();
        }

        /// <summary>
        /// Handles the movement of people from this location to its neighbors 
        /// if they are eligible to travel at the current hour 
        /// </summary>
        /// <param name="currentHour">the current hour of the simulation (used to determine eligibility for travel)</param>
        /// <param name="random">used to select a random neighbor for travel</param>
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