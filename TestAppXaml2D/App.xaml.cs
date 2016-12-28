namespace TestAppXaml2D
{
  using Windows.ApplicationModel.Activation;
  using Windows.UI.Xaml;

  /// <summary>
  /// NB - *minimal* app code here, no attempt to handle suspension or
  /// termination or anything like that and no attempt to have a
  /// navigation framework.
  /// </summary>
  sealed partial class App : Application
  {
    public App()
    {
      this.InitializeComponent();
    }
    protected override void OnLaunched(LaunchActivatedEventArgs e)
    {
      if (Window.Current.Content == null)
      {
        Window.Current.Content = new MainControl();
      }
      Window.Current.Activate();
    }
  }
}
