using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using LeagueSharp.Common.Data;

namespace AutomatedEvelynn
{
    internal enum Spells
    {
        Q, W, E, R      
    }

    internal class Evelynn
    {
        private static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        public static Orbwalking.Orbwalker Orbwalker;
        private static SpellSlot _ignite, _smite; 
        private static Obj_AI_Hero _currentTarget;
               
        public static Dictionary <Spells, Spell> SpellDirectory = new Dictionary<Spells, Spell>
        {
            { Spells.Q, new Spell(SpellSlot.Q, 500f)},
            { Spells.W, new Spell(SpellSlot.W)},
            { Spells.E, new Spell(SpellSlot.E, 265f)},
            { Spells.R, new Spell(SpellSlot.R, 650f)}         
        };

        public static Dictionary<ItemData.Item, Items.Item> ItemDictionary = new Dictionary<ItemData.Item, Items.Item>
        {
            {ItemData.Youmuus_Ghostblade, ItemData.Youmuus_Ghostblade.GetItem()},
            {ItemData.Bilgewater_Cutlass, ItemData.Bilgewater_Cutlass.GetItem()},
            {ItemData.Blade_of_the_Ruined_King, ItemData.Blade_of_the_Ruined_King.GetItem()},
            {ItemData.Quicksilver_Sash, ItemData.Quicksilver_Sash.GetItem()},
            {ItemData.Mercurial_Scimitar, ItemData.Mercurial_Scimitar.GetItem()},
            {ItemData.Ravenous_Hydra_Melee_Only, ItemData.Ravenous_Hydra_Melee_Only.GetItem()},
            {ItemData.Tiamat_Melee_Only, ItemData.Tiamat_Melee_Only.GetItem()},
            {ItemData.Randuins_Omen, ItemData.Randuins_Omen.GetItem()},
            {ItemData.Health_Potion, ItemData.Health_Potion.GetItem()},
            {ItemData.Total_Biscuit_of_Rejuvenation2, ItemData.Total_Biscuit_of_Rejuvenation2.GetItem()},
            {ItemData.Sweeping_Lens_Trinket, ItemData.Sweeping_Lens_Trinket.GetItem()},
            {ItemData.Oracles_Lens_Trinket, ItemData.Oracles_Lens_Trinket.GetItem()},
            {ItemData.Stalkers_Blade, ItemData.Stalkers_Blade.GetItem()},
            {ItemData.Skirmishers_Sabre, ItemData.Skirmishers_Sabre.GetItem()}
        };
           
        #region Gameloaded
        public static void Game_OnGameLoad(EventArgs args)
        {
            if (ObjectManager.Player.BaseSkinName != "Evelynn") return;

            Notifications.AddNotification("Automated Evelynn by Automated v1.0.1.0", 4000);
            
            SpellDirectory[Spells.R].SetSkillshot(0.25f, 650f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            _ignite = Player.GetSpellSlot("summonerdot");           
           
            Menu.Initialize();
            CustomDamageIndicator.Initialize(GetDamage);

            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += Drawings.Drawing_OnDraw;
            Orbwalking.AfterAttack += OrbwalkingAfterAttack;        
        } 
        #endregion
        
        #region OnGameUpdate
        private static void OnUpdate(EventArgs args)
        {
            if (Player.IsDead) return;

            _currentTarget = TargetSelector.GetTarget(SpellDirectory[Spells.Q].Range, 
                Player.TotalAttackDamage > Player.TotalMagicalDamage ? TargetSelector.DamageType.Physical : TargetSelector.DamageType.Magical);

            Misc();

            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                   
                    Combo(_currentTarget);
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:                   
                    JungleClear();
                    break;            
            }
           
            Killsteal();           
        }
        #endregion

