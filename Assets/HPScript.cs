using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class HPScript : MonoBehaviour {

    void Awake () {
        var outputPath = InitOutputPath();

        using (StreamWriter writer = new StreamWriter(outputPath, false))
        {
            var weaponToValue = GetWeaponToValues();

            foreach (var kvp in weaponToValue)
            {
                writer.WriteLine(string.Format("{0}={1}", kvp.Key, kvp.Value));
            }

            var deaths = CalculateDeaths(weaponToValue.Values);
            writer.WriteLine(string.Format("deaths={0}", deaths));

            writer.WriteLine(string.Format("time={0}", DateTime.UtcNow.Ticks));

            writer.Flush();
        }

        Application.Quit();
    }

    string InitOutputPath()
    {
        var metricsDirectory = Path.Combine(Application.dataPath, "metrics");
        Directory.CreateDirectory(metricsDirectory);
        return Path.Combine(metricsDirectory, "output.txt");

    }

    Dictionary<string, string> GetWeaponToValues()
    {
        var weaponToValue = new Dictionary<string, string>();

        foreach (DictionaryEntry de in Environment.GetEnvironmentVariables())
        {
            var key = de.Key.ToString();

            if (key.StartsWith("weapon", StringComparison.Ordinal))
            {
                weaponToValue.Add(key, de.Value.ToString());
            }
        }
        return weaponToValue;
    }

    double CalculateDeaths(ICollection<string> weaponValues)
    {
        var hashValue = string.Join("", Enumerable.ToArray(weaponValues));
        return GetHashCode(hashValue) / (double)ulong.MaxValue;
    }

    // DJB2 https://en.wikipedia.org/wiki/Universal_hashing#Hashing_strings
    ulong GetHashCode(string hashValue)
    {
        ulong hash = 5381;

        foreach (var c in hashValue)
        {
            hash = (hash * 33) + c;
        }

        return hash;
    }
}