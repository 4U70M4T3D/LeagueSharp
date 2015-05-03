using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;


namespace Automated_Evelyn
{
    class Program
    {
        private static Menu _config;
        private static Orbwalking.Orbwalker _orbwalker;
        private static Spell _q, _w, _e, _r;       
        private static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        public static SpellSlot SmiteSlot = SpellSlot.Unknown;
        private static Spell _smiteSlot;
        private const float Range = 570f;

        static void Main(string[] args)
        {           
                CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
            }

        private static float GetDamage(Obj_AI_Hero target)
        {
            float temp = 0;
            float mana = 0;

            temp += Convert.ToSingle((Player.PercentAttackSpeedMod * 2) * Player.GetAutoAttackDamage(target, true));
            if ((mana += _q.Instance.ManaCost * 2)<= Player.Mana)
                temp += 2 * _q.GetDamage(target);           
            if (_e.IsReady() && (mana += _e.Instance.ManaCost) <= Player.Mana)
                temp += _e.GetDamage(target);
            if (_r.IsReady() && (mana + _r.Instance.ManaCost) <= Player.Mana)
                temp += _r.GetDamage(target);          
            return temp;
        }//end GetDamage

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (ObjectManager.Player.BaseSkinName != Player.ChampionName) return;
            _q = new Spell(SpellSlot.Q, 495f);
            _w = new Spell(SpellSlot.W, 300f);
            _e = new Spell(SpellSlot.E, 265f);
            _r = new Spell(SpellSlot.R, 625f);
            _r.SetSkillshot(0.25f, 500f, float.MaxValue, false, SkillshotType.SkillshotCircle);
       
            //Menu instance
            _config = new Menu("Automated Evelyn", "Automated Evelyn", true);

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

            //KillSteal
            _config.SubMenu("KillSteal").AddItem(new MenuItem("Qks", "Kill steal with q").SetValue(true));
            _config.SubMenu("KillSteal").AddItem(new MenuItem("Eks", "Kill steal with E").SetValue(true));

            //Drawings
            _config.SubMenu("Drawings").AddItem(new MenuItem("DrawQ", "DrawQ").SetValue(true));
            _config.SubMenu("Drawings").AddItem(new MenuItem("DrawE", "DrawE").SetValue(true));
            _config.SubMenu("Drawings").AddItem(new MenuItem("DrawR", "DrawR").SetValue(true));
            _config.SubMenu("Drawings").AddItem(new MenuItem("Ctarget", "Draw current target").SetValue(true));
            _config.SubMenu("Drawings").AddItem(new MenuItem("Total damage", "Combo Damage").SetValue(new Circle(true, Color.White)));

