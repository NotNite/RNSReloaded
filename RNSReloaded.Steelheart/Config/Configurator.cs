using Reloaded.Mod.Interfaces;

namespace RNSReloaded.Steelheart.Config;

public class Configurator : IConfiguratorV3 {
    public string? ModFolder { get; private set; }
    public string? ConfigFolder { get; private set; }
    public ConfiguratorContext Context { get; private set; }
    public IUpdatableConfigurable[] Configurations => this.configurations ?? this.MakeConfigurations();

    private IUpdatableConfigurable[]? configurations;

    private IUpdatableConfigurable[] MakeConfigurations() {
        var filePath = Path.Combine(this.ConfigFolder!, "Config.json");
        const string configName = "Default Config";

        this.configurations = [Configurable<Config>.FromFile(filePath, configName)];

        // Add self-updating to configurations.
        for (var x = 0; x < this.Configurations.Length; x++) {
            var xCopy = x;
            this.Configurations[x].ConfigurationUpdated += configurable => {
                this.Configurations[xCopy] = configurable;
            };
        }

        return this.configurations;
    }

    public Configurator() { }
    public Configurator(string configDirectory) : this() {
        this.ConfigFolder = configDirectory;
    }

    public void Migrate(string oldDirectory, string newDirectory) { }

    public TType GetConfiguration<TType>(int index) => (TType) this.Configurations[index];

    public void SetConfigDirectory(string configDirectory) => this.ConfigFolder = configDirectory;

    public void SetContext(in ConfiguratorContext context) => this.Context = context;

    // ReSharper disable once CoVariantArrayConversion
    public IConfigurable[] GetConfigurations() => this.Configurations;

    public bool TryRunCustomConfiguration() => false;

    public void SetModDirectory(string modDirectory) {
        this.ModFolder = modDirectory;
    }
}
