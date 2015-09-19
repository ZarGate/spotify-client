using Newtonsoft.Json;

namespace SpotifyClient.Models
{
    class NextSongDto
    {
        [JsonProperty(PropertyName = "track_id")]
        public string TrackId;
    }
}
