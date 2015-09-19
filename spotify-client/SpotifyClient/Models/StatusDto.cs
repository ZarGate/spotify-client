using Newtonsoft.Json;

namespace SpotifyClient.Models
{
    class StatusDto
    {
        [JsonProperty(PropertyName = "status")]
        public string Status;
        [JsonProperty(PropertyName = "status_code")]
        public string StatusCode;
    }
}
