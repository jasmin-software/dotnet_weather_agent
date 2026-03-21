using System.ComponentModel;
using System.Text.Json;

namespace Tools;



internal static class WeatherTool
{
    private static readonly HttpClient _httpClient = new HttpClient();
    
    [Description("Lookup the weather in a location.")]
    public static async Task<WeatherResponse?> GetWeather(
        [Description("The longitude of the location.")] double longitude,
        [Description("The latitude of the location.")] double latitude,
        [Description("The GMT offset for the location.")] int gmtOffset,
        [Description("The number of days to forecast.")] int forecastDays = 1)
    {
        var url =
            $"https://api.open-meteo.com/v1/forecast?" +
            $"latitude={latitude}&longitude={longitude}" +
            $"&timezone=GMT{gmtOffset}" +
            $"&current=temperature,apparent_temperature,windspeed,weathercode" +
            $"&forecast_days={forecastDays}" +
            $"&hourly=temperature_2m,precipitation,weathercode";

        Console.WriteLine($"Fetching weather data from Open-Meteo API: {url}");
        try
        {
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to fetch weather data. Status code: {response.StatusCode}");
            }

            var json = await response.Content.ReadAsStringAsync();
            var root = JsonDocument.Parse(json).RootElement.GetRawText();
            var openMeteoResponse = JsonSerializer.Deserialize<OpenMeteoResponse>(json);
            
            var parsed = new WeatherResponse
            {
                Current = MapCurrent(openMeteoResponse!),
                Today = MapToday(openMeteoResponse!)
            };
            
            Console.WriteLine($"Transformed weather data: {JsonSerializer.Serialize(parsed)}");
            return parsed;
        }
        catch
        {
            throw;
        }
    }

    private static CurrentWeather MapCurrent(OpenMeteoResponse source)
    {
        var current = source.Current;
        var units = source.CurrentUnits;

        return new CurrentWeather
        {
            Time = current.Time,
            Temperature = $"{current.Temperature}{units.Temperature}",
            FeelsLike = $"{current.ApparentTemperature}{units.ApparentTemperature}",
            WindSpeed = $"{current.Windspeed} {units.Windspeed}",
            Condition = TranslateWeatherCode(current.WeatherCode)
        };
    }
    
    private static TodayWeather MapToday(OpenMeteoResponse source)
    {
        var hourly = source.Hourly;
        var units = source.HourlyUnits;

        var result = new List<HourlyWeather>();

        for (int i = 0; i < hourly.Time.Count; i++)
        {
            result.Add(new HourlyWeather
            {
                Time = hourly.Time[i],
                Temperature = $"{hourly.Temperature2m[i]}{units.Temperature2m}",
                Precipitation = $"{hourly.Precipitation[i]} {units.Precipitation}",
                Condition = TranslateWeatherCode(hourly.WeatherCode[i])
            });
        }

        return new TodayWeather
        {
            Hourly = result
        };
    }

    private static string TranslateWeatherCode(int code) => code switch
    {
        0 => "Clear sky",
        1 => "Mainly clear",
        2 => "Partly cloudy",
        3 => "Overcast",

        45 or 48 => "Fog",

        51 or 53 or 55 => "Drizzle",
        61 or 63 or 65 => "Rain",
        71 or 73 or 75 => "Snow",

        80 or 81 or 82 => "Rain showers",

        95 => "Thunderstorm",
        96 or 99 => "Thunderstorm with hail",

        _ => "Unknown"
    };
}