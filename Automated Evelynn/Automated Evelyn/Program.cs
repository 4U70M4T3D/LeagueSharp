using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace AutomatedEvelynn
{
    class Program
    {          
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Evelynn.Game_OnGameLoad;        
        }
    }
}
