using Newtonsoft.Json;

using ReactiveUI;
using ReactiveUI.Validation.Abstractions;
using ReactiveUI.Validation.Contexts;
using ReactiveUI.Validation.Extensions;

using SongProcessor.Models;
using SongProcessor.UI.Models;
using SongProcessor.Utils;

using System.Reactive;
using System.Reactive.Linq;

namespace SongProcessor.UI.ViewModels;

// Never serialize this view/viewmodel since this data is related to folder structure
[JsonConverter(typeof(NewtonsoftJsonSkipThis))]
public sealed class EditViewModel : ReactiveObject, IRoutableViewModel, IValidatableViewModel
{
	private readonly ObservableAnime _Anime;
	private readonly ISongLoader _Loader;
	private readonly IMessageBoxManager _MessageBoxManager;
	private readonly ObservableSong _Song;

	public static IReadOnlyList<AspectRatio> AspectRatios { get; } =
	[
		default,
		new AspectRatio(4, 3),
		new AspectRatio(16, 9),
	];
	public static IReadOnlyList<SongType> SongTypes { get; } =
	[
		SongType.Opening,
		SongType.Ending,
		SongType.Insert,
	];

	public string Artist
	{
		get;
		set => this.RaiseAndSetIfChanged(ref field, value);
	}
	public AspectRatio AspectRatio
	{
		get;
		set => this.RaiseAndSetIfChanged(ref field, value);
	}
	public int AudioTrack
	{
		get;
		set => this.RaiseAndSetIfChanged(ref field, value);
	}
	public string ButtonText
	{
		get;
		set => this.RaiseAndSetIfChanged(ref field, value);
	} = "Save";
	public string CleanPath
	{
		get;
		set => this.RaiseAndSetIfChanged(ref field, value);
	}
	public string End
	{
		get;
		set => this.RaiseAndSetIfChanged(ref field, value);
	}
	public int Episode
	{
		get;
		set => this.RaiseAndSetIfChanged(ref field, value);
	}
	public bool Has480p
	{
		get;
		set => this.RaiseAndSetIfChanged(ref field, value);
	}
	public bool Has720p
	{
		get;
		set => this.RaiseAndSetIfChanged(ref field, value);
	}
	public bool HasMp3
	{
		get;
		set => this.RaiseAndSetIfChanged(ref field, value);
	}
	public IScreen HostScreen { get; }
	public bool IsSubmitted
	{
		get;
		set => this.RaiseAndSetIfChanged(ref field, value);
	}
	public string Name
	{
		get;
		set => this.RaiseAndSetIfChanged(ref field, value);
	}
	public bool ShouldIgnore
	{
		get;
		set => this.RaiseAndSetIfChanged(ref field, value);
	}
	public int SongPosition
	{
		get;
		set => this.RaiseAndSetIfChanged(ref field, value);
	}
	public SongType SongType
	{
		get;
		set => this.RaiseAndSetIfChanged(ref field, value);
	}
	public string Start
	{
		get;
		set => this.RaiseAndSetIfChanged(ref field, value);
	}
	public string UrlPathSegment => "/edit";
	public ValidationContext ValidationContext { get; } = new();
	public int VideoTrack
	{
		get;
		set => this.RaiseAndSetIfChanged(ref field, value);
	}
	public int VolumeModifier
	{
		get;
		set => this.RaiseAndSetIfChanged(ref field, value);
	}

	#region Commands
	public ReactiveCommand<Unit, Unit> Save { get; }
	public ReactiveCommand<Unit, Unit> SelectCleanPath { get; }
	#endregion Commands

