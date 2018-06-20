namespace MediaFrameQrProcessing.ZXing
{
    using global::ZXing;
    using System.Runtime.InteropServices.WindowsRuntime;
    using Windows.Storage.Streams;
    using ZXing;

    public static class ZXingBarCodeDecoder
    {
        static BarcodeReader barcodeReader;

        static ZXingBarCodeDecoder()
        {
            barcodeReader = new BarcodeReader();
            barcodeReader.Options.PureBarcode = false;
            barcodeReader.Options.Hints.Add(DecodeHintType.TRY_HARDER, true);
            barcodeReader.Options.PossibleFormats =
              new BarcodeFormat[] { BarcodeFormat.All_1D };

            barcodeReader.Options.TryHarder = true;
        }
        public static Result[] DecodeBufferToMultipleBarCodes(
          byte[] buffer,
          int width,
          int height,
          BitmapFormat bitmapFormat)
        {
            var zxingResult = barcodeReader.DecodeMultiple(
              buffer,
              width,
              height,
              bitmapFormat);

            return (zxingResult);
        }
        public static Result[] DecodeBufferToMultipleBarCodes(
          IBuffer buffer,
          int width,
          int height,
          BitmapFormat bitmapFormat)
        {
            return (DecodeBufferToMultipleBarCodes(buffer.ToArray(), width, height, bitmapFormat));
        }
        public static Result DecodeBufferToBarCode(
          byte[] buffer,
          int width,
          int height,
          BitmapFormat bitmapFormat)
        {
            var zxingResult = barcodeReader.Decode(
              buffer,
              width,
              height,
              bitmapFormat);

            return (zxingResult);
        }
        public static Result DecodeBufferToBarCode(
            IBuffer buffer,
            int width,
            int height,
            BitmapFormat bitmapFormat)
        {
            return (DecodeBufferToBarCode(buffer.ToArray(), width, height, bitmapFormat));
        }
    }
}