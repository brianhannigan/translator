using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Translator
{
    /// <summary>
    /// Exit codes
    /// </summary>
    public enum ExitCodes
    {
        SUCCESS = 0,
        HELP_REQUESTED = 1,
        SETTINGS_NOT_LOADED = 2,
        INITILIZATION_DATA_NOT_LOADED = 3,
        PRE_BUILD_FAIL = 4,
        BUILD_FAIL = 5,
        POST_BUILD_FAIL = 6
    }
}
