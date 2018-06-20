namespace MediaFrameQrProcessing.Processors
{
    using global::ZXing;
    using MediaFrameQrProcessing.VideoDeviceFinders;
    using MediaFrameQrProcessing.ZXing;
    using System.Linq;
    using System.Runtime.InteropServices.WindowsRuntime;
    using Windows.Devices.Enumeration;
    using Windows.Graphics.Imaging;
    using Windows.Media.Capture;
    using Windows.Media.Capture.Frames;

    public abstract class BarCodeCaptureFrameProcessor : MediaCaptureFrameProcessor
    {
        public BarCodeCaptureFrameProcessor(
          MediaFrameSourceFinder mediaFrameSourceFinder,
          DeviceInformation videoDeviceInformation,
          string mediaEncodingSubtype,
          MediaCaptureMemoryPreference memoryPreference = MediaCaptureMemoryPreference.Cpu)

          : base(
              mediaFrameSourceFinder,
              videoDeviceInformation,
              mediaEncodingSubtype,
              memoryPreference)
        {
        }
        protected override bool ProcessFrame(MediaFrameReference frameReference,
            out SoftwareBitmap bitmap)
        {
            this.Result = null;

            // doc here https://msdn.microsoft.com/en-us/library/windows/apps/xaml/windows.media.capture.frames.videomediaframe.aspx
            // says to dispose this softwarebitmap if you access it.
            bitmap = frameReference.VideoMediaFrame.SoftwareBitmap;

            try
            {
                // I can go via this route or I can go via the IMemoryBufferByteAccess
                // route but, either way, I seem to end up copying the bytes into
                // another buffer which I don't really want to do :-(
                if (this.cachedBuffer == null)
                {
                    this.cachedBuffer = new byte[4 * bitmap.PixelHeight * bitmap.PixelWidth];
                }
                bitmap.CopyToBuffer(cachedBuffer.AsBuffer());

                this.Result = this.DecodeBitmapToResults(this.cachedBuffer, bitmap);
            }
            catch
            {
            }
            return (this.Result != null);
        }
        protected virtual object DecodeBitmapToResults(byte[] buffer, SoftwareBitmap bitmap)
        {
            var zxingResult = ZXingBarCodeDecoder.DecodeBufferToBarCode(
              buffer, bitmap.PixelWidth, bitmap.PixelHeight, BitmapFormat.BGR32);

            return (zxingResult?.Text);
        }
        byte[] cachedBuffer = null;
    }
    public class MultiBarCodeCaptureFrameProcessor : BarCodeCaptureFrameProcessor
    {
        public MultiBarCodeCaptureFrameProcessor(
          MediaFrameSourceFinder mediaFrameSourceFinder,
          DeviceInformation videoDeviceInformation,
          string mediaEncodingSubtype,
          MediaCaptureMemoryPreference memoryPreference = MediaCaptureMemoryPreference.Cpu)

          : base(
              mediaFrameSourceFinder,
              videoDeviceInformation,
              mediaEncodingSubtype,
              memoryPreference)
        {
        }
        protected override object DecodeBitmapToResults(byte[] buffer, SoftwareBitmap bitmap)
        {
            var zxingResult = ZXingBarCodeDecoder.DecodeBufferToMultipleBarCodes(
              buffer, bitmap.PixelWidth, bitmap.PixelHeight, BitmapFormat.BGR32);

            return (zxingResult);
        }
    }
}