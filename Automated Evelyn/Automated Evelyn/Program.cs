using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace Automated_Evelyn
{
    class Program
    {
        private static Menu _cfgMenu;
        private static Orbwalking.Orbwalker _orbwalker;
        private static Spell _q, _w, _e, _r;
        private static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        public static SpellSlot SmiteSlot = SpellSlot.Unknown;
        private static Spell _smiteSlot;
        private const float Range = 570f;       
        
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if ("Evelynn" != Player.ChampionName) return;
            _q = new Spell(SpellSlot.Q, 495f);
            _w = new Spell(SpellSlot.W, 300f);
            _e = new Spell(SpellSlot.E, 265f);
            _r = new Spell(SpellSlot.R, 625f);
            _r.SetSkillshot(0.25f, 500f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            //Menu instance
            _cfgMenu = new Menu("Automated Evelyn", "Automated Evelyn", true);

            //Orbwalker
            _orbwalker = new Orbwalking.Orbwalker(_cfgMenu.SubMenu("Orbwalking"));

            //Targetsleector
            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            _cfgMenu.AddSubMenu(targetSelectorMenu);

            //Combo
            _cfgMenu.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
            _cfgMenu.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
            _cfgMenu.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));
            _cfgMenu.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "Use R").SetValue(true));

            //Combo settings
            _cfgMenu.SubMenu("Combo Settings").AddItem(new MenuItem("RminP", "Min players to ult").SetValue(new Slider(3, 1, 5)));
            _cfgMenu.SubMenu("Combo Settings").AddItem(new MenuItem("WmaxHP", "Auto cast W when above hp %").SetValue(new Slider(25, 1, 99)));
            _cfgMenu.SubMenu("Combo Settings").AddItem(new MenuItem("BorkCut", "Use Botrk/Cutlass").SetValue(true));
            _cfgMenu.SubMenu("Combo Settings").AddItem(new MenuItem("UseGB", "Use Ghostblade").SetValue(true));
            _cfgMenu.SubMenu("Combo Settings").AddItem(new MenuItem("UseSmite", "Use Smite on combo").SetValue(true));
            
            //Drawings
            _cfgMenu.SubMenu("Drawings").AddItem(new MenuItem("DrawQ", "DrawQ").SetValue(true));
            _cfgMenu.SubMenu("Drawings").AddItem(new MenuItem("DrawE", "DrawE").SetValue(true));
            _cfgMenu.SubMenu("Drawings").AddItem(new MenuItem("DrawR", "DrawR").SetValue(true));
            _cfgMenu.SubMenu("Drawings").AddItem(new MenuItem("Ctarget", "Draw current target").SetValue(true));
            _cfgMenu.SubMenu("Drawings").AddItem(new MenuItem("Total damage", "Combo Damage").SetValue(new Circle(true, Color.White)));

            //Misc        
            _cfgMenu.SubMenu("Misc").AddItem(new MenuItem("WmaxHP", "Max HP to W slows").SetValue(new Slider(25, 1, 99)));      
            _cfgMenu.SubMenu("Misc").AddItem(new MenuItem("Strinket", "Swap to Red trinket at lvl 9 and upgrade").SetValue(true));
            
            //Attach to root
            _cfgMenu.AddToMainMenu();

            //Get Total damage
            CustomDamageIndicator.Initialize(GetDamage);

            //Listen to events
            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;

            //Assembly loaded notification
            Notifications.AddNotification("Automated Evelyn Loaded!", 8000);
        }//end OngameLoad

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (ObjectManager.Player.IsDead) return;

            //Draw current target
            if (_orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && _cfgMenu.Item("Ctarget").GetValue<bool>())
            {
                var target = TargetSelector.GetTarget(_q.Range, TargetSelector.DamageType.Magical);
                if (target.IsValidTarget(_q.Range))
                {
                    Drawing.DrawText(50, 150, Color.Green, "damage: " + (GetDamage(target)));
                    Render.Circle.DrawCircle(target.Position, 65f, Color.Red);
                }
            }
            // Total burts damage healthbar draw
            CustomDamageIndicator.DrawingColor = _cfgMenu.Item("Total damage").GetValue<Circle>().Color;
            CustomDamageIndicator.Enabled = _cfgMenu.Item("Total damage").GetValue<Circle>().Active;

            //Ability Range indicators
            if (_cfgMenu.Item("DrawQ").GetValue<bool>() && _q.IsReady())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, _q.Range, Color.WhiteSmoke);
            }
            if (_cfgMenu.Item("DrawE").GetValue<bool>() && _e.IsReady())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, _e.Range, Color.Red);
            }
            if (_cfgMenu.Item("DrawR").GetValue<bool>() && _r.IsReady())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, _r.Range, Color.Purple);
            }
        }//end draw

        private static void Game_OnUpdate(EventArgs args)
        {
            if (_orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                Combo();
            }    
            Extra();
        }//end game update

        private static void Combo()
        {
            //get target
            var target = TargetSelector.GetTarget(_r.Range, TargetSelector.DamageType.Physical);

            //return if no valid target is found
            if (target == null || Player.IsImmovable) return;

            //Use r 
            if (target.IsValidTarget(_r.Range) && _r.IsReady())
            {
                var bestposition = AutomatedLybrary.BestPosition(_r, 250f);
                if (bestposition.CountEnemiesInRange(250f) >= _cfgMenu.Item("RminP").GetValue<Slider>().Value)
                {
                    _r.Cast(bestposition);
                }
            }       

            //use q
            if (_cfgMenu.Item("UseQCombo").GetValue<bool>() && target.IsValidTarget(_q.Range))
                _q.Cast();

            //Use Botrk
            if (target.IsValidTarget(450) && target.Health > GetDamage(target) && _cfgMenu.Item("BorkCut").GetValue<bool>() && Items.CanUseItem(3144) ||
                Items.CanUseItem(3153) && !target.HasBuffOfType(BuffType.Slow) || !target.IsStunned)
            {
                var hasCutGlass = Items.HasItem(3144);
                var hasBotrk = Items.HasItem(3153);

                if (hasBotrk || hasCutGlass)
                {
                    var itemId = hasCutGlass ? 3144 : 3153;
                    var damage = Player.GetItemDamage(target, Damage.DamageItems.Botrk);
                    if (hasCutGlass || Player.Health + damage < Player.MaxHealth)
                        Items.UseItem(itemId, target);
                }
            }

            //use w
            if (_cfgMenu.Item("UseWCombo").GetValue<bool>() && _w.IsReady() && !target.IsValidTarget(_e.Range)
                 && Player.HealthPercent >= _cfgMenu.Item("WmaxHP").GetValue<Slider>().Value)
                _w.Cast();

            //use smite on enemy
            if (_cfgMenu.Item("UseSmite").GetValue<bool>() &&  Items.HasItem(3706) || Items.HasItem(3707) || Items.HasItem(3708) ||
                Items.HasItem(3709) || Items.HasItem(3710) || Items.HasItem(3714) || Items.HasItem(3715) || Items.HasItem(3716) ||
                Items.HasItem(3717) || Items.HasItem(3718))
            {

                if (_smiteSlot.CanCast(target) && GetDamage(target) < target.Health &&
                    !target.HasBuffOfType(BuffType.Slow))
                {
                    _smiteSlot.Slot = SmiteSlot;
                    Player.Spellbook.CastSpell(SmiteSlot, target);
                }
            }
            //use e
            if (_cfgMenu.Item("UseECombo").GetValue<bool>() && target.IsValidTarget(_e.Range) && _e.IsReady())
            {
                _e.Cast(target);
            }
            
            //Use Youmus
            if (_cfgMenu.Item("UseGB").GetValue<bool>() && target.IsValidTarget(_e.Range) && target.Health > GetDamage(target) && Items.CanUseItem(3142))
            {
                Items.UseItem(3142);
            }     
        }//end combo

        private static void Extra()
        {
            //use w if slowed and hp is below x
            if (_w.IsReady() &&
                ObjectManager.Player.HasBuffOfType(BuffType.Slow) && Player.HealthPercent < _cfgMenu.Item("WminHP").GetValue<Slider>().Value)
                _w.Cast();

            //Atuobuy Red trinket and upgrade it
            if (_cfgMenu.Item("Strinket").GetValue<bool>() && Player.Level >= 9 && Player.InShop() ||
                Player.IsDead && !(Items.HasItem(3364)))
            {
                if (!Items.HasItem(3341))
                {
                    Player.BuyItem(ItemId.Sweeping_Lens_Trinket);
                }
                if (Player.GoldTotal >= 250 && Items.HasItem(3341))
                {
                    Player.BuyItem(ItemId.Oracles_Lens_Trinket);
                }
            }
            
            if (!Player.InShop() && !_cfgMenu.Item("UseSmite").GetValue<bool>()) return;
            foreach (var spell in ObjectManager.Player.Spellbook.Spells.Where(spell => String.Equals(spell.Name, 
                "s5_summonersmiteplayerganker", StringComparison.CurrentCultureIgnoreCase)))
            {
                SmiteSlot = spell.Slot;
                _smiteSlot = new Spell(SmiteSlot, Range);
                return;
            }
        }//end extra
   
        private static float GetDamage(Obj_AI_Hero target)
        {
            float totaldamage = 0;
            float mana = 0;

            if (Items.HasItem(3153))
            {
                if (_e.IsReady())
                {                   
                    totaldamage += Convert.ToSingle((target.Health*.06));
                }
                else
                {
                    totaldamage += Convert.ToSingle((target.Health*.02));
                }
            }

            if  (Player.Mana > _r.Instance.ManaCost)
            {
                totaldamage += _r.GetDamage(target);
                mana += _r.Instance.ManaCost;
            }

            if (_q.IsReady() && Player.Mana > (_q.Instance.ManaCost + mana))
            {
                totaldamage += _q.GetDamage(target);
                mana += _q.Instance.ManaCost;
            }

            if (_e.IsReady() && Player.Mana > (_e.Instance.ManaCost + mana))
            {
                totaldamage += _e.GetDamage(target) + (Convert.ToSingle(Player.GetAutoAttackDamage(Player, true) * 2));                          
            }
            else
            {
                totaldamage += Convert.ToSingle(Player.GetAutoAttackDamage(Player, true));
            }
            return totaldamage;
        }//end GetDamage
    }
}
