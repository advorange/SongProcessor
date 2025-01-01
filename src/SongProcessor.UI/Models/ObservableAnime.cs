using ReactiveUI;

using SongProcessor.FFmpeg;
using SongProcessor.Models;

using System.Collections.ObjectModel;
using System.Diagnostics;

namespace SongProcessor.UI.Models;

[DebuggerDisplay(ModelUtils.DEBUGGER_DISPLAY)]
public sealed class ObservableAnime : ReactiveObject, IAnime
{
	public string AbsoluteInfoPath
	{
		get;
		set => this.RaiseAndSetIfChanged(ref field, value);
	}
	public int Id
	{
		get;
		set => this.RaiseAndSetIfChanged(ref field, value);
	}
	public bool IsExpanded
	{
		get;
		set => this.RaiseAndSetIfChanged(ref field, value);
	}
	public bool IsExpanderVisible
	{
		get;
		set => this.RaiseAndSetIfChanged(ref field, value);
	}
	public bool IsVisible
	{
		get;
		set => this.RaiseAndSetIfChanged(ref field, value);
	}
	public string Name
	{
		get;
		set => this.RaiseAndSetIfChanged(ref field, value);
	}
	public ObservableCollection<ObservableSong> Songs
	{
		get;
		set => this.RaiseAndSetIfChanged(ref field, value);
	}
	public string? Source => this.GetRelativeOrAbsoluteSourceFile();
	public VideoInfo? VideoInfo
	{
		get;
		set
		{
			this.RaiseAndSetIfChanged(ref field, value);
			this.RaisePropertyChanged(nameof(Source));
		}
	}
	public int Year
	{
		get;
		set => this.RaiseAndSetIfChanged(ref field, value);
	}
	IReadOnlyList<ISong> IAnimeBase.Songs => Songs;
	private string DebuggerDisplay => Name;

	public ObservableAnime(IAnime anime)
	{
		AbsoluteInfoPath = anime.AbsoluteInfoPath;
		Id = anime.Id;
		Name = anime.Name;
		var songs = anime.Songs.Select(x => new ObservableSong(this, x));
		Songs = new SortedObservableCollection<ObservableSong>(SongComparer.Instance, songs);
		IsExpanderVisible = Songs.Count > 0;
		VideoInfo = anime.VideoInfo;
		Year = anime.Year;
	}
}