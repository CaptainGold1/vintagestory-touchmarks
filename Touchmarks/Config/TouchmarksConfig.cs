using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Touchmarks.Config {
    public class TouchmarksConfig {
        // True by default, since the first run through should always generate a config.
        // You can set this to false if you want to persist your changes to the regex.
        public bool regenerateRegexes = true;
        // Will print the generated regexes as debug information.
        public bool printGeneratedRegex = false;

        public bool debugMessages = false;

        // These are automatically genereated if "regeneratePartRegex" is set to true
        public string stampableMetalItemsMatchAny = "";
        public string stampableMetalItemsMatchStart = "";
        public string stampableMetalItemsExcludeAny = "";

        public string touchmarkColorHexCode = "#ffff00";
    }
}
