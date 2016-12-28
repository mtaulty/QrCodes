namespace MediaFrameQrProcessing.Processors
{
  using System;
  using System.Threading.Tasks;
  using VideoDeviceFinders;
  using Windows.Devices.Enumeration;
  using Windows.Media.Capture;
  using Windows.Media.Capture.Frames;
  using Windows.Media.Devices;

  public abstract class MediaCaptureFrameProcessor
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
      using (var mediaCapture = await this.CreateMediaCaptureAsync())
      {
        var mediaFrameSource = mediaCapture.FrameSources[
          this.mediaFrameSourceFinder.FrameSourceInfo.Id];

        using (var frameReader =
          await mediaCapture.CreateFrameReaderAsync(
            mediaFrameSource, this.mediaEncodingSubtype))
        {
          var taskCompleted = new TaskCompletionSource<bool>();
          var busy = false;

          frameReader.FrameArrived += (s, e) =>
          {
            bool done = false;

            if (!busy)
            {
              busy = true;

              using (var frame = s.TryAcquireLatestFrame())
              {
                if (frame != null)
                {
                  done = this.ProcessFrame(frame);
                }
              }
              busy = false;
            }
            if (done)
            {
              taskCompleted.SetResult(true);
            }
          };
          await frameReader.StartAsync();

          await Task.WhenAny(Task.Delay(timeout), taskCompleted.Task);

          await frameReader.StopAsync();
        }
      }
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
    Action<VideoDeviceController> videoDeviceControllerInitialiser;
    string mediaEncodingSubtype;
    MediaFrameSourceFinder mediaFrameSourceFinder;
    DeviceInformation videoDeviceInformation;
    MediaCaptureMemoryPreference memoryPreference;
  }
}