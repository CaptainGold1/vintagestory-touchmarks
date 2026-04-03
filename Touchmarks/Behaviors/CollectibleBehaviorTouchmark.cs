using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Touchmarks.Utils;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace Touchmarks.Behaviors {
    internal class CollectibleBehaviorTouchmark : CollectibleBehavior {
        public CollectibleBehaviorTouchmark(CollectibleObject collObj) : base(collObj) {
        }

        public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo) {
            base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);

            ItemStack itemStack = inSlot.Itemstack;

            string touchmarkOwner = itemStack.Attributes.GetString("touchmarkOwner");
            if (touchmarkOwner != null) {
                dsc.AppendLine(Lang.Get("touchmarks:description-touchmarkowner", touchmarkOwner, TouchmarksModSystem.config.touchmarkColorHexCode));
            }
        }
    }
}
