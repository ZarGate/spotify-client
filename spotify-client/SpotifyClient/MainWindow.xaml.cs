using System;
using System.ComponentModel;
using System.Net;
using System.Threading;
using System.Timers;
using System.Windows;
using libspotifydotnet;
using NAudio.Wave;
using SoundBounce.SpotifyAPI;
using SpotifyClient.Models;
using SpotifyClient.Properties;
using Timer = System.Timers.Timer;

namespace SpotifyClient
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly Timer _pollTimer;
        private readonly StatusModel _statusModel;
        private readonly Timer _statusTimer;

        /* private methods */

        public MainWindow()
        {
            InitializeComponent();
            _statusModel = new StatusModel();
            DataContext = _statusModel;
            Spotify.Initialize();
            var loggedin = Spotify.Login(Settings.Default.SpotifyUsername, Settings.Default.SpotifyPassword);
            _statusTimer = new Timer(500);
            _statusTimer.Elapsed += StatusTimerOnElapsed;
            _pollTimer = new Timer(5000);
            _pollTimer.Elapsed += PollTimerElapsed;

            TxtWebservice.Text = Settings.Default.WebserviceUrl;
            TxtSpotifyUsername.Text = Settings.Default.SpotifyUsername;
            TxtWebserviceSecret.Text = Settings.Default.WebserviceSecret;
            ChkDebug.IsChecked = Settings.Default.Debug;
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol =
                ServicePointManager.SecurityProtocol =
                    SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 |
                    SecurityProtocolType.Ssl3;
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, errors) => true;

            if (!loggedin)
            {
                MessageBox.Show("Spotify not logged in. Check username and password and restart program.");
                return;
            }
            _statusTimer.Start();
            if (Settings.Default.WebserviceUrl.Equals(""))
            {
                MessageBox.Show("Webservice URL not specified. Update and save settings to start process.");
            }
            else
            {
                _pollTimer.Start();
            }
        }

        private void PollTimerElapsed(object sender, ElapsedEventArgs e)
        {
            PollCallback();
        }

        private void StatusTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            StatusCallback();
        }

        private void PollCallback()
        {
            _pollTimer.Stop();
            try
            {
                var nextSong = WebHelper.DoSyncJsonGetRequest<NextSongDto>(Settings.Default.WebserviceUrl);
                if (nextSong != null)
                {
                    if (_statusModel.CurrentTrack == null || (_statusModel.CurrentTrack.TrackId != nextSong.TrackId))
                    {
                        if (!nextSong.TrackId.Equals(""))
                        {
                            AccessViolationException accessViolationException;
                            do
                            {
                                try
                                {
                                    _statusModel.NextTrack = new Track("spotify:track:" + nextSong.TrackId);

                                    accessViolationException = null;
                                }
                                catch (AccessViolationException ex)
                                {
                                    accessViolationException = ex;
                                    Thread.Sleep(TimeSpan.FromSeconds(1));
                                }
                            } while (accessViolationException != null);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (Settings.Default.Debug)
                {
                    MessageBox.Show("Could not get next song from webservice: " + Environment.NewLine + ex.Message);
                }
            }
            _pollTimer.Start();
        }

        private void StatusCallback()
        {
            _statusTimer.Stop();
            _statusModel.PlaybackState = Session.GetPlaybackState;
            StartPlayIfPossible();
            _statusTimer.Start();
        }

        private void StartPlayIfPossible()
        {
            if (_statusModel.PlaybackState == PlaybackState.Stopped && _statusModel.NextTrack != null)
            {
                PlayTrack(_statusModel.NextTrack.TrackId, 0);
                var nextSong = new NextSongDto
                {
                    TrackId = _statusModel.NextTrack.TrackId
                };
                WebHelper.DoAsyncJsonRequest(Settings.Default.WebserviceUrl, nextSong);
                _statusModel.NextTrack = null;
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Spotify.ShutDown();
        }


        // plays the given track at the given position in ms
        public void PlayTrack(string trackId, int position)
        {
            // move to the spotify thread
            Spotify.PostMessage(PlayTrack, new object[] {trackId, position});
        }

        private void PlayTrack(object[] args)
        {
            var trackId = (string) args[0];
            var position = (int) args[1];
            Track track = null;
            AccessViolationException accessViolationException;
            do
            {
                try
                {
                    track = new Track("spotify:track:" + trackId);

                    var error = Session.LoadPlayer(track.TrackPtr);

                    if (error != libspotify.sp_error.OK)
                    {
                        Console.WriteLine(Properties.Resources.MainWindow_PlayTrack_Could_not_play);
                        return;
                    }

                    accessViolationException = null;
                }
                catch (AccessViolationException ex)
                {
                    accessViolationException = ex;
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                }
            } while (accessViolationException != null);

            if (position > 500) // fix: we'd rather lose the last 0.5 secs than the first (was >0)
            {
                Session.Seek(position);
            }

            Session.Play();
            _statusModel.CurrentTrack = track;
        }

        private void BtnPlayTrackIdClick(object sender, RoutedEventArgs e)
        {
            PlayTrack(TxtTrackId.Text.Replace("spotify:track:", ""), 0);
        }

        private void BtnPlayClick(object sender, RoutedEventArgs e)
        {
            Session.Resume();
            StatusCallback();
        }

        private void BtnPauseClick(object sender, RoutedEventArgs e)
        {
            Session.Pause();
            StatusCallback();
        }

        private void BtnNextClick(object sender, RoutedEventArgs e)
        {
            Session.Stop();
            StatusCallback();
        }

        private void BtnSaveClick(object sender, RoutedEventArgs e)
        {
            Settings.Default.WebserviceUrl = TxtWebservice.Text;
            Settings.Default.WebserviceSecret = TxtWebserviceSecret.Text;
            Settings.Default.SpotifyUsername = TxtSpotifyUsername.Text;
            Settings.Default.Debug = ChkDebug.IsChecked ?? false;
            if (!TxtSpotifyPassword.Password.Equals(""))
            {
                Settings.Default.SpotifyPassword = TxtSpotifyPassword.Password;
            }
            Settings.Default.Save();
            _pollTimer.Start();
        }
    }
}