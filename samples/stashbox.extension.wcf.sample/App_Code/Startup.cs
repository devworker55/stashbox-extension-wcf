using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace stashbox.extension.wcf.sample
{
    public static class Startup
    {
        public static void AppInitialize()
        {
            Bootstrapper.Configure();
        }
    }
}