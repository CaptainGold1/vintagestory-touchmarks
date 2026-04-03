using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Touchmarks.Config;
using Vintagestory.API.Common;
using Vintagestory.API.Util;

namespace Touchmarks.Utils {
    public static class TouchmarksUtils {
        //public static bool IsStampableAndMetal(CollectibleObject item, IWorldAccessor world) {
        //    if (item.HasBehavior<>)
        //        return false;
        //}

        public static void StampItemWithTouchmark(ItemStack touchmark, ItemStack toStamp) {
            toStamp.Attributes.SetString("touchmark", touchmark.Attributes.GetString("touchmarkOwner"));
        }

        public static bool IsItemPersonalTouchmark(Item item) {
            return item.Code.BeginsWith("touchmarks", "handstamp");
        }

        public static bool IsItemAssignedPersonalTouchmark(ItemStack itemStack) {
            return (
                (itemStack.Attributes?.GetString("touchmarkOwner", null) != null) &&
                itemStack.Item?.Code != null &&
                itemStack.Item.Code.BeginsWith("touchmarks", "handstamp")
            );
        }

        public static bool IsCollectibleMetalStampable(CollectibleObject coll) {
            if (coll == null || coll.Code == null) return false;
            string code = coll.Code;
            TouchmarksConfig config = TouchmarksModSystem.config;

            return (
                !WildcardUtil.Match(config.stampableMetalItemsExcludeAny, code) &&
                (
                    WildcardUtil.Match(config.stampableMetalItemsMatchStart, code) ||
                    WildcardUtil.Match(config.stampableMetalItemsMatchAny, code)
                )
            );
        }

        public static bool IsItemStamped(ItemStack itemStack) {
            return itemStack?.Attributes.GetString("touchmark", null) != null;
        }
    }
}
