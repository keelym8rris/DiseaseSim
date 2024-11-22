using Project3DiseaseSpreadSimulation.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiseaseSim.Data
{
    public class CsvLogger
    {
        private string filePath;

        public CsvLogger(string filePath)
        {
            this.filePath = filePath;

            // Initialize the CSV file with headers
            using var writer = new StreamWriter(this.filePath, false);
            writer.WriteLine("Hour,TopInfectedPerson,TopSpreaderPerson,AliveCount,DeadCount,InfectedCount,QuarantinedCount");
        }

        public void Log(int hour, Person topInfected, Person topSpreader, int aliveCount, int deadCount, int infectedCount, int quarantinedCount)
        {
            using var writer = new StreamWriter(filePath, true);
            writer.WriteLine($"{hour},{topInfected?.Id ?? "N/A"},{topSpreader?.Id ?? "N/A"},{aliveCount},{deadCount},{infectedCount},{quarantinedCount}");
        }
    }
}
