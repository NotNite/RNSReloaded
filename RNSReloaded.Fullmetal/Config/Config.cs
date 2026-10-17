using System.ComponentModel;

namespace RNSReloaded.Fullmetal.Config;

public class Config : Configurable<Config> {
    public enum RouteConfig {
        // BASE GAME
        ScholarsNest,
        KingsArsenal,
        RedDarkhouse,
        ChurchmouseStreets,
        EmeraldLakeside,
        PaleKeep,
        // DLC
        SubterraSanctum,
        DarkhouseDepths,
        AtelierAurum,
        LoopingHallway,
        // RANDOM
        TrueRandom,
        ChaoticRandom,
        // FMEXPLUS
    }

    [DisplayName("Route")]
    [Description("Select route for Fullmetal here")]
    [DefaultValue(RouteConfig.PaleKeep)]
    public RouteConfig Route { get; set; } = RouteConfig.PaleKeep;
}
