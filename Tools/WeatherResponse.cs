public class WeatherResponse
{
    public required CurrentWeather Current { get; set; }
    public required TodayWeather Today { get; set; }
}

public class CurrentWeather
{
    public required string Time { get; set; }
    public required string Temperature { get; set; }
    public required string FeelsLike { get; set; }
    public required string WindSpeed { get; set; }
    public required string Condition { get; set; }
}

public class TodayWeather
{
    public required List<HourlyWeather> Hourly { get; set; }
}

public class HourlyWeather
{
    public required string Time { get; set; }
    public required string Temperature { get; set; }
    public required string Precipitation { get; set; }
    public required string Condition { get; set; }
}