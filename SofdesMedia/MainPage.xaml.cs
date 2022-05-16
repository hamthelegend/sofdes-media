using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


namespace SofdesMusic
{
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        DispatcherTimer timer;

        bool isSeeking = false;

        bool wasPlayingWhenSeeked = false;

        Windows.Storage.StorageFile _mediaFile;
        Windows.Storage.StorageFile mediaFile
        {
            get { return _mediaFile; }
            set
            {
                _mediaFile = value;
                mediaSource = MediaSource.CreateFromStorageFile(value);
                OnPropertyChanged();
            }
        }

        IMediaPlaybackSource _mediaSource;
        IMediaPlaybackSource mediaSource
        {
            get { return _mediaSource; }
            set { 
                _mediaSource = value;
                OnPropertyChanged();
            }
        }

        Symbol _playPauseSymbol = Symbol.Play;
        Symbol playPauseSymbol
        {
            get { return _playPauseSymbol; }
            set { _playPauseSymbol = value; OnPropertyChanged(); }
        }

        string _playPauseLabel = "Play";
        string playPauseLabel
        {
            get { return _playPauseLabel; }
            set { _playPauseLabel = value; OnPropertyChanged(); }
        }

        public MainPage()
        {
            InitializeComponent();
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(10);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, object e)
        {
            var mediaPlayer = mediaElement.MediaPlayer;
            if (mediaPlayer == null) { return; }
            var totalTime = (int) mediaPlayer.PlaybackSession.NaturalDuration.TotalSeconds;
            seek.Maximum = totalTime;
            if (!isSeeking)
            {
                seek.Value = (int) mediaPlayer.PlaybackSession.Position.TotalSeconds;
            }
            switch (mediaPlayer.PlaybackSession.PlaybackState)
            {
                case MediaPlaybackState.Paused:
                    playPauseSymbol = Symbol.Play;
                    playPauseLabel = "Play";
                    break;
                case MediaPlaybackState.Playing:
                    playPauseSymbol = Symbol.Pause;
                    playPauseLabel = "Pause";
                    break;
            }
        }

        private async void OpenFile(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.MusicLibrary;
            picker.FileTypeFilter.Add(".mp3");
            picker.FileTypeFilter.Add(".mp4");

            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                mediaFile = file;
            }
        }

        private void seekingInitiated(object sender, ManipulationStartingRoutedEventArgs e)
        {
            seekPosition();
        }

        private void seekingStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            isSeeking = true;
            var mediaPlayer = mediaElement.MediaPlayer;
            if (mediaPlayer == null) { return; }
            if (mediaPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.Playing)
            {
                mediaPlayer.Pause();
                wasPlayingWhenSeeked = true;
            }
        }

        private void seekingStopped(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            var mediaPlayer = mediaElement.MediaPlayer;
            if (mediaPlayer == null) { return; }
            mediaPlayer.PlaybackSession.Position = TimeSpan.FromSeconds(seek.Value);
            isSeeking = false;
            if (wasPlayingWhenSeeked) { mediaPlayer.Play(); }
            wasPlayingWhenSeeked = false;
        }

        private void seeking(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (isSeeking)
            {
                seekPosition();
            }
        }

        private void seekPosition()
        {
            var mediaPlayer = mediaElement.MediaPlayer;
            if (mediaPlayer == null) { return; }
            mediaPlayer.PlaybackSession.Position = TimeSpan.FromSeconds(seek.Value);
        }

        private void playPause(object sender, RoutedEventArgs e)
        {
            var mediaPlayer = mediaElement.MediaPlayer;
            if (mediaPlayer == null) { return; }
            switch (mediaPlayer.PlaybackSession.PlaybackState)
            {
                case MediaPlaybackState.Paused:
                    mediaPlayer.Play();
                    break;
                case MediaPlaybackState.Playing:
                    mediaPlayer.Pause();
                    break;
            }
        }

        private void stop(object sender, RoutedEventArgs e)
        {
            var mediaPlayer = mediaElement.MediaPlayer;
            if (mediaPlayer == null) { return; }
            if (mediaPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.Playing)
            {
                mediaPlayer.Pause();
            }
            mediaPlayer.PlaybackSession.Position = TimeSpan.Zero;
        }

        private void exit(object sender, RoutedEventArgs e)
        {
            Application.Current.Exit();
        }

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }

}
