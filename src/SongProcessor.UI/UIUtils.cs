﻿using SongProcessor.UI.ViewModels;

using System.Collections.Immutable;

namespace SongProcessor.UI;

public static class UIUtils
{
	public const int MESSAGE_BOX_HEIGHT = 133;
	public const int MESSAGE_BOX_WIDTH = 278;
	public const string NO = "No";
	public const string YES = "Yes";

	public static ImmutableArray<string> YesNo { get; } = [YES, NO];

	public static async Task<bool> ConfirmAsync(
		this IMessageBoxManager manager,
		MessageBoxViewModel<string> viewModel)
	{
		viewModel.Options = YesNo;
		var result = await manager.ShowAsync(viewModel).ConfigureAwait(true);
		return result == YES;
	}

	public static Task<string?> GetDirectoryAsync(
		this IMessageBoxManager manager,
		string? directory)
	{
		directory = Directory.Exists(directory)
			? directory!
			: Directory.GetCurrentDirectory();
		return manager.GetDirectoryAsync(directory, "Directory");
	}

	public static Task ShowExceptionAsync(this IMessageBoxManager manager, Exception e)
	{
		return manager.ShowNoResultAsync(new()
		{
			CanResize = true,
			Height = MESSAGE_BOX_HEIGHT * 5,
			Text = e.ToString(),
			Title = "An Exception Has Occurred",
		});
	}

	public static Task ShowNoResultAsync(
		this IMessageBoxManager manager,
		MessageBoxViewModel<object> viewModel)
		=> manager.ShowAsync(viewModel);
}