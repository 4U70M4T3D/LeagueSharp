using System;
using System.Drawing;
using LeagueSharp;
using LeagueSharp.Common;

namespace AutomatedEvelynn
{
    class Drawings
    {
        public static void Drawing_OnDraw(EventArgs args)
        {
            
            var drawQ = Menu.ConfigMenu.Item("Evelynn.Draw.Q").GetValue<bool>();
            var drawE = Menu.ConfigMenu.Item("Evelynn.Draw.E").GetValue<bool>();
            var drawR = Menu.ConfigMenu.Item("Evelynn.Draw.R").GetValue<bool>();            
                       
            if (drawQ)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, Evelynn.SpellDirectory[Spells.Q].Range, Color.WhiteSmoke);
            }
            if (drawE && Evelynn.SpellDirectory[Spells.E].IsReady())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, Evelynn.SpellDirectory[Spells.E].Range, Color.Red);
            }
            if (drawR && Evelynn.SpellDirectory[Spells.R].IsReady())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, Evelynn.SpellDirectory[Spells.R].Range, Color.Purple);
            }    
        }
    }
}
