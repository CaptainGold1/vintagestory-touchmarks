using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Touchmarks.Behaviors;
using Touchmarks.Blocks;
using Touchmarks.Config;
using Touchmarks.Utils;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace Touchmarks
{
    public class TouchmarksModSystem : ModSystem
    {
        public static ILogger Logger;
        public static TouchmarksConfig config;
        public static readonly string modId = "touchmarks";
        public static readonly string harmonyId = "com.captaingold." + modId;

        private readonly Harmony harmony = new Harmony(harmonyId);

        // Called on server and client
        // Useful for registering block/entity classes on both sides
        public override void Start(ICoreAPI api)
        {
            Logger = Mod.Logger;
            Mod.Logger.Notification($"Loading Touchmarks on {api.Side} side");
            TryToLoadConfig(api);

            api.RegisterCollectibleBehaviorClass(modId + ":stampableBehavior", typeof(CollectibleBehaviorStampable));
            api.RegisterCollectibleBehaviorClass(modId + ":touchmarkBehavior", typeof(CollectibleBehaviorTouchmark));
            api.RegisterBlockClass(modId + ":BlockStampingTable", typeof(BlockStampingTable));
            api.RegisterBlockEntityClass(modId + ":BlockEntityStampingTable", typeof(BlockEntityStampingTable));

            // Will execute all unmarked patches, which should execute on both server and client
            harmony.PatchAllUncategorized();
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            // Should execute all server-side patches
            harmony.PatchCategory("Server");
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            // Should execute all client-side patches
            harmony.PatchCategory("Client");
        }

        public override void AssetsFinalize (ICoreAPI api) {
            base.AssetsFinalize(api);

            if (api.Side.IsClient()) return;

            GenerateConfigRegexesFromAssets(api);

            foreach (CollectibleObject coll in api.World.Collectibles.Where(coll => coll?.Code != null)) {
                if (TouchmarksUtils.IsCollectibleMetalStampable(coll)) {
                    if (config.debugMessages) {
                        Logger.Debug($"Registering stampable behavior to {coll.Code}");
                    }
                    coll.CollectibleBehaviors = ArrayExtensions.Append<CollectibleBehavior>(coll.CollectibleBehaviors, new CollectibleBehaviorStampable(coll));
                }

                if (coll is Item item && TouchmarksUtils.IsItemPersonalTouchmark(item)) {
                    if (config.debugMessages) {
                        Logger.Debug($"Registering touchmark behavior to {coll.Code}");
                    }
                    item.CollectibleBehaviors = ArrayExtensions.Append<CollectibleBehavior>(item.CollectibleBehaviors, new CollectibleBehaviorTouchmark(item));
                }
            }

            if (config.debugMessages) {
                Logger.Debug("Finished registering behaviors.");
            }
        }

        private void TryToSaveConfig(ICoreAPI api) {
            try {
                api.StoreModConfig(config, "Touchmarks.json");
            }
            catch (Exception e) {
                Mod.Logger.Error("Could not save config!");
                Mod.Logger.Error(e);
            }
        }

        private void TryToLoadConfig(ICoreAPI api) {
            try {
                config = api.LoadModConfig<TouchmarksConfig>("Touchmarks.json");
                if (config == null) {
                    config = new TouchmarksConfig();
                }
                api.StoreModConfig<TouchmarksConfig>(config, "Touchmarks.json");
            }
            catch (Exception e) {
                Mod.Logger.Error("Could not load config! Loading default settings.");
                Mod.Logger.Error(e);
                config = new TouchmarksConfig();
                api.StoreModConfig<TouchmarksConfig>(config, "Touchmarks.json");
            }
        }

        private void GenerateConfigRegexesFromAssets(ICoreAPI api) {
            if (config.regenerateRegexes) {
                Logger.Notification("Regenerating config regexes...");

                // Metal stampable items
                var metalStampableMatchAnyAssets = api.Assets.GetMany<List<string>>(Logger, "config/touchmarks/regex/metalstampable/matchany");
                config.stampableMetalItemsMatchAny = ConfigUtils.GetMatchAnyRegexString(ref metalStampableMatchAnyAssets);

                var metalStampableMatchStartAssets = api.Assets.GetMany<List<string>>(Logger, "config/touchmarks/regex/metalstampable/matchstart");
                config.stampableMetalItemsMatchStart = ConfigUtils.GetMatchStartRegexString(ref metalStampableMatchStartAssets);

                var metalStampableExcludeAnyAssets = api.Assets.GetMany<List<string>>(Logger, "config/touchmarks/regex/metalstampable/excludeany");
                config.stampableMetalItemsExcludeAny = ConfigUtils.GetMatchAnyRegexString(ref metalStampableExcludeAnyAssets);

                if (config.printGeneratedRegex) {
                    Logger.Debug($"stampableMetalItemsMatchAny: ${config.stampableMetalItemsMatchAny}");
                    Logger.Debug($"stampableMetalItemsMatchStart: ${config.stampableMetalItemsMatchStart}");
                    Logger.Debug($"stampableMetalItemsExcludeAny: ${config.stampableMetalItemsExcludeAny}");
                }

                TryToSaveConfig(api);
            }
        }
    }
}
