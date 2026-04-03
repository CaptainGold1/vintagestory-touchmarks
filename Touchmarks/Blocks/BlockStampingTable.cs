using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Touchmarks.Utils;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Util;
using Vintagestory.Common.Collectible.Block;
using Vintagestory.GameContent;

namespace Touchmarks.Blocks {
    internal class BlockStampingTable : Block {
        WorldInteraction[] interactions;
        public override void OnLoaded(ICoreAPI api) {
            base.OnLoaded(api);

            if (api.Side != EnumAppSide.Client) return;
            ICoreClientAPI clientAPI = api as ICoreClientAPI;

            interactions = ObjectCacheUtil.GetOrCreate(api, "stampingTableInteractions", () => {
                List<ItemStack> stampableList = new List<ItemStack>();
                List<ItemStack> handStampsList = new List<ItemStack>();

                foreach (Item item in api.World.Items) {
                    if (item.Code == null) continue;

                    if (TouchmarksUtils.IsCollectibleMetalStampable(item)) stampableList.Add(new ItemStack(item));
                    if (TouchmarksUtils.IsItemPersonalTouchmark(item)) handStampsList.Add(new ItemStack(item));
                }

                return new WorldInteraction[] {
                    new WorldInteraction() {
                        // TODO: Replace with mod code from modsystem
                        ActionLangCode = "touchmarks:blockhelp-stampingtable-additem",
                        HotKeyCode = "shift",
                        MouseButton = EnumMouseButton.Right,
                        Itemstacks = stampableList.ToArray(),
                        GetMatchingStacks = (wi, bs, es) => {
                            BlockEntityStampingTable blockEntityStampingTable = api.World.BlockAccessor.GetBlockEntity(bs.Position) as BlockEntityStampingTable;
                            if (blockEntityStampingTable != null && blockEntityStampingTable.stampingStack != null) {
                                return null;
                            }
                            return stampableList.ToArray();
                        }
                    },

                    new WorldInteraction() {
                        ActionLangCode = "touchmarks:blockhelp-stampingtable-removeitem",
                        HotKeyCode = null,
                        MouseButton = EnumMouseButton.Right,
                        Itemstacks = stampableList.ToArray(),
                        GetMatchingStacks = (wi, bs, es) => {
                            BlockEntityStampingTable blockEntityStampingTable = api.World.BlockAccessor.GetBlockEntity(bs.Position) as BlockEntityStampingTable;
                            if (blockEntityStampingTable != null && blockEntityStampingTable.stampingStack != null) {
                                return new ItemStack[] { blockEntityStampingTable.stampingStack };
                            }
                            return null;
                        }
                    },

                    new WorldInteraction() {
                        ActionLangCode = "touchmarks:blockhelp-stampingtable-stampitem",
                        HotKeyCode = null,
                        MouseButton = EnumMouseButton.Right,
                        Itemstacks = handStampsList.ToArray(),
                        GetMatchingStacks = (wi, bs, es) => {
                            BlockEntityStampingTable blockEntityStampingTable = api.World.BlockAccessor.GetBlockEntity(bs.Position) as BlockEntityStampingTable;
                            if (blockEntityStampingTable != null && blockEntityStampingTable.stampingStack != null) {
                                return handStampsList.ToArray();
                            }
                            return null;
                        }
                    }
                };
            });
        }

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel) {
            BlockEntityStampingTable stampingTableEntity = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BlockEntityStampingTable;

            var playerControls = byPlayer.Entity.Controls;
            var playerSlot = byPlayer.InventoryManager.ActiveHotbarSlot;

            if (stampingTableEntity != null) {
                if (!playerControls.ShiftKey) {
                    if (playerSlot.Itemstack != null &&
                        TouchmarksUtils.IsItemAssignedPersonalTouchmark(playerSlot.Itemstack) &&
                        stampingTableEntity.stampingStack != null
                    ) {
                        if (!(byPlayer.InventoryManager.OffhandTool == EnumTool.Hammer)) {
                            if (api is not ICoreClientAPI capi) return false;
                            capi.TriggerIngameError(this, "nohammer", Lang.Get("touchmarks:error-stampingtable-nohammer"));
                            return false;
                        } else if (TouchmarksUtils.IsItemStamped(stampingTableEntity.stampingStack)) {
                            if (api is not ICoreClientAPI capi) return false;
                            capi.TriggerIngameError(this, "alreadystamped", Lang.Get("touchmarks:error-stampingtable-alreadystamped"));
                            return false;
                        }
                        // Player is attempting to stamp item, this takes time so is handled over time, we play a sound now
                        //TouchmarksModSystem.Logger.Debug("Player attempting to stamp item...");
                        var loc = byPlayer.Entity.Pos;
                        world.PlaySoundAt(new AssetLocation("sounds/player/messycraft.ogg"), loc.X, loc.Y, loc.Z, byPlayer, false, 32f, 1f);
                        //if (api.Side.IsServer()) {
                        //TouchmarksModSystem.Logger.Debug("Attempting to play hammer and chisel animation...");
                        (byPlayer as IClientPlayer)?.Entity.StartAnimation("hammerandchisel");
                        //}
                        return true;
                    } else {
                        // Player is attempting to remove an item from the table
                        if (stampingTableEntity.TryTakeItemFromStampingTable(world, byPlayer, blockSel)) {
                            if (world.Side.IsServer()) {
                                stampingTableEntity.MarkDirty(true);
                            }
                            return true;
                        }
                    }
                } else {
                    // Player is holding shift, trying to add an item
                    if (playerSlot.Itemstack?.Item != null) {
                        if (TouchmarksUtils.IsCollectibleMetalStampable(playerSlot.Itemstack.Item)) {
                            if (stampingTableEntity.TryPutItemInStampingTable(world, byPlayer, blockSel)) {
                                if (world.Side.IsServer()) {
                                    stampingTableEntity.MarkDirty(true);
                                }
                                return true;
                            }
                        } else {
                            if (api is not ICoreClientAPI capi) return false;
                            capi.TriggerIngameError(this, "notstampable", Lang.Get("touchmarks:error-stampingtable-notstampable"));
                            return false;
                        }
                    }
                    
                }
            }

            return base.OnBlockInteractStart(world, byPlayer, blockSel);
        }

