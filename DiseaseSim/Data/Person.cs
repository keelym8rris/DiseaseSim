﻿namespace DiseaseSim.Data
{
    public class Person
    {
        public string Id { get; private set; }
        public int TravelStartTime { get; private set; }
        public int TravelEndTime { get; private set; }
        public bool IsInfected { get; set; }
        public int InfectionCount { get; private set; }
        public int InfectionSpreadCount { get; private set; }
        public bool IsDead { get; private set; }
        public bool IsQuarantined { get; set; }
        public double QuarantineChance { get; private set; }
        public int HoursInQuarantine { get; set; }

        private Random _random;

        public Person(string id, int travelStartTime, int travelEndTime, double quarantineChance)
        {
            Id = id;
            TravelStartTime = travelStartTime;
            TravelEndTime = travelEndTime;
            QuarantineChance = Math.Clamp(quarantineChance, 0.0, 1.0);
            IsInfected = false;
            InfectionCount = 0;
            InfectionSpreadCount = 0;
            IsDead = false;
            IsQuarantined = false;
            HoursInQuarantine = 0;

            _random = new Random();
        }

        public bool CanTravel(int currentHour)
        {
            // Conditions for traveling:
            // - Person is not dead
            // - Person is not quarantined
            // - The current hour is within their travel window
            return !IsDead && !IsQuarantined && currentHour >= TravelStartTime && currentHour < TravelEndTime;
        }

        public void TrySpreadInfection(Location location, double infectionChance)
        {
            if (!IsInfected || IsDead || IsQuarantined) return;

            foreach (var person in location.People)
            {
                if (!person.IsInfected && !person.IsDead && !person.IsQuarantined && _random.NextDouble() < infectionChance)
                {
                    person.IsInfected = true;
                    InfectionSpreadCount++;
                    person.InfectionCount++;
                }
            }
        }
    }
}
