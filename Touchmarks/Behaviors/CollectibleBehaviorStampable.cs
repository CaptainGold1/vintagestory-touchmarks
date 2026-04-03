using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Touchmarks.Config;
using Touchmarks.Utils;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace Touchmarks.Behaviors {
    public class CollectibleBehaviorStampable : CollectibleBehavior {
        public CollectibleBehaviorStampable(CollectibleObject collObj) : base(collObj) {
        }

        // For now, this is unused.
        // However, I plan to allow you to stamp custom names into items in the future.
        //public override void GetHeldItemName(StringBuilder sb, ItemStack itemStack) {
        //    sb.Clear();
        //    sb.Append("Test Name Override");
        //}

        public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo) {
            base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);

            ItemStack itemStack = inSlot.Itemstack;

            string touchmark = itemStack.Attributes.GetString("touchmark");
            if (touchmark != null) {
                dsc.AppendLine(Lang.Get("touchmarks:description-touchmarked", touchmark, TouchmarksModSystem.config.touchmarkColorHexCode));
            }
        }

        public override void OnCreatedByCrafting(ItemSlot[] allInputslots, ItemSlot outputSlot, GridRecipe byRecipe, ref EnumHandling bhHandling) {
            // Preserve touchmark across crafts
            // Technically, this could have issues if you have to craft a stampable item using a touchmarked item as a tool and a touchmarked item as a part
            // It has the potential to use the touchmarked tool instead of (validly) the touchmarked part
            // However I don't think?? any instances of this exist in vanilla...
            // Still,
            // TODO: Consider splitting craftable tools into a separate behavior, with a separate tag than touchmarked parts
            foreach (var slot in allInputslots.Where(i => i.Itemstack != null)) {
                if (TouchmarksUtils.IsItemStamped(slot.Itemstack)) {
                    if (TouchmarksModSystem.config.debugMessages) {
                        TouchmarksModSystem.Logger.Debug(
                            $"Transfering touchmark of {slot.Itemstack.Attributes.GetString("touchmark")} from {slot.Itemstack.Item.Code}"
                        );
                    }
                    outputSlot.Itemstack.Attributes.SetString("touchmark", slot.Itemstack.Attributes.GetString("touchmark"));
                    break; // We don't need to continue past this point
                }
            }

            bhHandling = EnumHandling.Handled;
        }
    }
}