        #region Combo
        private static void Combo(Obj_AI_Hero target)
        {
            var comboQ = Menu.ConfigMenu.Item("Evelynn.Combo.Q").GetValue<bool>();                    
            var comboW = Menu.ConfigMenu.Item("Evelynn.Combo.W").GetValue<bool>();
            var comboR = Menu.ConfigMenu.Item("Evelynn.Combo.R").GetValue<bool>();
            var comboSmite = Menu.ConfigMenu.Item("Evelynn.Combo.Smite").GetValue<bool>();
            var useIgnite = Menu.ConfigMenu.Item("Evelynn.Combo.Ignite").GetValue<bool>();
            var targets = Menu.ConfigMenu.Item("Evelynn.Combo.R.Setting").GetValue<Slider>().Value;
            var healthW = Menu.ConfigMenu.Item("Evelynn.Combo.W.Setting").GetValue<Slider>().Value;
        
            Orbwalker.SetMovement(true);
  
            if (comboR && SpellDirectory[Spells.R].IsReady() && _currentTarget.Distance(Player) <= SpellDirectory[Spells.R].Range)                          
                {
                var bestposition = AutomatedLybrary.BestPosition(SpellDirectory[Spells.R], 250f, TargetSelector.DamageType.Magical);
                if (bestposition.CountEnemiesInRange(250f) >= targets)
                    {
                        SpellDirectory[Spells.R].Cast(bestposition);
                    }
                }

            if (comboSmite && !_currentTarget.HasBuffOfType(BuffType.Stun) && !_currentTarget.HasBuffOfType(BuffType.Slow)
                && _currentTarget.Distance(Player) <= 475f)
            {
                Player.Spellbook.CastSpell(_smite, _currentTarget);
            }

            Items(target);
       
            if (comboQ && SpellDirectory[Spells.Q].IsReady())   
                SpellDirectory[Spells.Q].Cast();           

            if (comboW && SpellDirectory[Spells.W].IsReady() && healthW <= Player.HealthPercent)
                SpellDirectory[Spells.W].Cast();

            if (Player.Distance(target) <= 600f && IgniteDamage(target) >= target.Health &&
                useIgnite)
            {
                Player.Spellbook.CastSpell(_ignite, target);
            }
        }
        #endregion

        #region Killsteal
        private static void Killsteal()
        {
            var ksE = Menu.ConfigMenu.Item("Evelynn.Misc.E.Killsteal").GetValue<bool>();

            if (ksE && SpellDirectory[Spells.E].IsReady())
            {
                foreach (
                        var enemy in
                            ObjectManager.Get<Obj_AI_Hero>()
                                .Where(enemy => enemy.IsValidTarget(SpellDirectory[Spells.E].Range) && 
                                    SpellDirectory[Spells.E].IsKillable(enemy)))
                {
                    SpellDirectory[Spells.E].CastOnUnit(enemy);
                }
            }          
        }


        #endregion

        #region AfterAttack
        private static void OrbwalkingAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Combo || (!unit.IsMe) ||
                !(_currentTarget.Distance(Player) <= SpellDirectory[Spells.E].Range)) return;

            var comboE = Menu.ConfigMenu.Item("Evelynn.Combo.E").GetValue<bool>();                   
            var usehydra = Menu.ConfigMenu.Item("Evelynn.Items.Hydra").GetValue<bool>();

            if (comboE && SpellDirectory[Spells.E].IsReady())
            {
                SpellDirectory[Spells.E].CastOnUnit(_currentTarget);
            }

            if (ItemDictionary[ItemData.Ravenous_Hydra_Melee_Only].IsReady() && ItemDictionary[ItemData.Ravenous_Hydra_Melee_Only].IsOwned(Player) 
                && usehydra)
            {
                ItemDictionary[ItemData.Ravenous_Hydra_Melee_Only].Cast(Player);
            }

