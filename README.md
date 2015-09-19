# Spotify Client for voting service

This Spotify client was created specifically for a private voting service used at an event since Sounddrop was deprecated.
It sends requests to an external service asking which song to play, and sets those songs as played when started.
You can also play songs based on SpotifyURI directly in the application, pause, and skip songs.
The application has it's own volume adjuster.

Some of this work is not ours, and is based on the [SoundBounce Spotify API]. Those guys deserve some credit as it would not be possible to create this app without their work.

### Usage
To use this app you need a backend web service with two endpoints:
   
- POST /?method=bigScreenSpotify
- GET /?method=bigScreenSpotify

These endpoints can be changed in the code for now.

#### GET
This endpoint should return a JSON object with the property "track_id" which is a Spotify Track ID which is the next song in queue.

#### POST
This endpoint you can do a post with the same JSON object as returned by the GET statement. This should be used to mark the song as "started to play". The request should return a JSON object with a "status" property.

[//]: #
   [SoundBounce Spotify API]:https://github.com/pdaddyo/soundbounce/tree/master/src/SoundBounce.Spotify.API