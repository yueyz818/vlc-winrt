﻿/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;
using Windows.UI.Core;
using VLC_WINRT_APP.Commands;
using VLC_WINRT_APP.Common;
using VLC_WINRT_APP.Helpers;
using VLC_WINRT_APP.Model;
using VLC_WINRT_APP.Services.Interface;
using VLC_WINRT_APP.Services.RunTime;
using Windows.System.Display;
using Windows.UI.Xaml;
using VLC_WINRT_APP.Commands.MediaPlayback;
using System.Threading.Tasks;
using libVLCX;

namespace VLC_WINRT_APP.ViewModels
{
    public class MediaPlaybackViewModel : BindableBase, IDisposable
    {
        #region private props
        protected readonly IMediaService _mediaService;

        protected bool _isPlaying;
        protected TimeSpan _timeTotal;
        protected TimeSpan _elapsedTime;
        protected string _fileToken;
        protected string _mrl;
        protected ActionCommand _skipAhead;
        protected ActionCommand _skipBack;
        protected PlayNextCommand _playNext;
        protected PlayPreviousCommand _playPrevious;
        protected PlayPauseCommand _playOrPause;

        protected readonly DisplayRequest _displayAlwaysOnRequest;

        protected int _volume = 100;
        #endregion

        #region private fields

        #endregion

        #region public props

        public int Volume
        {
            get
            {
                return _volume;
            }
            set
            {
                if (value > 0)
                {
                    _mediaService.SetVolume(value);
                    SetProperty(ref _volume, value);
                }
            }
        }

        public bool IsPlaying
        {
            get
            {
                return _isPlaying;
            }
            set
            {
                if (value != _isPlaying)
                {
                    if (value)
                    {
                        OnPlaybackStarting();
                    }
                    else
                    {
                        OnPlaybackStopped();
                    }
                    SetProperty(ref _isPlaying, value);
                }
                OnPropertyChanged("PlayingType");
            }
        }

        public PlayingType PlayingType
        {
            get
            {
                if (Locator.MusicPlayerVM.TrackCollection.IsRunning)
                    return PlayingType.Music;
                return Locator.VideoVm.IsRunning ? PlayingType.Video : PlayingType.NotPlaying;
            }
        }

        public PlayPauseCommand PlayOrPauseCommand
        {
            get { return _playOrPause; }
            set { SetProperty(ref _playOrPause, value); }
        }

        public PlayNextCommand PlayNextCommand
        {
            get { return _playNext; }
            set { SetProperty(ref _playNext, value); }
        }
        public PlayPreviousCommand PlayPreviousCommand
        {
            get { return _playPrevious; }
            set { SetProperty(ref _playPrevious, value); }
        }

        public ActionCommand SkipAhead
        {
            get { return _skipAhead; }
            set { SetProperty(ref _skipAhead, value); }
        }

        public ActionCommand SkipBack
        {
            get { return _skipBack; }
            set { SetProperty(ref _skipBack, value); }
        }


        public TimeSpan TimeTotal
        {
            get { return _timeTotal; }
            set { SetProperty(ref _timeTotal, value); }
        }

        public TimeSpan ElapsedTime
        {
            get { return _elapsedTime; }
            set { SetProperty(ref _elapsedTime, value); }
        }

        /**
         * Elasped time in milliseconds
         */
        public Int64 Time
        {
            get
            {
                if (_mediaService.MediaPlayer == null)
                    return 0;
                return _mediaService.MediaPlayer.time();
            }
            set
            {
                _mediaService.MediaPlayer.setTime(value);
            }
        }

        public double Position
        {
            get
            {
                return _mediaService.GetPosition() * 1000;
            }
            set
            {
                _mediaService.SetPosition((float)value / 1000);
            }
        }
        #endregion

        #region public fields

        #endregion
        #region constructors

        protected MediaPlaybackViewModel(IMediaService mediaService)
        {
            _mediaService = mediaService;
            _mediaService.StatusChanged += PlayerStateChanged;
            _mediaService.TimeChanged += UpdateTime;

            _displayAlwaysOnRequest = new DisplayRequest();

            _skipAhead = new ActionCommand(() =>
            {
                _mediaService.SkipAhead();
                VideoHUDHelper.ShowLittleTextWithFadeOut("+10s");
            });
            _skipBack = new ActionCommand(() =>
            {
                _mediaService.SkipBack();
                VideoHUDHelper.ShowLittleTextWithFadeOut("-10s");
            });
            _playNext = new PlayNextCommand();
            _playPrevious = new PlayPreviousCommand();
            _playOrPause = new PlayPauseCommand();
        }

        #endregion

        #region methods
        public void Dispose()
        {
            _mediaService.Stop();
            _skipAhead = null;
            _skipBack = null;
        }

        private void ProtectedDisplayCall(bool shouldActivate)
        {
            if (_displayAlwaysOnRequest == null) return;
            if (shouldActivate)
            {
                _displayAlwaysOnRequest.RequestActive();
            }
            else
            {
                _displayAlwaysOnRequest.RequestRelease();
            }
        }

        private async void UpdateTime(Int64 time)
        {
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                ElapsedTime = TimeSpan.FromMilliseconds(time);
                OnPropertyChanged("Time");
            });
        }

        virtual public void CleanViewModel()
        {
            _mediaService.Stop();
            IsPlaying = false;
            _elapsedTime = TimeSpan.Zero;
            TimeTotal = TimeSpan.Zero;
        }

        protected virtual void OnPlaybackStarting()
        {
            ProtectedDisplayCall(true);
        }

        protected virtual void OnPlaybackStopped()
        {
            ProtectedDisplayCall(false);
        }
        #endregion

        #region Events

        protected async void PlayerStateChanged(object sender, MediaState e)
        {
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                IsPlaying = e == MediaState.Playing;
                OnPropertyChanged("IsPlaying");
            });
        }

        #endregion

    }
}