            if (ItemDictionary[ItemData.Tiamat_Melee_Only].IsReady() && ItemDictionary[ItemData.Tiamat_Melee_Only].IsOwned(Player) && usehydra)
            {
                ItemDictionary[ItemData.Tiamat_Melee_Only].Cast(Player);
            }
        }
        #endregion

        #region itemusage
        private static void Items(Obj_AI_Hero target)
        {
            var useghostblade = Menu.ConfigMenu.Item("Evelynn.Items.Ghostblade").GetValue<bool>();            
            var usebotrk = Menu.ConfigMenu.Item("Evelynn.Items.Blade").GetValue<bool>();           
            var usescemitar = Menu.ConfigMenu.Item("Evelynn.Items.MercurialScimitar").GetValue<bool>();
            var useranduins = Menu.ConfigMenu.Item("Evelynn.Items.RanduinOmen").GetValue<bool>();
 
            var targets = Menu.ConfigMenu.Item("Evelynn.Items.RanduinOmen.Targets").GetValue<Slider>().Value;

            if (ItemDictionary[ItemData.Blade_of_the_Ruined_King].IsReady() &&
                ItemDictionary[ItemData.Blade_of_the_Ruined_King].IsOwned(Player) &&
                ItemDictionary[ItemData.Blade_of_the_Ruined_King].IsInRange(target) &&
                target.Health >= GetDamage(target) && usebotrk)
            {
                ItemDictionary[ItemData.Blade_of_the_Ruined_King].Cast(target);
            }

            if (ItemDictionary[ItemData.Bilgewater_Cutlass].IsReady() &&
                ItemDictionary[ItemData.Bilgewater_Cutlass].IsOwned(Player) &&
                ItemDictionary[ItemData.Bilgewater_Cutlass].IsInRange(target) && target.Health >= GetDamage(target) &&
                usebotrk)
            {
                ItemDictionary[ItemData.Bilgewater_Cutlass].Cast(target);
            }

            if (ItemDictionary[ItemData.Youmuus_Ghostblade].IsReady() &&
                ItemDictionary[ItemData.Youmuus_Ghostblade].IsOwned(Player) &&
                SpellDirectory[Spells.E].IsInRange(target) && target.Health >= GetDamage(target) && useghostblade)
            {
                ItemDictionary[ItemData.Youmuus_Ghostblade].Cast(target);
            }

            if (ItemDictionary[ItemData.Randuins_Omen].IsReady() &&
                ItemDictionary[ItemData.Randuins_Omen].IsOwned(Player) &&
                ItemDictionary[ItemData.Randuins_Omen].IsInRange(target) &&
                Player.CountEnemiesInRange(ItemDictionary[ItemData.Randuins_Omen].Range) >= targets
                && useranduins)
            {
                ItemDictionary[ItemData.Randuins_Omen].Cast();
            }

            if (!Player.HasBuffOfType(BuffType.Stun) && !Player.HasBuffOfType(BuffType.Charm) &&
                !Player.HasBuffOfType(BuffType.Polymorph) && !Player.HasBuffOfType(BuffType.Fear) &&
                !Player.HasBuffOfType(BuffType.Snare) && !Player.HasBuffOfType(BuffType.Suppression)) return;
            
            if (ItemDictionary[ItemData.Quicksilver_Sash].IsReady() &&
                ItemDictionary[ItemData.Quicksilver_Sash].IsOwned(Player) && usescemitar)
            {
                ItemDictionary[ItemData.Quicksilver_Sash].Cast();
            }
            if (ItemDictionary[ItemData.Mercurial_Scimitar].IsReady() &&
                ItemDictionary[ItemData.Mercurial_Scimitar].IsOwned(Player) && usescemitar)
            {
                ItemDictionary[ItemData.Mercurial_Scimitar].Cast();
            }
        }
        #endregion

        #region JungleClear
        private static void JungleClear()
        {
            var jClearQ = Menu.ConfigMenu.Item("Evelynn.jClear.Q").GetValue<bool>();
            var jClearE = Menu.ConfigMenu.Item("Evelynn.jClear.E").GetValue<bool>();                
            var usehydra = Menu.ConfigMenu.Item("Evelynn.Items.Hydra").GetValue<bool>();
           
            var minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, 500, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            Orbwalker.SetMovement(Menu.ConfigMenu.Item("Evelynn.jClear.M").GetValue<bool>());
            
             foreach (var minion in minions)
             {
                 if (SpellDirectory[Spells.Q].IsReady() && jClearQ)
                 {
                     SpellDirectory[Spells.Q].Cast();
                 }

                 if (SpellDirectory[Spells.E].IsReady() && jClearE)
                 {
                     SpellDirectory[Spells.E].CastOnUnit(minion);
                 }

                 if (ItemDictionary[ItemData.Ravenous_Hydra_Melee_Only].IsReady() &&
                     ItemDictionary[ItemData.Ravenous_Hydra_Melee_Only].IsOwned(Player)
                     && usehydra)
                 {
                     ItemDictionary[ItemData.Ravenous_Hydra_Melee_Only].Cast(Player);
                 }

                 if (ItemDictionary[ItemData.Tiamat_Melee_Only].IsReady() &&
                     ItemDictionary[ItemData.Tiamat_Melee_Only].IsOwned(Player)
                     && usehydra)
                 {
                     ItemDictionary[ItemData.Tiamat_Melee_Only].Cast();
                 }
             }
        }
        #endregion

        #region IgniteDamage
        private static float IgniteDamage(Obj_AI_Base target)
        {
            if (_ignite == SpellSlot.Unknown || Player.Spellbook.CanUseSpell(_ignite) != SpellState.Ready)
            {
                return 0f;
            }
            return (float)Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
        }
        #endregion
      
        #region Misc
        private static void Misc()
        {
            var usePotions = Menu.ConfigMenu.Item("Evelynn.Items.Potion").GetValue<bool>();
            var comboW = Menu.ConfigMenu.Item("Evelynn.Combo.W").GetValue<bool>();
            var potionHp = Menu.ConfigMenu.Item("Evelynn.Items.Potion.HP").GetValue<Slider>().Value;

            if (comboW && SpellDirectory[Spells.W].IsReady() && Player.HasBuffOfType(BuffType.Slow))
            {
                SpellDirectory[Spells.W].Cast();
            }

            if (usePotions && Player.HealthPercent < potionHp && !Player.HasBuff("RegenerationPotion", true)
            && !Player.HasBuff("ItemMiniRegenPotion", true) && !Player.IsRecalling() && !Player.InFountain())
            {
                if (ItemDictionary[ItemData.Health_Potion].IsOwned() && ItemDictionary[ItemData.Health_Potion].IsReady())
                {
                    ItemDictionary[ItemData.Health_Potion].Cast();
                } 

                if (ItemDictionary[ItemData.Total_Biscuit_of_Rejuvenation2].IsOwned() && ItemDictionary[ItemData.Total_Biscuit_of_Rejuvenation2].IsReady())
                {
                    ItemDictionary[ItemData.Total_Biscuit_of_Rejuvenation2].Cast();
                }
            }

            if (!Player.InShop()) return;
            
            if (ItemDictionary[ItemData.Stalkers_Blade].IsOwned())
                {
                    _smite = Player.GetSpellSlot("s5_summonersmiteplayerganker");
                }
           
            if (ItemDictionary[ItemData.Skirmishers_Sabre].IsOwned())
                {
                    _smite = Player.GetSpellSlot("s5_summonersmiteduel");
                }
                    
            if (Player.Level < 9) return;
            var buytrinket = Menu.ConfigMenu.Item("Evelynn.Misc.Trinket").GetValue<bool>();

            if (!ItemDictionary[ItemData.Sweeping_Lens_Trinket].IsOwned(Player) && 
                !ItemDictionary[ItemData.Oracles_Lens_Trinket].IsOwned(Player) && buytrinket)
            {
                ItemDictionary[ItemData.Sweeping_Lens_Trinket].Buy();
            }
            if (ItemDictionary[ItemData.Sweeping_Lens_Trinket].IsOwned(Player) && 
                !ItemDictionary[ItemData.Oracles_Lens_Trinket].IsOwned(Player) && 
                buytrinket)
            {
                ItemDictionary[ItemData.Oracles_Lens_Trinket].Buy();
            }
        }
        #endregion

        #region GetDamage
        private static float GetDamage(Obj_AI_Hero target)
        {
            float totaldamage = 0;
            float mana = 0;
            

            if (ItemDictionary[ItemData.Blade_of_the_Ruined_King].IsOwned(Player))
            {
                if (SpellDirectory[Spells.E].IsReady())
                {
                    totaldamage += Convert.ToSingle((target.Health * .06));
                }
                else
                {
                    totaldamage += Convert.ToSingle((target.Health * .02));
                }
            }

            if (Player.Mana > SpellDirectory[Spells.R].Instance.ManaCost)
            {
                totaldamage += SpellDirectory[Spells.R].GetDamage(target);
                mana += SpellDirectory[Spells.R].Instance.ManaCost;
            }

            if (SpellDirectory[Spells.Q].IsReady() && Player.Mana > (SpellDirectory[Spells.Q].Instance.ManaCost + mana))
            {
                totaldamage += SpellDirectory[Spells.Q].GetDamage(target);
                mana += SpellDirectory[Spells.Q].Instance.ManaCost;
            }

            if (SpellDirectory[Spells.E].IsReady() && Player.Mana > (SpellDirectory[Spells.E].Instance.ManaCost + mana))
            {
                totaldamage += SpellDirectory[Spells.E].GetDamage(target) + (Convert.ToSingle(Player.GetAutoAttackDamage(Player, true) * 2));
            }
            else
            {
                totaldamage += Convert.ToSingle(Player.GetAutoAttackDamage(Player, true));
            }
            return totaldamage;
        }
        #endregion
    }
}
