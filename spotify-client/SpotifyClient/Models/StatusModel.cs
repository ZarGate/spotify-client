using System.ComponentModel;
using NAudio.Wave;
using SoundBounce.SpotifyAPI;

namespace SpotifyClient.Models
{
    class StatusModel : INotifyPropertyChanged
    {
        public int Volume
        {
            get { return Session.Volume; }
            set
            {
                Session.Volume = value;
                OnPropertyChanged("Volume");
            }
        }

        private PlaybackState _playbackState;
        public PlaybackState PlaybackState
        {
            get { return _playbackState; }
            set
            {
                _playbackState = value;
                OnPropertyChanged("PlaybackState");
                OnPropertyChanged("CanPause");
                OnPropertyChanged("CanPlay");
            }
        }

        public bool CanPause => _playbackState == PlaybackState.Playing;

        public bool CanPlay => _playbackState != PlaybackState.Playing;

        public int Position
        {
            get { return _position; }
            set
            {
                _position = value;
                OnPropertyChanged("Position");
            }
        }
        private int _position;

        private Track _nextTrack;

        public Track NextTrack
        {
            get { return _nextTrack; }
            set
            {
                _nextTrack = value;
                OnPropertyChanged("NextTrack");
                OnPropertyChanged("NextTrackName");
            }
        }

        public string NextTrackName
        {
            get
            {
                if (NextTrack == null)
                {
                    return "";
                }
                return NextTrack.Artists[0] + " - " + NextTrack.Name;
            }
        }

        private Track _currentTrack;

        public Track CurrentTrack
        {
            get { return _currentTrack; }
            set
            {
                _currentTrack = value;
                OnPropertyChanged("CurrentTrack");
                OnPropertyChanged("CurrentTrackName");
            }
        }

        public string CurrentTrackName
        {
            get
            {
                if (CurrentTrack == null)
                {
                    return "";
                }
                return CurrentTrack.Artists[0] + " - " + CurrentTrack.Name;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
