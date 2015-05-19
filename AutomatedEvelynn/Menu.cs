using System;
using LeagueSharp.Common;

namespace AutomatedEvelynn
{
    
    public class Menu
    {
        public static LeagueSharp.Common.Menu ConfigMenu;

        public static void Initialize()
        {
            ConfigMenu = new LeagueSharp.Common.Menu("Automated Evelynn", "Menu", true);

            //AutomatedEvelynn.Orbwalker
            var orbwalkerMenu = new LeagueSharp.Common.Menu("Orbwalker", "orbwalker");
            Evelynn.Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);

            ConfigMenu.AddSubMenu(orbwalkerMenu);

            //AutomatedEvelynn.TargetSelector
            var targetSelector = new LeagueSharp.Common.Menu("Target Selector", "TargetSelector");
            TargetSelector.AddToMenu(targetSelector);

            ConfigMenu.AddSubMenu(targetSelector);

            var comboMenu = new LeagueSharp.Common.Menu("Combo", "Combo");
            comboMenu.AddItem(new MenuItem("Evelynn.Combo.Q", "Use Q").SetValue(true));
            comboMenu.AddItem(new MenuItem("Evelynn.Combo.W", "Use W").SetValue(true));
            comboMenu.AddItem(new MenuItem("Evelynn.Combo.E", "Use E").SetValue(true));
            comboMenu.AddItem(new MenuItem("Evelynn.Combo.R", "Use R").SetValue(true));
            comboMenu.AddItem(new MenuItem("Evelynn.Combo.Smite", "Use Smite").SetValue(true));
            comboMenu.AddItem(new MenuItem("Evelynn.Combo.Ignite", "Use Ignite").SetValue(true));
         
            comboMenu.AddItem(new MenuItem("Evelynn.Combo.Seperator", "Additional Settings"));
            comboMenu.AddItem(new MenuItem("Evelynn.Combo.R.Setting", "Min players to ult").SetValue(new Slider(3, 1, 5)));
            comboMenu.AddItem(new MenuItem("Evelynn.Combo.W.Setting", "Use W when above").SetValue(new Slider(50, 1)));       
            comboMenu.AddItem(new MenuItem("Evelyn..Combo.Active", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));

            ConfigMenu.AddSubMenu(comboMenu);
       
            var jcMenu = new LeagueSharp.Common.Menu("Jungle Clear", "Jungle Clear");
            jcMenu.AddItem(new MenuItem("Evelynn.jClear.Q", "Use Q").SetValue(true));
            jcMenu.AddItem(new MenuItem("Evelynn.jClear.E", "Use E").SetValue(true));
            jcMenu.AddItem(new MenuItem("Evelynn.jClear.M", "Movement").SetValue(true));
            
                  
            ConfigMenu.AddSubMenu(jcMenu);

            var itemMenu = new LeagueSharp.Common.Menu("Items", "Items");          
            itemMenu.AddItem(new MenuItem("Evelynn.Items.Seperator0", "Offenssive"));//Offensive
            itemMenu.AddItem(new MenuItem("Evelynn.Items.Ghostblade", "Use Youmuu's Ghostblade").SetValue(true));
            itemMenu.AddItem(new MenuItem("Evelynn.Items.Blade", "Use Blade of the Ruined King / Cutlass").SetValue(true));
            itemMenu.AddItem(new MenuItem("Evelynn.Items.Hydra", "Use Hydra / Tiamat").SetValue(true));
           
            itemMenu.AddItem(new MenuItem("Evelynn.Items.Seperator1", "Defensive"));//Defensive
            itemMenu.AddItem(new MenuItem("Evelynn.Items.RanduinOmen", "Randuin's Omen").SetValue(true));
            itemMenu.AddItem(new MenuItem("Evelynn.Items.RanduinOmen.Targets", "Randuins: Use if Enemies are in range").SetValue(new Slider(2, 1, 5)));
            itemMenu.AddItem(new MenuItem("Evelynn.Items.MercurialScimitar", "Mercurial Scimitar / QSS").SetValue(true));
  
            itemMenu.AddItem(new MenuItem("Evelynn.Items.Seperator3", "Consumables"));//Consumables
            itemMenu.AddItem(new MenuItem("Evelynn.Items.Potion", "Use HP Potion/Biscuit").SetValue(true));
            itemMenu.AddItem(new MenuItem("Evelynn.Items.Potion.HP", "Min HP to use Potion/Biscuit ").SetValue(new Slider(75, 1, 99)));
       
            ConfigMenu.AddSubMenu(itemMenu);

            //AutomatedEvelynn.Misc
            var miscMenu = new LeagueSharp.Common.Menu("Misc", " Drawings & Misc");
            miscMenu.AddItem(new MenuItem("Evelynn.Misc.E.Killsteal", "E to KS").SetValue(true));
            miscMenu.AddItem(new MenuItem("Evelynn.Misc.Trinket", "Swap to red trinket and upgrade at lvl 9").SetValue(true));
            miscMenu.AddItem(new MenuItem("Evelynn.Draw.Q", "Draw Q").SetValue(true));
            miscMenu.AddItem(new MenuItem("Evelynn.Draw.E", "Draw E").SetValue(true));
            miscMenu.AddItem(new MenuItem("Evelynn.Draw.R", "Draw R").SetValue(true));
         
            ConfigMenu.AddSubMenu(miscMenu);
            ConfigMenu.AddToMainMenu();

            Console.WriteLine("Menu Loaded");
        }
    }
}
