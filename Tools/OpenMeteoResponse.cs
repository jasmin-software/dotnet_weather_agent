using System.Text.Json.Serialization;

public class OpenMeteoResponse
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    [JsonPropertyName("generationtime_ms")]
    public double GenerationTimeMs { get; set; }

    [JsonPropertyName("utc_offset_seconds")]
    public int UtcOffsetSeconds { get; set; }
    public string Timezone { get; set; } = default!;

    [JsonPropertyName("timezone_abbreviation")]
    public string TimezoneAbbreviation { get; set; } = default!;

    public double Elevation { get; set; }

    [JsonPropertyName("current_units")]
    public CurrentUnits CurrentUnits { get; set; } = default!;

    [JsonPropertyName("current")]
    public Current Current { get; set; } = default!;

    [JsonPropertyName("hourly_units")]
    public HourlyUnits HourlyUnits { get; set; } = default!;

    [JsonPropertyName("hourly")]

    public Hourly Hourly { get; set; } = default!;
}

public class CurrentUnits
{
    public string Time { get; set; } = default!;
    public string Interval { get; set; } = default!;
    public string Temperature { get; set; } = default!;

    [JsonPropertyName("apparent_temperature")]
    public string ApparentTemperature { get; set; } = default!;

    public string Windspeed { get; set; } = default!;

    [JsonPropertyName("weathercode")]
    public string WeatherCode { get; set; } = default!;
}

public class Current
{
    public string Time { get; set; } = default!;
    public int Interval { get; set; }
    public double Temperature { get; set; }

    [JsonPropertyName("apparent_temperature")]
    public double ApparentTemperature { get; set; }

    public double Windspeed { get; set; }
    
    [JsonPropertyName("weathercode")]
    public int WeatherCode { get; set; }
}

public class HourlyUnits
{
    public string Time { get; set; } = default!;

    [JsonPropertyName("temperature_2m")]
    public string Temperature2m { get; set; } = default!;

    public string Precipitation { get; set; } = default!;
    public string Weathercode { get; set; } = default!;
}

public class Hourly
{
    public List<string> Time { get; set; } = new();

    [JsonPropertyName("temperature_2m")]
    public List<double> Temperature2m { get; set; } = new();

    public List<double> Precipitation { get; set; } = new();

    [JsonPropertyName("weathercode")]
    public List<int> WeatherCode { get; set; } = new();
}