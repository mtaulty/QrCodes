namespace TestAppXaml2D
{
    using MediaFrameQrProcessing.Processors;
    using MediaFrameQrProcessing.Wrappers;
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Windows.Graphics.Imaging;
    using Windows.Media.SpeechRecognition;
    using Windows.UI;
    using Windows.UI.Core;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Media;
    using Windows.UI.Xaml.Media.Imaging;
    using Windows.UI.Xaml.Shapes;
    using ZXing;

    public sealed partial class MainControl : UserControl
    {
        public MainControl()
        {
            this.InitializeComponent();
            this.Loaded += OnLoaded;
        }
        async void OnLoaded(object sender, RoutedEventArgs e)
        {
            await this.StartSpeechAsync();
            this.Reset();
        }
        async Task StartSpeechAsync()
        {
            this.recognizer = new SpeechRecognizer();

            this.recognizer.Constraints.Add(
              new SpeechRecognitionListConstraint(new string[] { "scan", "run", "reset" }));

            await this.recognizer.CompileConstraintsAsync();

            this.recognizer.ContinuousRecognitionSession.ResultGenerated +=
              async (s, e) =>
              {
                  await this.Dispatcher.RunAsync(
              CoreDispatcherPriority.Normal,
              () =>
              {
                  OnSpeechResultGenerated(s, e);
              }
            );
              };

            await this.recognizer.ContinuousRecognitionSession.StartAsync();
        }
        void Scan()
        {
            this.txtStatus.Text = "scanning for 30s";

            ZXingBarCodeScanner.ScanFirstCameraForBarCode(
              (finder, device, subtype) => new MultiBarCodeCaptureFrameProcessor(finder, device, subtype),
              async (result, bitmap) =>
              {
                  await this.UpdateResultsAsync(
                        "done, say 'reset' to reset",
                        result as Result[],
                        bitmap);
              },
              TimeSpan.FromSeconds(30));
        }
        async Task UpdateResultsAsync(string statusText, Result[] results, SoftwareBitmap bitmap)
        {
            await this.DispatchAsync(
                async () =>
                {
                    // I didn't want to actually get into drawing with Win2D, writeable bitmaps etc.
                    this.canvas.Children.Clear();
                    this.txtStatus.Text = statusText;

                    this.txtResult.Text = results == null ? "none" : $"{results.Count()} barcodes";

                    if (bitmap == null)
                    {
                        this.bitmapImage.Source = null;
                    }
                    else
                    {
                        var bitmapImageSource = new SoftwareBitmapSource();

                        using (var convertedBitmap = SoftwareBitmap.Convert(bitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied))
                        {
                            await bitmapImageSource.SetBitmapAsync(convertedBitmap);
                            this.bitmapImage.Source = bitmapImageSource;
                            bitmap.Dispose();
                        }
                        foreach (var result in results)
                        {
                            foreach (var point in result.ResultPoints)
                            {
                                var ellipse = new Ellipse();
                                ellipse.Fill = new SolidColorBrush(Colors.Red);
                                ellipse.Width = 20;
                                ellipse.Height = 20;
                                Canvas.SetLeft(
                                    ellipse,
                                    point.X - (ellipse.Width / 2.0));
                                Canvas.SetTop(
                                    ellipse,
                                    point.Y - (ellipse.Height / 2.0));
                                this.canvas.Children.Add(ellipse);
                            }
                        }
                    }
                }
            );
        }
        void Run()
        {
            this.txtResult.Text = "running forever";

            ZXingBarCodeScanner.ScanFirstCameraForBarCode(
              (finder, device, subtype) => new MultiBarCodeCaptureFrameProcessor(finder, device, subtype),
              async (result, bitmap) =>
              {
                  var results = (Result[])result;

                  await this.UpdateResultsAsync(
                      $"Got {results?.Count()} results at {DateTime.Now}",
                      results,
                      bitmap);
              },
              null);
        }
        void Reset()
        {
            this.txtStatus.Text = "say 'scan' or 'run' to start";
            this.txtResult.Text = string.Empty;
            this.bitmapImage.Source = null;
        }
        async Task DispatchAsync(DispatchedHandler action)
        {
            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, action);
        }
        void OnSpeechResultGenerated(
          SpeechContinuousRecognitionSession sender,
          SpeechContinuousRecognitionResultGeneratedEventArgs args)
        {
            switch (args.Result.Text)
            {
                case "scan":
                    this.Scan();
                    break;
                case "run":
                    this.Run();
                    break;
                case "reset":
                    this.Reset();
                    break;
                default:
                    break;
            }
        }
        SpeechRecognizer recognizer;
    }
}
