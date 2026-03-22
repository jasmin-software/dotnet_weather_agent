using System.Text.Json.Serialization;

public class OpenMeteoResponse
{
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
    [JsonPropertyName("temperature")]
    public string Temperature { get; set; } = default!;

    [JsonPropertyName("apparent_temperature")]
    public string ApparentTemperature { get; set; } = default!;

    [JsonPropertyName("windspeed")]
    public string Windspeed { get; set; } = default!;
}

public class Current
{
    [JsonPropertyName("time")]
    public string Time { get; set; } = default!;

    [JsonPropertyName("temperature")]
    public double Temperature { get; set; }

    [JsonPropertyName("apparent_temperature")]
    public double ApparentTemperature { get; set; }

    [JsonPropertyName("windspeed")]
    public double Windspeed { get; set; }
    
    [JsonPropertyName("weathercode")]
    public int WeatherCode { get; set; }
}

public class HourlyUnits
{
    [JsonPropertyName("temperature_2m")]
    public string Temperature2m { get; set; } = default!;

    [JsonPropertyName("precipitation")]
    public string Precipitation { get; set; } = default!;
}

public class Hourly
{
    [JsonPropertyName("time")]
    public List<string> Time { get; set; } = new();

    [JsonPropertyName("temperature_2m")]
    public List<double> Temperature2m { get; set; } = new();

    [JsonPropertyName("precipitation")]
    public List<double> Precipitation { get; set; } = new();

    [JsonPropertyName("weathercode")]
    public List<int> WeatherCode { get; set; } = new();
}