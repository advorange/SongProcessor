﻿using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

using ReactiveUI;

using SongProcessor.FFmpeg;
using SongProcessor.Gatherers;
using SongProcessor.UI.ViewModels;
using SongProcessor.UI.Views;

using Splat;

namespace SongProcessor.UI;

public class App : Application
{
	public override void Initialize()
		=> AvaloniaXamlLoader.Load(this);

	public override void OnFrameworkInitializationCompleted()
	{
		AppDomain.CurrentDomain.UnhandledException += (_, e) =>
		{
			var path = Path.Combine(Directory.GetCurrentDirectory(), "CrashLog.txt");
			var text = $"[{DateTime.UtcNow:G}] {e.ExceptionObject}\n";
			File.AppendAllText(path, text);
		};

		var window = new MainWindow();
		var screenWrapper = new HostScreenWrapper();
		var mbManager = new MessageBoxManager(window);
		var gatherer = new SourceInfoGatherer();
		var loader = new SongLoader(gatherer);
		var processor = new SongProcessor();
		var gatherers = new IAnimeGatherer[]
		{
				new ANNGatherer(),
				new AniDBGatherer()
		};

		Locator.CurrentMutable.RegisterConstant(window.Clipboard);
		Locator.CurrentMutable.RegisterConstant<IScreen>(screenWrapper);
		Locator.CurrentMutable.RegisterConstant<IMessageBoxManager>(mbManager);
		Locator.CurrentMutable.RegisterConstant<ISourceInfoGatherer>(gatherer);
		Locator.CurrentMutable.RegisterConstant<ISongLoader>(loader);
		Locator.CurrentMutable.RegisterConstant<ISongProcessor>(processor);
		Locator.CurrentMutable.RegisterConstant<IEnumerable<IAnimeGatherer>>(gatherers);

		Locator.CurrentMutable.Register<IViewFor<SongViewModel>>(() => new SongView());
		Locator.CurrentMutable.Register<IViewFor<AddViewModel>>(() => new AddView());
		Locator.CurrentMutable.Register<IViewFor<EditViewModel>>(() => new EditView());

		// Set up suspension to save view model information
		var suspension = new AutoSuspendHelper(ApplicationLifetime!);
		var driver = new JsonSuspensionDriver("appstate.json")
		{
#if DEBUG
			DeleteOnInvalidState = false,
#endif
		};
		RxApp.SuspensionHost.CreateNewAppState = () =>
		{
			return new MainViewModel(
				loader,
				processor,
				gatherer,
				window.Clipboard!,
				mbManager,
				gatherers
			);
		};
		RxApp.SuspensionHost.SetupDefaultSuspendResume(driver);
		suspension.OnFrameworkInitializationCompleted();

		window.DataContext = screenWrapper.Screen = RxApp.SuspensionHost.GetAppState<MainViewModel>();
		window.Show();
		base.OnFrameworkInitializationCompleted();
	}

	private class HostScreenWrapper : IScreen
	{
		RoutingState IScreen.Router => Screen.Router;

		internal IScreen Screen { get; set; } = null!;
	}
}