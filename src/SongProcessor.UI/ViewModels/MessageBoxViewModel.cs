using ReactiveUI;

using SongProcessor.UI.Views;

using System.Reactive;
using System.Reactive.Linq;

namespace SongProcessor.UI.ViewModels;

public sealed class MessageBoxViewModel<T> : ReactiveObject
{
	public string? ButtonText
	{
		get;
		set => this.RaiseAndSetIfChanged(ref field, value);
	} = "Ok";
	public bool CanResize
	{
		get;
		set => this.RaiseAndSetIfChanged(ref field, value);
	}
	public T? CurrentOption
	{
		get;
		set => this.RaiseAndSetIfChanged(ref field, value);
	}
	public bool HasOptions
	{
		get;
		private set => this.RaiseAndSetIfChanged(ref field, value);
	}
	public int Height
	{
		get;
		set => this.RaiseAndSetIfChanged(ref field, value);
	} = UIUtils.MESSAGE_BOX_HEIGHT;
	public IEnumerable<T>? Options
	{
		get;
		set
		{
			this.RaiseAndSetIfChanged(ref field, value);
			CurrentOption = default!;
			HasOptions = value?.Any() ?? false;
			ButtonText = HasOptions ? "Confirm" : "Ok";
		}
	}
	public string? Text
	{
		get;
		set => this.RaiseAndSetIfChanged(ref field, value);
	}
	public string? Title
	{
		get;
		set => this.RaiseAndSetIfChanged(ref field, value);
	}
	public int Width
	{
		get;
		set => this.RaiseAndSetIfChanged(ref field, value);
	} = UIUtils.MESSAGE_BOX_WIDTH;

	#region Commands
	public ReactiveCommand<MessageBox, Unit> Escape { get; }
	public ReactiveCommand<MessageBox, Unit> Ok { get; }
	#endregion Commands

	public MessageBoxViewModel()
	{
		Escape = ReactiveCommand.Create<MessageBox>(x => x.Close());

		var canClose = this.WhenAnyValue(
			x => x.CurrentOption!,
			x => x.Options,
			(current, all) => new
			{
				Current = current,
				All = all,
			})
			.Select(x => x.All is null || !Equals(x.Current, default));
		Ok = ReactiveCommand.Create<MessageBox>(
			x => x.Close(CurrentOption),
			canClose
		);
	}
}