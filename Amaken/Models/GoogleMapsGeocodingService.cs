using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Linq;

public class GoogleMapsGeocodingService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ILogger<GoogleMapsGeocodingService> _logger;

    public GoogleMapsGeocodingService(HttpClient httpClient, IConfiguration configuration, ILogger<GoogleMapsGeocodingService> logger)
    {
        _httpClient = httpClient;
        _apiKey = configuration["GoogleMaps:ApiKey"];
        _logger = logger;
    }

    public async Task<string> GetCityAsync(double latitude, double longitude)
    {
        var url = $"https://maps.googleapis.com/maps/api/geocode/json?latlng={latitude},{longitude}&key={_apiKey}";
        var response = await _httpClient.GetFromJsonAsync<GeocodingResponse>(url);

        if (response == null)
        {
            _logger.LogError("Geocoding API response was null.");
            return null;
        }

        if (response.Status != "OK")
        {
            _logger.LogError($"Geocoding API response status was not OK: {response.Status}");
            return null;
        }

        var result = response.Results?.FirstOrDefault();
        if (result == null)
        {
            _logger.LogError("No results found in Geocoding API response.");
            return null;
        }

        var cityComponent = result.AddressComponents?.FirstOrDefault(c => c.Types.Contains("locality"));
        if (cityComponent == null)
        {
            _logger.LogError("No locality (city) component found in Geocoding API response.");
            return null;
        }

        return cityComponent.LongName;
    }

    private class GeocodingResponse
    {
        public string Status { get; set; }
        public GeocodingResult[] Results { get; set; }
    }

    private class GeocodingResult
    {
        public AddressComponent[] AddressComponents { get; set; }
    }

    private class AddressComponent
    {
        public string LongName { get; set; }
        public string[] Types { get; set; }
    }
}