            //Misc
            _config.SubMenu("Misc").AddItem(new MenuItem("RminP", "Min players to ult").SetValue(new Slider(1, 1, 5)));
            _config.SubMenu("Misc").AddItem(new MenuItem("WminHP", "Min HP to W slows").SetValue(new Slider(1, 25, 99)));
            _config.SubMenu("Misc").AddItem(new MenuItem("Strinket", "Swap to Red trinket at lvl 9 and upgrade").SetValue(true));
            _config.SubMenu("Misc").AddItem(new MenuItem("BorkCut", "Use Botrk/Cutlass").SetValue(true));
            _config.SubMenu("Misc").AddItem(new MenuItem("UseGB", "Use Ghostblade").SetValue(true));
            _config.SubMenu("Misc").AddItem(new MenuItem("UseSmite", "Use Smite on combo").SetValue(true));

       
            //Attach to root
            _config.AddToMainMenu();

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
                Render.Circle.DrawCircle(ObjectManager.Player.Position, _q.Range, Color.LawnGreen);
            }
            if (_config.Item("DrawE").GetValue<bool>() && _e.IsReady())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, _e.Range, Color.Orange);
            }
            if (_config.Item("DrawR").GetValue<bool>() && _r.IsReady())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, _r.Range, Color.Red);
            }
        }//end draw



        private static void Game_OnUpdate(EventArgs args)
        {
            Extra(); 
            if (_orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                Combo();
            }              
        }

        private static void Combo()
        {
            //get target
            var target = TargetSelector.GetTarget(_q.Range, TargetSelector.DamageType.True);

            //return if no valid target is found
            if (target == null || Player.IsImmovable) return;
                       
            //use q
            if (_config.Item("UseQCombo").GetValue<bool>())
                _q.Cast();

            //use w
            if (_config.Item("UseWCombo").GetValue<bool>() && _w.IsReady() && !target.IsValidTarget(_e.Range)
                 && Player.HealthPercent > 25)
                _w.Cast();

            //Use Botrk
            if (target.IsValidTarget(450) && target.Health > GetDamage(target) && _config.Item("BorkCut").GetValue<bool>() && Items.CanUseItem(3144) ||
                Items.CanUseItem(3153) && !target.IsMovementImpaired())
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
            
            //use smite on enemy
            if (_config.Item("UseSmite").GetValue<bool>() && _smiteSlot.CanCast(target) && !target.IsMovementImpaired())
            {
                _smiteSlot.Slot = SmiteSlot;
                Player.Spellbook.CastSpell(SmiteSlot, target);
            }

            //use e
            if (_config.Item("UseECombo").GetValue<bool>() && _e.IsReady() && _config.Item("UseGB").GetValue<bool>())             
                _e.CastOnUnit(target);
           
            //Use Youmus
            if (Orbwalking.InAutoAttackRange(target) && target.Health < GetDamage(target) && Items.CanUseItem(3142))
            {
                Items.UseItem(3142);
            }

            //Use r 
            if (!_r.IsReady()) return;
            var bestUltPos =
                Environment.Hero.bestVectorToAoeSpell(
                    ObjectManager.Get<Obj_AI_Hero>().Where(i => (i.IsEnemy && _r.CanCast(i))), _r.Range, 250f);
            if (_config.Item("UseRCombo").GetValue<bool>() &&
                bestUltPos.CountEnemiesInRange(250f) >= _config.Item("RminP").GetValue<Slider>().Value &&
                _r.Range > Player.Distance(bestUltPos))
            {
                _r.Cast(bestUltPos);
            }
        }


        private static void Extra()
        {
            //use w if slowed and hp is below x
            if (_w.IsReady() &&
                ObjectManager.Player.HasBuffOfType(BuffType.Slow) && Player.HealthPercent < _config.Item("WminHP").GetValue<Slider>().Value)
                _w.Cast();
                          
            if (_q.IsReady() && _config.Item("Qks").GetValue<bool>() && _q.Instance.ManaCost <= Player.Mana)
            {     
                    foreach (
                        var target in
                            ObjectManager.Get<Obj_AI_Hero>()
                                .Where(enemy => enemy.IsValidTarget(_q.Range) && _q.IsKillable(enemy))
                        )
                    {
                        _q.Cast();
                    }              
            }
            if (_e.IsReady() && _config.Item("Eks").GetValue<bool>() && _e.Instance.ManaCost <= Player.Mana)
            {
                foreach (
                    var target in
                        ObjectManager.Get<Obj_AI_Hero>()
                            .Where(enemy => enemy.IsValidTarget(_e.Range) && _e.IsKillable(enemy))
                    )
                {
                    _e.Cast(target);
                }             
            }

            //Atuobuy Red trinket and upgrade it
            if (_config.Item("Strinket").GetValue<bool>() && Player.Level >= 9 && Player.InShop() ||
                Player.IsDead && !(Items.HasItem(3364)))
            {
                Player.BuyItem(ItemId.Sweeping_Lens_Trinket);
                if (Player.GoldTotal >= 250)
                {
                    Player.BuyItem(ItemId.Oracles_Lens_Trinket);
                }
            }

            if (!Player.InShop() && !_config.Item("UseSmite").GetValue<bool>()) return;
            foreach (var spell in ObjectManager.Player.Spellbook.Spells.Where(spell => String.Equals(spell.Name, "s5_summonersmiteplayerganker", StringComparison.CurrentCultureIgnoreCase)))
            {
                SmiteSlot = spell.Slot;
                _smiteSlot = new Spell(SmiteSlot, Range);
                Notifications.AddNotification("It's Smiting time!", 4000);
                return;
            }
        }
    }
}
