using ReactiveUI;

using SongProcessor.Models;

using System.Diagnostics;

namespace SongProcessor.UI.Models;

[DebuggerDisplay(ModelUtils.DEBUGGER_DISPLAY)]
public sealed class ObservableSong : ReactiveObject, ISong
{
	public HashSet<int> AlsoIn
	{
		get;
		set => this.RaiseAndSetIfChanged(ref field, value);
	} = null!;
	public string Artist
	{
		get;
		set => this.RaiseAndSetIfChanged(ref field, value);
	} = null!;
	public string? CleanPath
	{
		get;
		set => this.RaiseAndSetIfChanged(ref field, value);
	}
	public TimeSpan End
	{
		get;
		set => this.RaiseAndSetIfChanged(ref field, value);
	}
	public int? Episode
	{
		get;
		set => this.RaiseAndSetIfChanged(ref field, value);
	}
	public bool IsVisible
	{
		get;
		set => this.RaiseAndSetIfChanged(ref field, value);
	} = true;
	public string Name
	{
		get;
		set => this.RaiseAndSetIfChanged(ref field, value);
	} = null!;
	public AspectRatio? OverrideAspectRatio
	{
		get;
		set => this.RaiseAndSetIfChanged(ref field, value);
	}
	public int OverrideAudioTrack
	{
		get;
		set => this.RaiseAndSetIfChanged(ref field, value);
	}
	public int OverrideVideoTrack
	{
		get;
		set => this.RaiseAndSetIfChanged(ref field, value);
	}
	public ObservableAnime Parent { get; }
	public bool ShouldIgnore
	{
		get;
		set => this.RaiseAndSetIfChanged(ref field, value);
	}
	public TimeSpan Start
	{
		get;
		set => this.RaiseAndSetIfChanged(ref field, value);
	}
	public Status Status
	{
		get;
		set => this.RaiseAndSetIfChanged(ref field, value);
	}
	public SongTypeAndPosition Type
	{
		get;
		set => this.RaiseAndSetIfChanged(ref field, value);
	}
	public VolumeModifer? VolumeModifier
	{
		get;
		set => this.RaiseAndSetIfChanged(ref field, value);
	}
	IReadOnlySet<int> ISong.AlsoIn => AlsoIn;
	private string DebuggerDisplay => this.GetFullName();

	public ObservableSong(ObservableAnime parent, ISong other)
	{
		Parent = parent;
		AlsoIn = new(other.AlsoIn);
		Artist = other.Artist;
		CleanPath = other.CleanPath;
		End = other.End;
		Episode = other.Episode;
		Name = other.Name;
		OverrideAspectRatio = other.OverrideAspectRatio;
		OverrideAudioTrack = other.OverrideAudioTrack;
		OverrideVideoTrack = other.OverrideVideoTrack;
		ShouldIgnore = other.ShouldIgnore;
		Start = other.Start;
		Status = other.Status;
		Type = other.Type;
		VolumeModifier = other.VolumeModifier;
	}
}