using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using PingUI.Extensions;
using PingUI.I18N;
using PingUI.Models;
using PingUI.ServiceModels;
using Splat;

namespace PingUI.Services;

/// <summary>
/// A configuration that is stored next to the executable or in special folder <see cref="Environment.SpecialFolder.ApplicationData" /> in JSON format.
/// </summary>
public partial class LocalOrAppDataJsonConfiguration : IConfiguration
{
	/// <summary>
	/// The name of the configuration file.
	/// </summary>
	public const string ConfigurationFileName = "config.json";

	/// <summary>
	/// The path to the configuration file in ApplicationData.
	/// </summary>
	public static readonly string AppDataConfigurationPath = Path.Combine(
		Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
		typeof(LocalOrAppDataJsonConfiguration).Assembly.GetCustomAttribute<AssemblyTitleAttribute>()!.Title,
		ConfigurationFileName);

	/// <summary>
	/// The path to the configuration file in the local folder.
	/// </summary>
	public static readonly string LocalConfigurationPath = Path.Combine(AppContext.BaseDirectory, ConfigurationFileName);

	/// <summary>
	/// A helper object for serializing and deserializing to JSON.
	/// </summary>
	private static readonly JsonContext jsonContext = new(
		new JsonSerializerOptions(JsonSerializerDefaults.Web)
		{
			PreferredObjectCreationHandling = JsonObjectCreationHandling.Populate
		});

	/// <inheritdoc />
	public ObservableCollection<Target> Targets
	{
		get;
	} = [];

	/// <summary>
	/// The absolute path to this configuration file's backing store.
	/// </summary>
	[JsonIgnore]
	public string ConfigurationPath
	{
		get;
		private set;
	} = AppDataConfigurationPath;

	/// <inheritdoc />
	public bool Topmost
	{
		get;
		set;
	}

	/// <inheritdoc />
	public bool? CheckOnlineForUpdates
	{
		get;
		set;
	}

	/// <summary>
	/// Loads the configuration from the stored JSON or returns a new <see cref="LocalOrAppDataJsonConfiguration" /> if the file does not exist or a loading error occurs.
	/// </summary>
	/// <returns>A <see cref="LocalOrAppDataJsonConfiguration" /> value.</returns>
	public static LocalOrAppDataJsonConfiguration LoadOrDefault()
	{
		if (TryLoadFile(LocalConfigurationPath, out var config))
		{
			return config;
		}
		if (TryLoadFile(AppDataConfigurationPath, out config))
		{
			return config;
		}
		return new LocalOrAppDataJsonConfiguration();

		static bool TryLoadFile(string path, [NotNullWhen(true)] out LocalOrAppDataJsonConfiguration? config)
		{
			if (File.Exists(path))
			{
				try
				{
					using var file = File.OpenRead(path);
					config = JsonSerializer.Deserialize(file, jsonContext.LocalOrAppDataJsonConfiguration);
					if (config is not null)
					{
						config.ConfigurationPath = path;
					}
					return config is not null;
				}
				catch (Exception exception)
				{
					Locator.Current.GetRequiredService<IErrorReporter>().ReportError(string.Format(Strings.LocalOrAppDataJsonConfiguration_Error_LoadFile, path), exception);
				}
			}
			config = null;
			return false;
		}
	}

	/// <inheritdoc />
	public void Save()
	{
		new FileInfo(ConfigurationPath).Directory?.Create();
		using var file = File.Open(ConfigurationPath, FileMode.Create);
		JsonSerializer.Serialize(file, this, jsonContext.LocalOrAppDataJsonConfiguration);
	}

	/// <summary>
	/// Helper class for serializing and deserializing to JSON.
	/// </summary>
	[JsonSerializable(typeof(LocalOrAppDataJsonConfiguration))]
	internal partial class JsonContext : JsonSerializerContext
	{
	}
}
