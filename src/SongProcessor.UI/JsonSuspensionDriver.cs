using ReactiveUI;

using SongProcessor.UI.ViewModels;

using System.Reactive;
using System.Reactive.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace SongProcessor.UI;

public class JsonSuspensionDriver(string Path) : ISuspensionDriver
{
	private static readonly JsonSerializerOptions _Options = new()
	{
		IgnoreReadOnlyFields = true,
		IgnoreReadOnlyProperties = true,
		WriteIndented = true,
		TypeInfoResolver = new DefaultJsonTypeInfoResolver
		{
			Modifiers =
			{
				UseDataContract,
				UseTypeNamesForViewModels,
				IgnoreCertainViewModels,
			}
		}
	};
	public bool DeleteOnInvalidState { get; set; }

	public IObservable<Unit> InvalidateState()
	{
		if (DeleteOnInvalidState && File.Exists(Path))
		{
			File.Delete(Path);
		}
		return Observable.Return(Unit.Default);
	}

	public IObservable<object> LoadState()
	{
		// ReactiveUI relies on this method throwing an exception
		// to determine if CreateNewAppState should be called
		using var fs = File.OpenRead(Path);
		var state = JsonSerializer.Deserialize<MainViewModel>(fs, _Options);
		return Observable.Return(state)!;
	}

	public IObservable<Unit> SaveState(object state)
	{
		using var fs = File.Create(Path);
		JsonSerializer.Serialize(fs, state, _Options);
		return Observable.Return(Unit.Default);
	}

	private static void IgnoreCertainViewModels(JsonTypeInfo typeInfo)
	{
		if (typeInfo.Type != typeof(RoutingStateWorkaround))
		{
			return;
		}

		// This will be an issue if settings are serialized at any time other than
		// application shutdown
		typeInfo.OnSerializing = static obj =>
		{
			var navStack = ((RoutingState)obj).NavigationStack;
			for (var i = navStack.Count - 1; i >= 0; --i)
			{
				if (navStack[i].GetType() == typeof(EditViewModel))
				{
					navStack.RemoveAt(i);
				}
			}
		};
	}

	private static void UseDataContract(JsonTypeInfo typeInfo)
	{
		if (typeInfo.Type.GetCustomAttribute<DataContractAttribute>() is null)
		{
			return;
		}

		foreach (var propertyInfo in typeInfo.Properties)
		{
			if (propertyInfo.AttributeProvider is not ICustomAttributeProvider provider
				|| !provider.GetCustomAttributes(true).Any(x => x is DataMemberAttribute))
			{
				propertyInfo.ShouldSerialize = static (_, _)
					=> false;
			}
		}
	}

	private static void UseTypeNamesForViewModels(JsonTypeInfo typeInfo)
	{
		if (typeInfo.Type != typeof(IRoutableViewModel))
		{
			return;
		}

		typeInfo.PolymorphismOptions = new()
		{
			DerivedTypes =
			{
				new(typeof(SongViewModel), "vm_song"),
				new(typeof(AddViewModel), "vm_add"),
			},
		};
	}
}

public class RoutingStateWorkaround : RoutingState;