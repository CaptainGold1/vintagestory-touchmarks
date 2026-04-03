using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Touchmarks;
using Touchmarks.Utils;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.Client.NoObf;
using Vintagestory.GameContent;

namespace Touchmarks.Blocks {
    internal class BlockEntityStampingTable : BlockEntityDisplay {
        // A lot of this code is somewhat adapted from Toolsmith's workbench and the vanilla forge and oven BlockEntities.
        
        InventoryGeneric inventory;
        private (float x, float y, float z) offset = (0f, 1f, 0f);
        private int rotationDegrees;
        ItemSlot stampingSlot => inventory[0];
        public ItemStack stampingStack => stampingSlot.Itemstack;

        public override InventoryBase Inventory => inventory;

        public override string InventoryClassName => "stampingtable";

        public BlockEntityStampingTable() {
            inventory = new InventoryGeneric(1, null, null);
        }

        public override void Initialize(ICoreAPI api) {
            base.Initialize(api);

            if (api is ICoreClientAPI) {
                capi = (ICoreClientAPI) api;
            }

            setRotation();
        }

        private void setRotation() {
            switch (Block.Variant["side"]) {
                case "south":
                    rotationDegrees = 270;
                    break;
                case "west":
                    rotationDegrees = 180;
                    break;
                case "east":
                    rotationDegrees = 0;
                    break;
                default:
                    rotationDegrees = 90;
                    break;
            }
        }

        public bool TryTakeItemFromStampingTable(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel) {
            ItemSlot playerSlot = byPlayer.InventoryManager.ActiveHotbarSlot;

            if (stampingStack == null) return false;
            if (byPlayer.InventoryManager.TryGiveItemstack(stampingStack)) {

            }
            else {
                world.SpawnItemEntity(stampingStack, Pos);
            }

            Api.World.Logger.Audit("{0} took 1x{1} from Stamping Table at {2}.",
                byPlayer.PlayerName,
                stampingStack.Collectible.Code,
                blockSel.Position
            );

            stampingSlot.Itemstack = null;
            updateMesh(0);
            MarkDirty();
            return true;
        }

        public bool TryPutItemInStampingTable(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel) {
            ItemSlot playerSlot = byPlayer.InventoryManager.ActiveHotbarSlot;

            if (playerSlot.Itemstack == null) return false;

            if (stampingStack == null && TouchmarksUtils.IsCollectibleMetalStampable(playerSlot.Itemstack.Item)) {
                int movedItems = playerSlot.TryPutInto(world, stampingSlot, 64);
                updateMesh(0);
                MarkDirty();

                playerSlot.MarkDirty();

                Api.World.Logger.Audit("{0} Put {3}x{1} into Stamping Table at {2}.",
                    byPlayer.PlayerName,
                    stampingStack?.Collectible.Code,
                    blockSel.Position,
                    movedItems
                );
                return true;
            }
            else {
                return false;
            }
        }

        public bool TryStampItemInStampingTable(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel) {
            if (stampingStack == null) return false;

            ItemSlot playerSlot = byPlayer.InventoryManager.ActiveHotbarSlot;
            ItemSlot offhandSlot = byPlayer.InventoryManager.OffhandHotbarSlot;

            if (playerSlot.Itemstack != null &&
                TouchmarksUtils.IsItemAssignedPersonalTouchmark(playerSlot.Itemstack) &&
                byPlayer.InventoryManager.OffhandTool == EnumTool.Hammer &&
                !TouchmarksUtils.IsItemStamped(stampingStack)
            ) {
                TouchmarksUtils.StampItemWithTouchmark(playerSlot.Itemstack, stampingStack);
                playerSlot.Itemstack.Collectible.DamageItem(world, byPlayer.Entity, playerSlot);
                offhandSlot.Itemstack.Collectible.DamageItem(world, byPlayer.Entity, offhandSlot);
                TryTakeItemFromStampingTable(world, byPlayer, blockSel);

                return true;
            } else {
                return false;
            }
        }

        protected override float[][] genTransformationMatrices() {
            float[][] tfMatrices = [
                new Matrixf()
                    .Translate(0.5, 0, 0.5)
                    .RotateYDeg(rotationDegrees)
                    .Translate(-0.5, 0, -0.5)
                    .Translate(offset.x, offset.y, offset.z)
                    .Values
            ];
            return tfMatrices;
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve) {
            base.FromTreeAttributes(tree, worldAccessForResolve);

            inventory.FromTreeAttributes(tree);
            rotationDegrees = tree.GetInt("rotation", 90);
            RedrawAfterReceivingTreeAttributes(worldAccessForResolve);
        }

        public override void ToTreeAttributes(ITreeAttribute tree) {
            base.ToTreeAttributes(tree);

            inventory.ToTreeAttributes(tree);
            tree.SetInt("rotation", rotationDegrees);
        }
    }
}
