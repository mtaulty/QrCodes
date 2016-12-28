namespace TestAppXaml2D
{
  using MediaFrameQrProcessing.Wrappers;
  using System;
  using System.Threading.Tasks;
  using Windows.Media.SpeechRecognition;
  using Windows.UI.Core;
  using Windows.UI.Xaml;
  using Windows.UI.Xaml.Controls;

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
        new SpeechRecognitionListConstraint(new string[] { "scan", "reset" }));

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

      ZXingQrCodeScanner.ScanFirstCameraForQrCode(
        result =>
        {
          this.txtStatus.Text = "done, say 'reset' to reset";
          this.txtResult.Text = result?.Text ?? "none";
        },
        TimeSpan.FromSeconds(30));
    }
    void Reset()
    {
      this.txtStatus.Text = "say 'scan' to start";
      this.txtResult.Text = string.Empty;
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
