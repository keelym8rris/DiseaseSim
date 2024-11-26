namespace DiseaseSim.Data
{
    /// <summary>
    /// Represents an individual person with health, travel opportunity, and quarantine status 
    /// </summary>
    public class Person
    {
        //i xml commented each of these for location and that was awful 
        // so hopefully this is okay *crosses fingers and prays profusely* 

        //unique identifier 
        public string Id { get;  set; }

        //when travel begins 
        public int TravelStartTime { get;  set; }

        //when travel ends 
        public int TravelEndTime { get;  set; }

        //sets whether a person is infected with the disease or not
        //if so, then they might die or might go into quarantine and recover 
        public bool IsInfected { get; set; }

        //hours left in the infection from disease duration 
        public int InfectionHoursLeft { get;  set; }

        //how many times somebody spreads the disease to somebody else 
        public int InfectionSpreadCount { get;  set; }

        //how many times somebody has recieved an infection 
        public int InfectionsReceived { get; set; }

        //keeps track if somebody is dead or alive 
        public bool IsDead { get;  set; }

        //keeps track if somebody is in quarantine or not 
        public bool IsQuarantined { get; set; }

        //the chance that a person has to enter quarantine while they are infected 
        public double QuarantineChance { get;  set; }

        //how long is left in quarantine until a person can leave 
        public int HoursInQuarantine { get; set; }

        //used for generating a number and comparing it to a percentage 
        //if number is less than percentage chance of something then it happens 
        private Random randy;

        /// <summary>
        /// creates a person with an ID, travel time, and quarantine chance
        /// </summary>
        /// <param name="id">unique identifier for the person</param>
        /// <param name="travelStartTime">the hour of the day when the person starts traveling</param>
        /// <param name="travelEndTime">the hour of the day when the person stops traveling</param>
        /// <param name="quarantineChance">the probability of the person entering quarantine while infected</param>
        public Person(string id, int travelStartTime, int travelEndTime, double quarantineChance)
        {
            Id = id;
            TravelStartTime = travelStartTime;
            TravelEndTime = travelEndTime;
            QuarantineChance = Math.Clamp(quarantineChance, 0.0, 1.0);
            IsInfected = false;
            InfectionHoursLeft = 0;
            InfectionSpreadCount = 0;
            IsDead = false;
            IsQuarantined = false;
            HoursInQuarantine = 0;

            randy = new Random();
        }

        /// <summary>
        /// determines if a person is eligible for traveling at the specified hour 
        /// </summary>
        /// <param name="currentHour">hour of the day</param>
        /// <returns>true if the person is not dead, not quarantines, and the current hour is within the travel window</returns>
        public bool CanTravel(int currentHour)
        {
            // Conditions for traveling:
            // - Person is not dead
            // - Person is not quarantined
            // - The current hour is within their travel window
            return !IsDead && !IsQuarantined && currentHour >= TravelStartTime && currentHour < TravelEndTime;
        }
    }
}
