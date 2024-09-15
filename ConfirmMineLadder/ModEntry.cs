using GenericModConfigMenu;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using System.Linq;
using System.Net.NetworkInformation;

namespace ConfirmMineLadder
{
    public class ModEntry : Mod
    {
        public const string ConfirmationLevel_None = "None";
        public const string ConfirmationLevel_Dialog = "Dialog";
        public const string ConfirmationLevel_Warning = "Warning";

        /*********
        ** Properties
        *********/
        /// <summary>The mod configuration from the player.</summary>
        private ModConfig Config;

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            this.Config = this.Helper.ReadConfig<ModConfig>();

            Helper.Events.GameLoop.GameLaunched += (e, a) => OnGameLaunched(e, a);

            ObjectPatches.ModInstance = this;
            ObjectPatches.Config = this.Config;

            var harmony = new Harmony(this.ModManifest.UniqueID);
            // adjust behavior when player performs action in mines
            harmony.Patch(
                original: AccessTools.Method(typeof(MineShaft), nameof(MineShaft.checkAction)),
                prefix: new HarmonyMethod(typeof(ObjectPatches), nameof(ObjectPatches.Object_checkAction_Prefix))
            );
        }

        /// <summary>Add to Generic Mod Config Menu</summary>
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            // get Generic Mod Config Menu's API (if it's installed)
            var configMenu = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;

            // register mod
            configMenu.Register(
                mod: this.ModManifest,
                reset: () => this.Config = new ModConfig(),
                save: () => this.Helper.WriteConfig(this.Config)
            );

            // add config options
            configMenu.AddTextOption(
                mod: this.ModManifest,
                getValue: () => this.Config.ConfirmationLevelLadderUp,
                setValue: value => this.Config.ConfirmationLevelLadderUp = value,
                name: () => Helper.Translation.Get("Options_LadderUp"),
                allowedValues: new string[] {
                    ConfirmationLevel_None,
                    ConfirmationLevel_Dialog,
                    ConfirmationLevel_Warning
                },
                formatAllowedValue: value => Helper.Translation.Get($"Options_{value}")
            );
            configMenu.AddTextOption(
                mod: this.ModManifest,
                getValue: () => this.Config.ConfirmationLevelLadderDown,
                setValue: value => this.Config.ConfirmationLevelLadderDown = value,
                name: () => Helper.Translation.Get("Options_LadderDown"),
                allowedValues: new string[] {
                    ConfirmationLevel_None,
                    ConfirmationLevel_Dialog,
                    ConfirmationLevel_Warning
                },
                formatAllowedValue: value => Helper.Translation.Get($"Options_{value}")
            );
            configMenu.AddTextOption(
                mod: this.ModManifest,
                getValue: () => this.Config.ConfirmationLevelLadderDownSkullCavern,
                setValue: value => this.Config.ConfirmationLevelLadderDownSkullCavern = value,
                name: () => Helper.Translation.Get("Options_LadderDownSkullCavern"),
                allowedValues: new string[] {
                    ConfirmationLevel_None,
                    ConfirmationLevel_Dialog,
                    ConfirmationLevel_Warning
                },
                formatAllowedValue: value => Helper.Translation.Get($"Options_{value}")
            );
            configMenu.AddTextOption(
                mod: this.ModManifest,
                getValue: () => this.Config.ConfirmationLevelShaft,
                setValue: value => this.Config.ConfirmationLevelShaft = value,
                name: () => Helper.Translation.Get("Options_Shaft"),
                allowedValues: new string[] {
                    ConfirmationLevel_None,
                    ConfirmationLevel_Dialog,
                    ConfirmationLevel_Warning
                },
                formatAllowedValue: value => Helper.Translation.Get($"Options_{value}")
            );
        }
    }
}
