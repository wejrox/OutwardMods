using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Marathoner
{
    class ModOptions
    {
        /// <summary>
        /// Multiplier to increase sprint speed by. 
        /// 1.0 is the default, anything above 3.0 may cause fall damage due to character speed.
        /// </summary>
        public float SprintSpeedMultiplier = 1.0f;
    }
}
