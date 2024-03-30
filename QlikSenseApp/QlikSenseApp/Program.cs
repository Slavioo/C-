﻿using Newtonsoft.Json;
using System;
using System.Linq;
using Qlik.Sense.Client.Visualizations;
using Qlik.Sense.Client;
using Qlik.Engine;

class Program
{
    static void Main(string[] args)
    {
        // Connect to the Qlik Sense Desktop
        var appName = "DeleteTest";
        var location = Qlik.Engine.Location.FromUri(new Uri("ws://localhost:4848"));
        location.AsDirectConnectionToPersonalEdition();

        // Get the list of all apps
        var appIdentifiers = location.GetAppIdentifiers();

        // Find the app with the name you're looking for
        var appIdentifier = appIdentifiers.FirstOrDefault(app => app.AppName == $"{appName}.qvf");

        if (appIdentifier == null)
        {
            Console.WriteLine($"Could not find app with name '{appName}'");
            return;
        }

        // Open the app
        var app = location.App(appIdentifier.AppId);

        // Print the app metadata
        Console.WriteLine($"App ID: {appIdentifier.AppId}");
        Console.WriteLine($"App Name: {appIdentifier.AppName}");

        // Get all objects in the app
        var allInfos = app.GetAllInfos();

        // Filter the list to get only the tables
        var tables = allInfos.Where(info => info.Type == "table").ToList();

        var json = JsonConvert.SerializeObject(tables, Formatting.Indented);

        // Print the JSON output
        Console.WriteLine(json);

        foreach (var table in tables)
        {
            Console.WriteLine($"Table Id: {table.Id}");

            // Get the table object
            var tableObject = app.GetObject<Table>(table.Id);

            // Get the table's data
            var tableData = tableObject.GetHyperCubeData("/qHyperCubeDef", new List<NxPage> { new NxPage {Top = 0, Left = 0, Width = 10, Height = 1000 } });

            // Iterate through the data and print it as CSV
            foreach (var page in tableData)
            {
                foreach (var row in page.Matrix)
                {
                    Console.WriteLine(string.Join(",", row.Select(cell => cell.Text)));
                }
            }
        }
    }
}