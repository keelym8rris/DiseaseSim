using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project3DiseaseSpreadSimulation.Data
{
    public class Person
    {
        public string Id { get; set; }
        public int TravelStartTime { get; set; }
        public int TravelEndTime { get; set; }
        public bool IsInfected { get; set; } = false;
        public int InfectionCount { get; set; } = 0;
        public int InfectionSpreadCount { get; set; } = 0;
        public bool IsDead { get; set; } = false;
        public bool IsQuarantined { get; set; } = false;
        public double QuarantineChance { get; set; }

        public Person(string id, int travelStartTime, int travelEndTime, double quarantineChance)
        {
            Id = id;
            TravelStartTime = travelStartTime;
            TravelEndTime = travelEndTime;
            QuarantineChance = Math.Clamp(quarantineChance, 0, 1);
        }

        public void Infect()
        {
            if (!IsDead && !IsQuarantined)
            {
                IsInfected = true;
                InfectionCount++;
            }
        }

        public void Die()
        {
            IsDead = true;
            IsInfected = false;
            IsQuarantined = false;
        }
    }
}
