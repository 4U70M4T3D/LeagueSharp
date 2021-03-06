﻿using System;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace AutomatedAmumu
{
    public class Program
    {
        private static Menu _config;
        private static Orbwalking.Orbwalker _orbwalker;
        private static Spell _q, _w, _e, _r;
        private static readonly HitChance[] Hitchances = { HitChance.Low, HitChance.Medium, HitChance.High, HitChance.VeryHigh };
        private static Obj_AI_Hero Player { get { return ObjectManager.Player; } }      

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static float GetDamage(Obj_AI_Hero target)
        {
            float temp = 0;
            float mana = 0;

            temp += Convert.ToSingle(Player.GetAutoAttackDamage(target, true));
            if ((mana += _q.Instance.ManaCost) <= Player.Mana)
                temp += _q.GetDamage(target);
            if ((mana += _w.Instance.ManaCost) <= Player.Mana)
                temp += _w.GetDamage(target);
            if (_e.IsReady() && (mana += _e.Instance.ManaCost) <= Player.Mana)
                temp += _e.GetDamage(target);
            if (_r.IsReady() && (mana + _r.Instance.ManaCost) <= Player.Mana)
                temp += _r.GetDamage(target);
            return temp;
        }//end GetDamage

        private static void Game_OnGameLoad(EventArgs args)
        {
            //Verify Champion
            if (Player.ChampionName != "Amumu")
                return;

            //Spells
            _q = new Spell(SpellSlot.Q, 1000);         
            _w = new Spell(SpellSlot.W, 300);
            _e = new Spell(SpellSlot.E, 330);
            _r = new Spell(SpellSlot.R, 500);

            _q.SetSkillshot(250f, 90f, 2000f, true, SkillshotType.SkillshotLine);
            _w.SetSkillshot(0f, _w.Range, float.MaxValue, false, SkillshotType.SkillshotCircle);
            _e.SetSkillshot(.5f, _e.Range, float.MaxValue, false, SkillshotType.SkillshotCircle);
            _r.SetSkillshot(.25f, _r.Range, float.MaxValue, false, SkillshotType.SkillshotCircle);

            //Menu instance
            _config = new Menu("Automated Amumu", "Automated Amumu", true);

            //Orbwalker
            _orbwalker = new Orbwalking.Orbwalker(_config.SubMenu("Orbwalking"));

            //Targetsleector
            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            _config.AddSubMenu(targetSelectorMenu);

            //Combo
            _config.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
            _config.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
            _config.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));
            _config.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "Use R").SetValue(true));

            //Hitchance 
            _config.SubMenu("SKill Hitchance").AddItem(new MenuItem("QhitChance", "Q hitchance").SetValue(new Slider(2, 2, 3)));        
            _config.SubMenu("SKill Hitchance").AddItem(new MenuItem("QMaxRange", "Q max range").SetValue(new Slider(1000, 0, 1100)));
            _config.SubMenu("SKill Hitchance").AddItem(new MenuItem("EhitChance", "E hitchance").SetValue(new Slider(2, 0, 3)));
            _config.SubMenu("SKill Hitchance").AddItem(new MenuItem("RhitChance", "R hitchance").SetValue(new Slider(2, 0, 3)));
         
            //Drawings
            _config.SubMenu("Drawings").AddItem(new MenuItem("DrawQ", "DrawQ").SetValue(true));
            _config.SubMenu("Drawings").AddItem(new MenuItem("DrawW", "DrawW").SetValue(true));
            _config.SubMenu("Drawings").AddItem(new MenuItem("DrawE", "DrawE").SetValue(true));
            _config.SubMenu("Drawings").AddItem(new MenuItem("DrawR", "DrawR").SetValue(true));
            _config.SubMenu("Drawings").AddItem(new MenuItem("Ctarget", "Draw current target").SetValue(true));
            _config.SubMenu("Drawings").AddItem(new MenuItem("Total damage", "Combo Damage").SetValue(new Circle(true, Color.White)));

            //Misc
            _config.SubMenu("Misc").AddItem(new MenuItem("RminP", "Min players to ult").SetValue(new Slider(3, 1, 5)));
            _config.SubMenu("Misc").AddItem(new MenuItem("EKillsteal", "E kill Steal").SetValue(true));
            _config.SubMenu("Misc").AddItem(new MenuItem("WToggle", "W Auto Toggle").SetValue(true));
            _config.SubMenu("Misc").AddItem(new MenuItem("Strinket", "Swap to Red trinket at lvl 9 and upgrade").SetValue(true));
    
            //Attach to root
            _config.AddToMainMenu();

            //Get Total damage
            CustomDamageIndicator.Initialize(GetDamage);
     
            //Listen to events
            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;

            //Assembly loaded notification
            Notifications.AddNotification("Automated Amuumu Loaded!", 8000);          
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (ObjectManager.Player.IsDead) return;

            //Draw current target
            if (_orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && _config.Item("Ctarget").GetValue<bool>())
            {
                var target = TargetSelector.GetTarget(_q.Range, TargetSelector.DamageType.Magical);
                if (target.IsValidTarget(_q.Range))
                {
                    Render.Circle.DrawCircle(target.Position, 65f, Color.Red);
                }
            }

            // Total burts damage healthbar draw
            CustomDamageIndicator.DrawingColor = _config.Item("Total damage").GetValue<Circle>().Color;
            CustomDamageIndicator.Enabled = _config.Item("Total damage").GetValue<Circle>().Active;

            //Ability Range indicators
            if (_config.Item("DrawQ").GetValue<bool>() && _q.IsReady())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, _config.Item("QMaxRange").GetValue<Slider>().Value, Color.LawnGreen);
            }
            if (_config.Item("DrawW").GetValue<bool>() && ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).ToggleState != 1)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, _w.Range, Color.Cyan);
            }
            if (_config.Item("DrawE").GetValue<bool>() && _e.IsReady())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, _e.Range, Color.Orange);
            }
            if (_config.Item("DrawR").GetValue<bool>() && _r.IsReady())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, _r.Range, Color.Red);
            }
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (_e.IsReady() && _e.Instance.ManaCost <= Player.Mana)
            {
                //Killsteal with E
                if (_config.Item("EKillsteal").GetValue<bool>())
                {
                    foreach (
                        var enemy in
                            ObjectManager.Get<Obj_AI_Hero>()
                                .Where(enemy => enemy.IsValidTarget(_e.Range) && _e.IsKillable(enemy))
                        )
                    {
                        _e.CastIfHitchanceEquals(enemy, Hitchances[_config.Item("EhitChance").GetValue<Slider>().Value]);
                    }
                }       
            }

            //Check if enemies are nearby 
            if (_config.Item("WToggle").GetValue<bool>() &&
                ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).ToggleState != 1)
            {
                if (!ObjectManager.Get<Obj_AI_Hero>().Any(hero => hero.IsValidTarget(_w.Range + 150)) && !ObjectManager.Get<Obj_AI_Minion>().Any(minion => minion.IsValidTarget(_w.Range + 100)))
                {
                    _w.Cast();
                }
            }

            //Auto buy red trinket
            if (!_config.Item("Strinket").GetValue<bool>()) return;
            if (Player.Level >= 9 && Player.InShop() || Player.IsDead && !(Items.HasItem(3364)))
            {
                Player.BuyItem(ItemId.Sweeping_Lens_Trinket);
                if (Player.GoldTotal >= 250) {
                Player.BuyItem(ItemId.Oracles_Lens_Trinket);
                }
            }

            //Combo
            if (_orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Combo) return;
            var target = TargetSelector.GetTarget(_config.Item("QMaxRange").GetValue<Slider>().Value, TargetSelector.DamageType.Magical);

            if (!target.IsValidTarget())
            {
                return;
            }
            //Use q
            if (_config.Item("UseQCombo").GetValue<bool>() && _q.Instance.ManaCost <= Player.Mana)
            {
                if (target.IsValidTarget(_config.Item("QMaxRange").GetValue<Slider>().Value) && _q.IsReady())
                {
                    _q.CastIfHitchanceEquals(target, Hitchances[_config.Item("QhitChance").GetValue<Slider>().Value]);
                }
            }

            //Use w
            if (_config.Item("UseWCombo").GetValue<bool>() && _w.Instance.ManaCost <= Player.Mana)
            {
                if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).ToggleState == 1)
                {
                    if (_w.IsReady() && ObjectManager.Get<Obj_AI_Hero>().Any(hero => hero.IsValidTarget(_w.Range)))
                    {
                        _w.Cast();
                    }
                }              
            }

            //Use e
            if (_config.Item("UseECombo").GetValue<bool>() && _e.Instance.ManaCost <= Player.Mana)
            {                    
                if (target.IsValidTarget(_e.Range) && _e.IsReady())
                {
                    _e.CastIfHitchanceEquals(target, Hitchances[_config.Item("EhitChance").GetValue<Slider>().Value]);
                }
            }

            //Use r
            if (!_config.Item("UseRCombo").GetValue<bool>() && _r.Instance.ManaCost <= Player.Mana) return;
            var enemyCount = ObjectManager.Get<Obj_AI_Hero>().Count(e => e.IsValidTarget(_r.Range));
            if (_config.Item("UseRCombo").GetValue<bool>() && enemyCount >= _config.Item("RminP").GetValue<Slider>().Value && _r.IsReady())
            {
                _r.CastIfHitchanceEquals(target, Hitchances[_config.Item("RhitChance").GetValue<Slider>().Value]);
            }
        }
    }
}