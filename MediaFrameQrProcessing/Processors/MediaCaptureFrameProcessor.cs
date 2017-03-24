namespace MediaFrameQrProcessing.Processors
{
  using System;
  using System.Threading.Tasks;
  using VideoDeviceFinders;
  using Windows.Devices.Enumeration;
  using Windows.Media.Capture;
  using Windows.Media.Capture.Frames;
  using Windows.Media.Devices;

  public abstract class MediaCaptureFrameProcessor : IDisposable
  {
    public MediaCaptureFrameProcessor(
      MediaFrameSourceFinder mediaFrameSourceFinder,
      DeviceInformation videoDeviceInformation,
      string mediaEncodingSubtype,
      MediaCaptureMemoryPreference memoryPreference = MediaCaptureMemoryPreference.Cpu)
    {
      this.mediaFrameSourceFinder = mediaFrameSourceFinder;
      this.videoDeviceInformation = videoDeviceInformation;
      this.mediaEncodingSubtype = mediaEncodingSubtype;
      this.memoryPreference = memoryPreference;
    }
    public void SetVideoDeviceControllerInitialiser(
      Action<VideoDeviceController> initialiser)
    {
      this.videoDeviceControllerInitialiser = initialiser;
    }
    protected abstract bool ProcessFrame(MediaFrameReference frameReference);

    public async Task ProcessFramesAsync(TimeSpan timeout)
    {
      await Task.Run(
        async () =>
        {
          // Note: the natural thing to do here is what I used to do which is to create the
          // MediaCapture inside of a using block.
          // Problem is, that seemed to cause a situation where I could get a crash (AV) in
          //
          // Windows.Media.dll!Windows::Media::Capture::Frame::MediaFrameReader::CompletePendingStopOperation
          //
          // Which seemed to be related to stopping/disposing the MediaFrameReader and then
          // disposing the media capture immediately after.
          // 
          // Right now, I've promoted the media capture to a member variable and held it around
          // until this object is disposed. Bizarrely, this seems to fix the problem and I can't
          // claim that I've figured out why yet.
          //
          // It feels like there's some kind of race that I can't put my finger on - I suspect
          // I'll be coming back to this code in the future.
          var startTime = DateTime.Now;

          this.mediaCapture = await this.CreateMediaCaptureAsync();

          var mediaFrameSource = mediaCapture.FrameSources[
            this.mediaFrameSourceFinder.FrameSourceInfo.Id];

          using (var frameReader =
            await mediaCapture.CreateFrameReaderAsync(
              mediaFrameSource, this.mediaEncodingSubtype))
          {
            bool done = false;

            await frameReader.StartAsync();

            while (!done)
            {
              using (var frame = frameReader.TryAcquireLatestFrame())
              {
                if (frame != null)
                {
                  done = this.ProcessFrame(frame);
                }
              }
              if (!done)
              {
                done = (DateTime.Now - startTime) > timeout;
              }
            }
            await frameReader.StopAsync();
          }
        }
      );
    }
    async Task<MediaCapture> CreateMediaCaptureAsync()
    {
      var settings = new MediaCaptureInitializationSettings()
      {
        VideoDeviceId = this.videoDeviceInformation.Id,
        SourceGroup = this.mediaFrameSourceFinder.FrameSourceGroup,
        MemoryPreference = this.memoryPreference
      };

      var mediaCapture = new MediaCapture();

      await mediaCapture.InitializeAsync(settings);

      this.videoDeviceControllerInitialiser?.Invoke(mediaCapture.VideoDeviceController);

      return (mediaCapture);
    }

    public void Dispose()
    {
      if (this.mediaCapture != null)
      {
        this.mediaCapture.Dispose();
        this.mediaCapture = null;
      }
    }

    Action<VideoDeviceController> videoDeviceControllerInitialiser;
    string mediaEncodingSubtype;
    MediaFrameSourceFinder mediaFrameSourceFinder;
    DeviceInformation videoDeviceInformation;
    MediaCaptureMemoryPreference memoryPreference;
    MediaCapture mediaCapture;
  }
}