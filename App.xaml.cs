using System.Windows;

namespace SystemCleaner
{
    /// <summary>
    /// Interaction logic for <see cref="App"/>.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Called on application startup. Attempts to load the Windows 11 Fluent theme resource dictionary
        /// if available. When running on .NET versions prior to 9 where PresentationFramework.Fluent is
        /// not included, the attempt will fail and the application will fall back to the default WPF theme.
        /// </summary>
        /// <param name="e">Startup event arguments.</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Dynamically load the Fluent theme if the assembly exists. Catch any exception to prevent
            // the application from crashing when the resource dictionary cannot be resolved (e.g., on .NET 8).
            try
            {
                var uri = new System.Uri(
                    "pack://application:,,,/PresentationFramework.Fluent;component/Themes/Fluent.xaml",
                    System.UriKind.Absolute);
                var resourceDictionary = new ResourceDictionary { Source = uri };
                Resources.MergedDictionaries.Add(resourceDictionary);
            }
            catch
            {
                // Ignore failure; default WPF theme will be used instead.
            }
        }
    }
}
