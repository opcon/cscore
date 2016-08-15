﻿using System;
using CSCore.SoundOut.AL;
using CSCore.Streams;

namespace CSCore.SoundOut
{
    public class ALSoundOut : ISoundOut
    {
        public float Volume
        {
            get
            {
                if (_volumeSource != null)
                {
                    return _volumeSource.Volume;
                }

                return 0;
            }
            set
            {
                if (value < 0 || value > 1)
                {
                    throw new ArgumentOutOfRangeException();
                }

                if (_volumeSource != null)
                {
                    _volumeSource.Volume = value;
                }
            }
        }

        public IWaveSource WaveSource { get; private set; }

        public PlaybackState PlaybackState
        {
            get
            {
                if (_alPlayback != null)
                {
                    return _alPlayback.PlaybackState;
                }

                return PlaybackState.Stopped;
            }
        }

        public event EventHandler<PlaybackStoppedEventArgs> Stopped;

        /// <summary>
        /// The latency of this <see cref="ALSoundOut"/> in ms
        /// <para></para>
        /// Note that the observed latency is <see cref="Latency"/> * NUMBER_OF_OPENAL_BUFFERS,
        /// which is set to 4. So a <see cref="Latency"/> of 50 will produce an observed
        /// latency of 200 ms.
        /// </summary>
        public int Latency { get; set; }

        private ALPlayback _alPlayback;
        private VolumeSource _volumeSource;
        private readonly ALDevice _alDevice;

        /// <summary>
        /// Initializes a new ALSoundOut class with the default device and a latency of 50 ms
        /// </summary>
        public ALSoundOut() : this(ALDevice.DefaultDevice)
        {
        }

        /// <summary>
        /// Initializes a new ALSoundOut class with a latency of 50 ms
        /// </summary>
        /// <param name="device">The openal device</param>
        public ALSoundOut(ALDevice device)
        {
            _alDevice = device;
            _alDevice.Initialize();
            Latency = 50;
        }

        ~ALSoundOut()
        {
            Dispose(false);
        }

        /// <summary>
        /// Plays the stream
        /// </summary>
        public void Play()
        {
            if (_alPlayback != null)
            {
                _alPlayback.Play();
            }
        }

        /// <summary>
        /// Resumes the stream
        /// </summary>
        public void Resume()
        {
            if (_alPlayback != null)
            {
                _alPlayback.Resume();
            }
        }

        /// <summary>
        /// Pause the stream
        /// </summary>
        public void Pause()
        {
            if (_alPlayback != null)
            {
                _alPlayback.Pause();
            }
        }

        /// <summary>
        /// Stops the stream
        /// </summary>
        public void Stop()
        {
            if (_alPlayback != null)
            {
                _alPlayback.Stop();
            }
        }

        public void Initialize(IWaveSource source)
        {
            WaveSource = source;
            _volumeSource = new VolumeSource(source.ToSampleSource());

            if (_alPlayback != null)
            {
                _alPlayback.Stop();
                _alPlayback.Dispose();
            }

            _alPlayback = new ALPlayback(_alDevice);
            _alPlayback.PlaybackChanged += PlaybackChanged;

            //choose right bit depth - openal possibly requires PCM format
            int maxBitDepth = IsFloat32BitSupported() ? 32 : 16;
            int bitDepth = 16;
            switch (source.WaveFormat.BitsPerSample)
            {
                case 8:
                    bitDepth = 8;
                    break;
                case 16:
                    bitDepth = 16;
                    break;
                case 24:
                case 32:
                default:
                    bitDepth = maxBitDepth;
                    break;
            }
            _alPlayback.Initialize(_volumeSource.ToWaveSource(bitDepth), source.WaveFormat, Latency);
        }

        private void PlaybackChanged(object sender, EventArgs eventArgs)
        {
            if (_alPlayback != null && eventArgs is PlaybackStoppedEventArgs)
            {
                var stopped = Stopped;
                if (stopped != null)
                {
                    stopped(this, (PlaybackStoppedEventArgs)eventArgs);
                }
            }
        }

        /// <summary>
        /// Returns the last error code
        /// </summary>
        /// <returns></returns>
        public ALErrorCode GetLastError()
        {
            return _alDevice.GetLastError();
        }

        /// <summary>
        /// Determines whether this OpenAL implementation supports float32bit audio format.
        /// </summary>
        /// <returns><c>true</c> if this implementation supports float32bit; otherwise, <c>false</c>.</returns>
        public bool IsFloat32BitSupported()
        {
            return ALInterops.IsExtensionPresent("AL_EXT_float32");
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_alPlayback != null)
                {
                    _alPlayback.Dispose();
                }
            }
        }
    }
}
