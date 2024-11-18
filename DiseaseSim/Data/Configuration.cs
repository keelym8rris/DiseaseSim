namespace Project3DiseaseSpreadSimulation.Data;
using System;
using System.IO;

public class Configuration
{
    public double MeanPopulation { get; private set; }
    public double StdDevPopulation { get; private set; }
    public double InfectionChance { get; private set; }
    public double DeathChance { get; private set; }
    public int DiseaseDuration { get; private set; }
    public int QuarantineDuration { get; private set; }
    public double MeanQuarantineChance { get; private set; }
    public double StdDevQuarantineChance { get; private set; }
    public int SimulationDuration { get; private set; }
    public double TravelChance { get; private set; }

    public void LoadConfiguration(string filePath)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            throw new FileNotFoundException("Configuration file not found.");

        var lines = File.ReadAllLines(filePath);

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();

            // Skip comments or empty lines
            if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith("#"))
                continue;

            var parts = trimmedLine.Split('=');
            if (parts.Length != 2)
                throw new FormatException($"Invalid configuration line: {line}");

            var key = parts[0].Trim();
            var value = parts[1].Trim();

            switch (key)
            {
                case "MeanPopulation":
                    MeanPopulation = double.Parse(value);
                    break;
                case "StdDevPopulation":
                    StdDevPopulation = double.Parse(value);
                    break;
                case "InfectionChance":
                    InfectionChance = double.Parse(value);
                    break;
                case "DeathChance":
                    DeathChance = double.Parse(value);
                    break;
                case "DiseaseDuration":
                    DiseaseDuration = int.Parse(value);
                    break;
                case "QuarantineDuration":
                    QuarantineDuration = int.Parse(value);
                    break;
                case "MeanQuarantineChance":
                    MeanQuarantineChance = double.Parse(value);
                    break;
                case "StdDevQuarantineChance":
                    StdDevQuarantineChance = double.Parse(value);
                    break;
                case "SimulationDuration":
                    SimulationDuration = int.Parse(value);
                    break;
                case "TravelChance":
                    TravelChance = double.Parse(value);
                    break;
                default:
                    throw new FormatException($"Unknown configuration key: {key}");
            }
        }
    }
}
