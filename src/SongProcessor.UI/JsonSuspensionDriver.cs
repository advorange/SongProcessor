using ReactiveUI;

using SongProcessor.UI.ViewModels;

using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace SongProcessor.UI;

public class JsonSuspensionDriver(string Path) : ISuspensionDriver
{
	private static readonly JsonSerializerOptions _Options = new()
	{
		IgnoreReadOnlyFields = true,
		IgnoreReadOnlyProperties = true,
		WriteIndented = true,
		Converters =
		{
			new NavStackConverter(),
		},
		TypeInfoResolver = new DefaultJsonTypeInfoResolver
		{
			Modifiers =
			{
				UseDataContract,
				UseTypeNamesForViewModels,
				UseRoutingStateConstructor,
			}
		},
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
		var text = JsonSerializer.Serialize(state, _Options);
		File.WriteAllText(Path, text);
		return Observable.Return(Unit.Default);
	}

	private static void UseDataContract(JsonTypeInfo typeInfo)
	{
		if (typeInfo.Type.GetCustomAttribute<DataContractAttribute>() is null)
		{
			return;
		}

		// System.Text.Json does not automatically abide by DataContract/Member
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

	private static void UseRoutingStateConstructor(JsonTypeInfo typeInfo)
	{
		if (typeInfo.Type != typeof(RoutingState))
		{
			return;
		}

		// System.Text.Json does not consider a ctor with only optional parameters
		// to be a parameterless constructor
		typeInfo.CreateObject = () => new RoutingState();
	}

	private static void UseTypeNamesForViewModels(JsonTypeInfo typeInfo)
	{
		if (typeInfo.Type != typeof(IRoutableViewModel))
		{
			return;
		}

		// This actually is an improvement over Newtonsoft
		typeInfo.PolymorphismOptions = new()
		{
			DerivedTypes =
			{
				new(typeof(SongViewModel), "vm_song"),
				new(typeof(AddViewModel), "vm_add"),
			},
		};
	}

	private sealed class NavStackConverter : JsonConverter<ObservableCollection<IRoutableViewModel>>
	{
		public override ObservableCollection<IRoutableViewModel>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
			=> new(JsonSerializer.Deserialize<IRoutableViewModel[]>(ref reader, options) ?? []);

		public override void Write(Utf8JsonWriter writer, ObservableCollection<IRoutableViewModel> value, JsonSerializerOptions options)
		{
			writer.WriteStartArray();
			foreach (var vm in value)
			{
				if (vm is not EditViewModel)
				{
					JsonSerializer.Serialize(writer, vm, options);
				}
			}
			writer.WriteEndArray();
		}
	}
}