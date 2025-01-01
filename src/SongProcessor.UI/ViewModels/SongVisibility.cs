using ReactiveUI;

using SongProcessor.Models;

using System.Runtime.Serialization;

namespace SongProcessor.UI.ViewModels;

[DataContract]
public sealed class SongVisibility : ReactiveObject
{
	[DataMember]
	public bool IsExpanded
	{
		get;
		set => this.RaiseAndSetIfChanged(ref field, value);
	}
	[DataMember]
	public bool ShowCompletedSongs
	{
		get;
		set => this.RaiseAndSetIfChanged(ref field, value);
	} = true;
	[DataMember]
	public bool ShowIgnoredSongs
	{
		get;
		set => this.RaiseAndSetIfChanged(ref field, value);
	} = true;
	[DataMember]
	public bool ShowMissing480pSongs
	{
		get;
		set => this.RaiseAndSetIfChanged(ref field, value);
	} = true;
	[DataMember]
	public bool ShowMissing720pSongs
	{
		get;
		set => this.RaiseAndSetIfChanged(ref field, value);
	} = true;
	[DataMember]
	public bool ShowMissingMp3Songs
	{
		get;
		set => this.RaiseAndSetIfChanged(ref field, value);
	} = true;
	[DataMember]
	public bool ShowUnsubmittedSongs
	{
		get;
		set => this.RaiseAndSetIfChanged(ref field, value);
	} = true;

	public bool IsVisible(ISong song)
	{
		const Status COMPLETED = Status.Mp3 | Status.Res480 | Status.Res720;

		if (!ShowIgnoredSongs && song.ShouldIgnore)
		{
			return false;
		}
		if (!ShowUnsubmittedSongs && song.IsUnsubmitted())
		{
			return false;
		}

		return (ShowCompletedSongs && (song.Status & COMPLETED) == COMPLETED)
			|| (ShowMissingMp3Songs && (song.Status & Status.Mp3) == 0)
			|| (ShowMissing480pSongs && (song.Status & Status.Res480) == 0)
			|| (ShowMissing720pSongs && (song.Status & Status.Res720) == 0);
	}
}