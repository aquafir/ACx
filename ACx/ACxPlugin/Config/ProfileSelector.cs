using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ACxPlugin
{
    /// <summary>
    /// The main config maps to character profiles based on a Regex of their name, account, and server in order of priority
    /// </summary>
    public class ProfileSelector
    {
        // Name of the profile the user will recognize
        public string FriendlyName { get; set; }
        // Regexs that match this profile.  Missing ones are not considered.
        public string CharName { get; set; }
        public string Account { get; set; }
        public string Server { get; set; }

        // Order in which profiles are looked at for a match, in descending value
        public int Priority { get; set; }

        // Path to the profile to be loaded        
        public string Path { get; set; }

        public ProfileSelector() { }
    }
}

