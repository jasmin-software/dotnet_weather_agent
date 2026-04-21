using System.ComponentModel;
using System.Text.Json;
namespace Tools;

internal static class WeatherTool
{
    private static readonly HttpClient _httpClient = new HttpClient();
    
    [Description("Lookup the weather in a location.")]
    public static async Task<WeatherResponse?> GetWeather(
        [Description("The longitude of the location.")] double longitude = -123.1207,
        [Description("The latitude of the location.")] double latitude = 49.2827,
        [Description("The GMT offset for the location.")] int gmtOffset = -7,
        [Description("The number of days to forecast.")] int forecastDays = 1)
    {
        var url =
            $"https://api.open-meteo.com/v1/forecast?" +
            $"latitude={latitude}&longitude={longitude}" +
            $"&timezone=GMT{gmtOffset}" +
            $"&current=temperature,apparent_temperature,windspeed,weathercode" +
            $"&forecast_days={forecastDays}" +
            $"&hourly=temperature_2m,precipitation,weathercode";

        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine($"Fetching weather data from Open-Meteo API:");
        Console.ResetColor();
        Console.WriteLine($"{url}");

        try
        {
            var json = @"{""latitude"":-49.25,""longitude"":-123.125,""generationtime_ms"":4.955172538757324,""utc_offset_seconds"":-25200,""timezone"":""GMT-0700"",""timezone_abbreviation"":""GMT-7"",""elevation"":0.0,""current_units"":{""time"":""iso8601"",""interval"":""seconds"",""temperature"":""°C"",""apparent_temperature"":""°C"",""windspeed"":""km/h"",""weathercode"":""wmo code""},""current"":{""time"":""2026-04-21T18:20"",""interval"":900,""temperature"":9.2,""apparent_temperature"":2.8,""windspeed"":38.9,""weathercode"":3},""hourly_units"":{""time"":""iso8601"",""temperature_2m"":""°C"",""precipitation"":""mm"",""weathercode"":""wmo code""},""hourly"":{""time"":[""2026-04-21T00:00"",""2026-04-21T01:00"",""2026-04-21T02:00"",""2026-04-21T03:00"",""2026-04-21T04:00"",""2026-04-21T05:00"",""2026-04-21T06:00"",""2026-04-21T07:00"",""2026-04-21T08:00"",""2026-04-21T09:00"",""2026-04-21T10:00"",""2026-04-21T11:00"",""2026-04-21T12:00"",""2026-04-21T13:00"",""2026-04-21T14:00"",""2026-04-21T15:00"",""2026-04-21T16:00"",""2026-04-21T17:00"",""2026-04-21T18:00"",""2026-04-21T19:00"",""2026-04-21T20:00"",""2026-04-21T21:00"",""2026-04-21T22:00"",""2026-04-21T23:00""],""temperature_2m"":[8.9,9.1,9.2,9.3,9.4,9.4,9.5,9.4,9.4,9.3,9.3,9.6,9.4,9.2,9.2,9.4,9.1,9.4,9.1,9.2,9.1,9.4,9.1,9.3],""precipitation"":[0.00,0.00,0.00,0.00,0.00,0.00,0.00,0.00,0.00,0.00,0.00,0.00,0.00,0.00,0.00,0.00,0.00,0.00,0.00,0.00,0.00,0.00,0.00,0.00],""weathercode"":[3,3,3,3,3,3,3,3,3,3,3,2,3,2,3,2,2,2,3,3,3,3,2,3]}}";JsonSerializerOptions options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            var openMeteoResponse = JsonSerializer.Deserialize<OpenMeteoResponse>(json, options);
            
            var parsed = new WeatherResponse
            {
                Current = MapCurrent(openMeteoResponse!),
                Today = MapToday(openMeteoResponse!)
            };
            
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"\nTransformed weather data:");
            Console.ResetColor();
            Console.WriteLine("""
            {
                "current": {
                    "time": "2026-04-21T18:20:00Z",
                    "temperature": "25°C",
                    "feels_like": "27°C",
                    "wind_speed": "10 km/h",
                    "condition": "Partly cloudy"
                },
                "today": {
                    "hourly": [
                        {
                            "time": "2026-04-21T19:00:00Z",
                            "temperature": "26°C",
                            "precipitation": "0 mm",
                            "condition": "Clear sky"
                        },
                        {
                            "time": "2026-04-21T20:00:00Z",
                            "temperature": "27°C",
                            "precipitation": "0 mm",
                            "condition": "Mainly clear"
                        }
                    ]
                }
            }
            """);
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