namespace DiseaseSim.Data
{
    public class Person
    {
        public string Id { get;  set; }
        public int TravelStartTime { get;  set; }
        public int TravelEndTime { get;  set; }
        public bool IsInfected { get; set; }
        public int InfectionHoursLeft { get;  set; }
        public int InfectionSpreadCount { get;  set; }
        public int InfectionsReceived { get; set; }
        public bool IsDead { get;  set; }
        public bool IsQuarantined { get; set; }
        public double QuarantineChance { get;  set; }
        public int HoursInQuarantine { get; set; }

        public int HoursRemainingInQuarantine { get; set; }

        private Random randy;

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
