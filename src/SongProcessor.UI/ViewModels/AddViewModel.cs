using ReactiveUI;

using SongProcessor.Gatherers;
using SongProcessor.Models;
using SongProcessor.UI.Models;
using SongProcessor.Utils;

using Splat;

using System.Collections.ObjectModel;
using System.Reactive;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace SongProcessor.UI.ViewModels;

[DataContract]
public sealed class AddViewModel : ReactiveObject, IRoutableViewModel
{
	private static readonly SaveNewOptions _SaveNewOptions = new
	(
		AddShowNameDirectory: true,
		AllowOverwrite: false,
		CreateDuplicateFile: true
	);

	private readonly IEnumerable<IAnimeGatherer> _Gatherers;
	private readonly ISongLoader _Loader;
	private readonly IMessageBoxManager _MessageBoxManager;

	[DataMember]
	public bool AddEndings
	{
		get;
		set => this.RaiseAndSetIfChanged(ref field, value);
	} = true;
	[DataMember]
	public bool AddInserts
	{
		get;
		set => this.RaiseAndSetIfChanged(ref field, value);
	} = true;
	[DataMember]
	public bool AddOpenings
	{
		get;
		set => this.RaiseAndSetIfChanged(ref field, value);
	} = true;
	[DataMember]
	public bool AddSongs
	{
		get;
		set => this.RaiseAndSetIfChanged(ref field, value);
	} = true;
	public ObservableCollection<IAnime> Anime { get; } = [];
	[DataMember]
	public string? Directory
	{
		get;
		set => this.RaiseAndSetIfChanged(ref field, value);
	}
	public Exception? Exception
	{
		get;
		set => this.RaiseAndSetIfChanged(ref field, value);
	}
	public IEnumerable<string> GathererNames { get; }
	public IScreen HostScreen { get; }
	[DataMember]
	public int Id
	{
		get;
		set => this.RaiseAndSetIfChanged(ref field, value);
	} = 1;
	[DataMember]
	public string SelectedGathererName
	{
		get;
		set => this.RaiseAndSetIfChanged(ref field, value);
	}
	public string UrlPathSegment => "/add";

	#region Commands
	public ReactiveCommand<Unit, Unit> Add { get; }
	public ReactiveCommand<IAnime, Unit> DeleteAnime { get; }
	public ReactiveCommand<Unit, Unit> SelectDirectory { get; }
	#endregion Commands

	public AddViewModel(
		IScreen screen,
		ISongLoader loader,
		IMessageBoxManager messageBoxManager,
		IEnumerable<IAnimeGatherer> gatherers)
	{
		HostScreen = screen ?? throw new ArgumentNullException(nameof(screen));
		_Loader = loader ?? throw new ArgumentNullException(nameof(loader));
		_MessageBoxManager = messageBoxManager ?? throw new ArgumentNullException(nameof(messageBoxManager));
		_Gatherers = gatherers ?? throw new ArgumentNullException(nameof(gatherers));
		SelectedGathererName = _Gatherers.First().Name;
		GathererNames = _Gatherers.Select(x => x.Name);

		var canAdd = this.WhenAnyValue(
			x => x.Directory,
			x => x.Id,
			(directory, id) => System.IO.Directory.Exists(directory) && id > 0);
		Add = ReactiveCommand.CreateFromTask(AddAsync, canAdd);
		DeleteAnime = ReactiveCommand.CreateFromTask<IAnime>(DeleteAnimeAsync);
		SelectDirectory = ReactiveCommand.CreateFromTask(SelectDirectoryAsync);
	}

	[JsonConstructor]
	private AddViewModel() : this(
		Locator.Current.GetService<IScreen>()!,
		Locator.Current.GetService<ISongLoader>()!,
		Locator.Current.GetService<IMessageBoxManager>()!,
		Locator.Current.GetService<IEnumerable<IAnimeGatherer>>()!)
	{
	}

	private async Task AddAsync()
	{
		try
		{
			var gatherer = _Gatherers.Single(x => x.Name == SelectedGathererName);
			var model = await gatherer.GetAsync(Id, new
			(
				AddEndings: AddEndings,
				AddInserts: AddInserts,
				AddOpenings: AddOpenings,
				AddSongs: AddSongs
			)).ConfigureAwait(true);
			var file = await _Loader.SaveNewAsync(Directory!, model, _SaveNewOptions).ConfigureAwait(true);
			Anime.Add(new ObservableAnime(new Anime(file!, model, null)));
			Exception = null;
		}
		catch (Exception e)
		{
			Exception = e;
		}
	}

	private async Task DeleteAnimeAsync(IAnime anime)
	{
		var result = await _MessageBoxManager.ConfirmAsync(new()
		{
			Text = $"Are you sure you want to delete {anime.Name}?",
			Title = "Anime Deletion",
		}).ConfigureAwait(true);
		if (!result)
		{
			return;
		}

		Anime.Remove(anime);
		File.Delete(anime.AbsoluteInfoPath);
	}

	private async Task SelectDirectoryAsync()
	{
		var path = await _MessageBoxManager.GetDirectoryAsync(Directory).ConfigureAwait(true);
		if (string.IsNullOrWhiteSpace(path))
		{
			return;
		}

		Directory = path;
	}
}