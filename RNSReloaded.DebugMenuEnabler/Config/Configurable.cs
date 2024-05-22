using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Reloaded.Mod.Interfaces;

namespace RNSReloaded.DebugMenuEnabler.Config;

// ReSharper disable StaticMemberInGenericType
public class Configurable<TParentType> : IUpdatableConfigurable where TParentType : Configurable<TParentType>, new() {
    public static JsonSerializerOptions SerializerOptions { get; } = new JsonSerializerOptions() {
        Converters = {new JsonStringEnumConverter()},
        WriteIndented = true
    };

    [Browsable(false)] public event Action<IUpdatableConfigurable>? ConfigurationUpdated;
    [JsonIgnore] [Browsable(false)] public string? FilePath { get; private set; }
    [JsonIgnore] [Browsable(false)] public string? ConfigName { get; private set; }
    [JsonIgnore] [Browsable(false)] private FileSystemWatcher? ConfigWatcher { get; set; }

    private void Initialize(string filePath, string configName) {
        this.FilePath = filePath;
        this.ConfigName = configName;

        this.MakeConfigWatcher();
        this.Save = this.OnSave;
    }

    public void DisposeEvents() {
        this.ConfigWatcher?.Dispose();
        this.ConfigurationUpdated = null;
    }

    [JsonIgnore] [Browsable(false)] public Action? Save { get; private set; }
    [Browsable(false)] private static readonly object ReadLock = new object();

    public static TParentType FromFile(string filePath, string configName) => ReadFrom(filePath, configName);

    private void MakeConfigWatcher() {
        this.ConfigWatcher =
            new FileSystemWatcher(Path.GetDirectoryName(this.FilePath)!, Path.GetFileName(this.FilePath)!);
        this.ConfigWatcher.Changed += (_, _) => this.OnConfigurationUpdated();
        this.ConfigWatcher.EnableRaisingEvents = true;
    }

    private void OnConfigurationUpdated() {
        lock (ReadLock) {
            // Note: External program might still be writing to file while this is being executed, so we need to keep retrying.
            var newConfig = TryGetValue(() => ReadFrom(this.FilePath!, this.ConfigName!), 250, 2);
            newConfig.ConfigurationUpdated = this.ConfigurationUpdated;

            this.DisposeEvents();

            newConfig.ConfigurationUpdated?.Invoke(newConfig);
        }
    }

    private void OnSave() {
        var parent = (TParentType) this;
        File.WriteAllText(this.FilePath!, JsonSerializer.Serialize(parent, SerializerOptions));
    }

    private static TParentType ReadFrom(string filePath, string configName) {
        var result = (File.Exists(filePath)
                          ? JsonSerializer.Deserialize<TParentType>(File.ReadAllBytes(filePath), SerializerOptions)
                          : new TParentType()) ?? new TParentType();

        result.Initialize(filePath, configName);
        return result;
    }

    private static T TryGetValue<T>(Func<T> getValue, int timeout, int sleepTime, CancellationToken token = default)
        where T : new() {
        var watch = new Stopwatch();
        watch.Start();
        var valueSet = false;
        var value = new T();

        while (watch.ElapsedMilliseconds < timeout) {
            if (token.IsCancellationRequested)
                return value;

            try {
                value = getValue();
                valueSet = true;
                break;
            } catch (Exception) {
                // ignored
            }

            Thread.Sleep(sleepTime);
        }

        if (valueSet == false)
            throw new Exception($"Timeout limit {timeout} exceeded.");

        return value;
    }
}
