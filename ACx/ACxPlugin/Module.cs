using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ACxPlugin
{
    public abstract class Module
    {
        /// <summary>
        /// Called once when the main plugin is loaded
        /// </summary>
        public abstract void Startup();

        /// <summary>
        /// Called when the main plugin is shutting down.  Unregister from any events here and do any cleanup.
        /// </summary>
        public abstract void Shutdown();
    }
}
