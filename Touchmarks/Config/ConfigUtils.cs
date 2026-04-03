using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Touchmarks;
using Vintagestory.API.Common;
using Vintagestory.API.Util;

namespace Touchmarks.Config {
    public static class ConfigUtils {
        public static string GetMatchAnyRegexString(ref Dictionary<AssetLocation, List<string>> assets) {
            StringBuilder reg = new StringBuilder();

            reg.Append("@.*(");

            bool anyValues = false;
            foreach (var assetFile in assets) {
                foreach (var entry in assetFile.Value) {
                    anyValues = true;
                    reg.Append(entry).Append('|');
                }
            }
            // Prune the last bar, but only if we actually have anything in the string
            if (anyValues) {
                reg.Remove(reg.Length - 1, 1);
            }
            // Add the ending
            reg.Append(").*");

            return reg.ToString();
        }

        public static string GetMatchStartRegexString(ref Dictionary<AssetLocation, List<string>> assets) {
            StringBuilder reg = new StringBuilder();

            reg.Append("@.*:(");

            bool anyValues = false;
            foreach (var assetFile in assets) {
                foreach (var entry in assetFile.Value) {
                    anyValues = true;
                    reg.Append(entry).Append('|');
                }
            }
            // Prune the last bar, but only if we actually have anything in the string
            if (anyValues) {
                reg.Remove(reg.Length - 1, 1);
            }
            // Add the ending
            reg.Append(").*");

            return reg.ToString();
        }
    }
}