	public EditViewModel(
		IScreen screen,
		ISongLoader loader,
		IMessageBoxManager messageBoxManager,
		ObservableSong song)
	{
		HostScreen = screen ?? throw new ArgumentNullException(nameof(screen));
		_Song = song ?? throw new ArgumentNullException(nameof(song));
		_Anime = song.Parent ?? throw new ArgumentException("Parent cannot be null.", nameof(song));
		_Loader = loader ?? throw new ArgumentNullException(nameof(loader));
		_MessageBoxManager = messageBoxManager ?? throw new ArgumentNullException(nameof(messageBoxManager));

		Artist = _Song.Artist;
		AspectRatio = _Song.OverrideAspectRatio ?? AspectRatios[0];
		AudioTrack = _Song.OverrideAudioTrack;
		CleanPath = _Song.CleanPath!;
		End = _Song.End.ToString();
		Episode = _Song.Episode ?? 0;
		Has480p = !song.IsMissing(Status.Res480);
		Has720p = !song.IsMissing(Status.Res720);
		HasMp3 = !song.IsMissing(Status.Mp3);
		IsSubmitted = song.Status != Status.NotSubmitted;
		Name = _Song.Name;
		ShouldIgnore = _Song.ShouldIgnore;
		SongPosition = _Song.Type.Position ?? 0;
		SongType = _Song.Type.Type;
		Start = _Song.Start.ToString();
		VideoTrack = _Song.OverrideVideoTrack;
		VolumeModifier = (int)(_Song.VolumeModifier?.Value ?? 0);

		this.ValidationRule(
			x => x.Artist,
			x => !string.IsNullOrWhiteSpace(x),
			"Artist must not be null or empty.");
		this.ValidationRule(
			x => x.CleanPath,
			x => string.IsNullOrEmpty(x) || File.Exists(Path.Combine(_Anime.GetDirectory(), x)),
			"Clean path must be null, empty, or lead to an existing file.");
		this.ValidationRule(
			x => x.Name,
			x => !string.IsNullOrWhiteSpace(x),
			"Name must not be null or empty.");

		var validTimes = this.WhenAnyValue(
			x => x.Start,
			x => x.End,
			(start, end) => new
			{
				ValidStart = TimeSpan.TryParse(start, out var s),
				Start = s,
				ValidEnd = TimeSpan.TryParse(end, out var e),
				End = e,
			})
			.Select(x => x.ValidStart && x.ValidEnd && x.Start <= x.End);
		this.ValidationRule(
			validTimes,
			"Invalid times supplied or start is less than end.");

		ValidationContext.ValidationStatusChange.Subscribe(
			x => ButtonText = x.IsValid ? "Save" : x.Text.ToSingleLine(" "));

		Save = ReactiveCommand.CreateFromTask(SaveAsync, this.IsValid());
		SelectCleanPath = ReactiveCommand.CreateFromTask(SelectCleanPathAsync);
	}

	private static AspectRatio? GetAspectRatio(AspectRatio ratio)
		=> ratio.Width == 0 || ratio.Height == 0 ? null : ratio;

	private static string? GetNullIfEmpty(string str)
		=> string.IsNullOrEmpty(str) ? null : str;

	private static int? GetNullIfZero(int? num)
		=> num == 0 ? null : num;

	private static VolumeModifer? GetVolumeModifer(int? num)
		=> GetNullIfZero(num) is null ? null : VolumeModifer.FromDecibels(num!.Value);

	private Status GetStatus()
	{
		var status = Status.NotSubmitted;
		if (HasMp3)
		{
			status |= Status.Mp3;
		}
		if (Has480p)
		{
			status |= Status.Res480;
		}
		if (Has720p)
		{
			status |= Status.Res720;
		}
		if (IsSubmitted)
		{
			status |= Status.Submitted;
		}
		return status;
	}

	private async Task SaveAsync()
	{
		_Song.Artist = Artist;
		_Song.OverrideAspectRatio = GetAspectRatio(AspectRatio);
		_Song.OverrideAudioTrack = AudioTrack;
		_Song.CleanPath = FileUtils.GetRelativeOrAbsoluteFile(_Anime.GetDirectory(), GetNullIfEmpty(CleanPath));
		_Song.End = TimeSpan.Parse(End);
		_Song.Episode = GetNullIfZero(Episode);
		_Song.Name = Name;
		_Song.Type = new(SongType, GetNullIfZero(SongPosition));
		_Song.ShouldIgnore = ShouldIgnore;
		_Song.Status = GetStatus();
		_Song.Start = TimeSpan.Parse(Start);
		_Song.OverrideVideoTrack = VideoTrack;
		_Song.VolumeModifier = GetVolumeModifer(VolumeModifier);

		await _Loader.SaveAsync(_Anime).ConfigureAwait(false);
	}

	private async Task SelectCleanPathAsync()
	{
		var dir = _Anime.GetDirectory();
		var result = await _MessageBoxManager.GetFilesAsync(dir, "Clean Path", false).ConfigureAwait(true);
		if (result.SingleOrDefault() is not string file)
		{
			return;
		}

		CleanPath = file;
	}
}