using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using xTile.Dimensions;
using static StardewValley.GameLocation;

namespace ConfirmMineLadder
{
    internal class ObjectPatches
    {
        // initialized by ModEntry.cs
        public static ModEntry ModInstance;
        public static ModConfig Config;

        private static MineShaft? currentMineShaft;

        public static bool Object_checkAction_Prefix(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who, MineShaft __instance)
        {
            // If we should do something differently, then do so and return false (skips base game function)
            // Otherwise, return true (runs base game function normally)
            try
            {
                ModInstance.Monitor.Log("[Confirm Mine Ladder] Checking action", LogLevel.Trace);

                // Perform same context checks as base game function
                if (!Game1.player.IsLocalPlayer)
                {
                    ModInstance.Monitor.Log("[Confirm Mine Ladder] Player is not local", LogLevel.Trace);
                    return true;
                }

                var tileIndex = __instance.getTileIndexAt(tileLocation, "Buildings");
                ModInstance.Monitor.Log($"[Confirm Mine Ladder] Tile index = {tileIndex}", LogLevel.Trace);
                switch (tileIndex)
                {
                    case 115: // ladder up
                        ModInstance.Monitor.Log($"[Confirm Mine Ladder] Ladder up, confirmation level {Config.ConfirmationLevelLadderUp}", LogLevel.Debug);
                        switch (Config.ConfirmationLevelLadderUp)
                        {
                            case ModEntry.ConfirmationLevel_None:
                                currentMineShaft = __instance;
                                ladderUpConfirmed(who);
                                return false;
                            case ModEntry.ConfirmationLevel_Dialog:
                                return true;
                            case ModEntry.ConfirmationLevel_Warning:
                                currentMineShaft = __instance;
                                // this creates a mid-screen OK/cancel dialog, e.g. "are you sure you want to leave the festival?"
                                Game1.activeClickableMenu = new ConfirmationDialog(
                                    message: ModInstance.Helper.Translation.Get("Question_LadderUp"),
                                    onConfirm: ladderUpConfirmed,
                                    onCancel: actionCanceled
                                );
                                return false;
                        }
                        break;

                    case 173: // ladder down
                        ModInstance.Monitor.Log($"[Confirm Mine Ladder] Ladder down, confirmation level {Config.ConfirmationLevelLadderDown}", LogLevel.Debug);
                        var behavior = (__instance.mineLevel > 120)
                            ? Config.ConfirmationLevelLadderDownSkullCavern
                            : Config.ConfirmationLevelLadderDown;
                        switch (behavior)
                        {
                            case ModEntry.ConfirmationLevel_None:
                                return true;
                            case ModEntry.ConfirmationLevel_Dialog:
                                currentMineShaft = __instance;
                                __instance.createQuestionDialogue(
                                    question: ModInstance.Helper.Translation.Get("Question_LadderDown"),
                                    answerChoices: __instance.createYesNoResponses(),
                                    afterDialogueBehavior: new afterQuestionBehavior(ladderDownResponse)
                                );
                                return false;
                            case ModEntry.ConfirmationLevel_Warning:
                                currentMineShaft = __instance;
                                Game1.activeClickableMenu = new ConfirmationDialog(
                                    message: ModInstance.Helper.Translation.Get("Question_LadderDown"),
                                    onConfirm: ladderDownConfirmed,
                                    onCancel: actionCanceled
                                );
                                return false;
                        }
                        break;

                    case 174: // shaft
                        ModInstance.Monitor.Log($"[Confirm Mine Ladder] Shaft, confirmation level {Config.ConfirmationLevelShaft}", LogLevel.Debug);
                        switch (Config.ConfirmationLevelShaft)
                        {
                            case ModEntry.ConfirmationLevel_None:
                                shaftConfirmed(who);
                                return false;
                            case ModEntry.ConfirmationLevel_Dialog:
                                return true;
                            case ModEntry.ConfirmationLevel_Warning:
                                currentMineShaft = __instance;
                                Game1.activeClickableMenu = new ConfirmationDialog(
                                    message: ModInstance.Helper.Translation.Get("Question_Shaft"),
                                    onConfirm: shaftConfirmed,
                                    onCancel: actionCanceled
                                );
                                return false;
                        }
                        break;

                }

                return true;
            }
            catch (Exception ex)
            {
                ModInstance.Monitor.Log($"[Confirm Mine Ladder] Object_performUseAction_Prefix: {ex.Message} - {ex.StackTrace}", LogLevel.Error);
                return true;
            }
        }

        private static void ladderUpConfirmed(Farmer who)
        {
            // Reset UI state
            Game1.exitActiveMenu();

            // If we lost track of mod state, then give up and exit
            if (currentMineShaft == null)
            {
                ModInstance.Monitor.Log($"[Confirm Mine Ladder] Ignoring confirmation (lost track of ladder)", LogLevel.Error);
                return;
            }

            // Trigger same behavior as if player confirmed base game dialog
            currentMineShaft.answerDialogueAction(
                questionAndAnswer: "ExitMine_Leave",
                questionParams: null // unused for this action
            );
        }

        private static void ladderDownResponse(Farmer who, string responseKey)
        {
            if (responseKey.Contains("Yes"))
            {
                ladderDownConfirmed(who);
            }
            else
            {
                actionCanceled(who);
                who.canMove = true; // otherwise they get stuck
            }
        }

        private static void ladderDownConfirmed(Farmer who)
        {
            // Reset UI state
            Game1.exitActiveMenu();

            // If we lost track of mod state, then give up and exit
            if (currentMineShaft == null)
            {
                ModInstance.Monitor.Log($"[Confirm Mine Ladder] Ignoring confirmation (lost track of ladder)", LogLevel.Error);
                return;
            }

            // Trigger same behavior as if player confirmed base game dialog
            Game1.enterMine(currentMineShaft.mineLevel + 1);
            currentMineShaft.playSound("stairsdown");
        }

        private static void shaftConfirmed(Farmer who)
        {
            // Reset UI state
            Game1.exitActiveMenu();

            // If we lost track of mod state, then give up and exit
            if (currentMineShaft == null)
            {
                ModInstance.Monitor.Log($"[Confirm Mine Ladder] Ignoring confirmation (lost track of shaft)", LogLevel.Error);
                return;
            }

            // Trigger same behavior as if player confirmed base game dialog
            currentMineShaft.answerDialogueAction(
                questionAndAnswer: "Shaft_Jump",
                questionParams: null // unused for this action
            );
        }

        private static void actionCanceled(Farmer who)
        {
            ModInstance.Monitor.Log($"[Confirm Mine Ladder] Action was canceled", LogLevel.Debug);

            // Reset UI state
            Game1.exitActiveMenu();

            // Reset mod state
            currentMineShaft = null;
        }

    }
}