        public override bool OnBlockInteractStep(float secondsUsed, IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel) {
            //TouchmarksModSystem.Logger.Debug("Stamping table interaction stepped.");
            BlockEntityStampingTable stampingTableEntity = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BlockEntityStampingTable;
            var playerSlot = byPlayer.InventoryManager.ActiveHotbarSlot;

            if (stampingTableEntity != null && playerSlot.Itemstack != null) {
                if (
                    TouchmarksUtils.IsItemAssignedPersonalTouchmark(playerSlot.Itemstack) &&
                    byPlayer.InventoryManager.OffhandTool == EnumTool.Hammer &&
                    stampingTableEntity.stampingStack != null &&
                    !TouchmarksUtils.IsItemStamped(stampingTableEntity.stampingStack)
                ) {
                    //TouchmarksModSystem.Logger.Debug("Continuing stamp interaction...");
                    if (secondsUsed > 3 && world.Side.IsServer()) {
                        stampingTableEntity.TryStampItemInStampingTable(world, byPlayer, blockSel);
                        return false;
                    }
                    return true;
                }
                return false;
            }

            return base.OnBlockInteractStep(secondsUsed, world, byPlayer, blockSel);
        }

        public override bool OnBlockInteractCancel(float secondsUsed, IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, EnumItemUseCancelReason cancelReason) {
            if (byPlayer.Entity.AnimManager.IsAnimationActive("hammerandchisel")) {
                byPlayer.Entity.StopAnimation("hammerandchisel");
                return true;
            }

            return base.OnBlockInteractCancel(secondsUsed, world, byPlayer, blockSel, cancelReason);
        }

        public override void OnBlockInteractStop(float secondsUsed, IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel) {
            if (byPlayer.Entity.AnimManager.IsAnimationActive("hammerandchisel")) {
                byPlayer.Entity.StopAnimation("hammerandchisel");
            }

            base.OnBlockInteractStop(secondsUsed, world, byPlayer, blockSel);
        }

        public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer) {
            return interactions.Append(base.GetPlacedBlockInteractionHelp(world, selection, forPlayer));
        }
    }
}
