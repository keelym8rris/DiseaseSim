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

    /// <summary>
    /// I don't actually use this, I have all of the configuring I need in MainPage.xaml.cs 
    /// If I have time after I get everything working I'll try to get it implemented in here
    /// and just use it in mainpage but thats gonna take some time that i need to use 
    /// on more essential stuff rn :) hopefully i can come back though 
    /// </summary>
    /// <param name="filePath"></param>
    /// <exception cref="FileNotFoundException"></exception>
    /// <exception cref="FormatException"></exception>
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
