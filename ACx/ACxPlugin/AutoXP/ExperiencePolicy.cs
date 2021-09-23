using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ACxPlugin.AutoXP
{
	public class ExperiencePolicy
	{
		/// <summary>
		/// Weights for how to spend experience
		/// </summary>
		public Dictionary<ExpTarget, double> Weights { get; set; } = new Dictionary<ExpTarget, double>();

		public static ExperiencePolicy Default
		{
			get
			{
				return new ExperiencePolicy()
				{
					Weights = new Dictionary<ExpTarget, double>
					{
                //Attributes - Default to neutral
                { ExpTarget.Strength, 1},
				{ ExpTarget.Endurance, 1},
				{ ExpTarget.Coordination, 1},
				{ ExpTarget.Quickness, 1},
				{ ExpTarget.Focus, 1},
				{ ExpTarget.Self, 1},
                //Vitals
                { ExpTarget.Health, 1.4},
				{ ExpTarget.Stamina, .1},
				{ ExpTarget.Mana, .1},
                //Skills -- Default to low secondary, high primary
                { ExpTarget.Alchemy, 0},
				{ ExpTarget.ArcaneLore, .1},
				{ ExpTarget.ArmorTinkering, 0},
				{ ExpTarget.AssessCreature, 0},
				{ ExpTarget.AssessPerson, 0},
				{ ExpTarget.Cooking, 0},
				{ ExpTarget.CreatureEnchantment, .2},
				{ ExpTarget.Deception, .1},
				{ ExpTarget.DirtyFighting, .1},
				{ ExpTarget.DualWield, .1},
				{ ExpTarget.FinesseWeapons, 10},
				{ ExpTarget.Fletching, .1},
				{ ExpTarget.Healing, .1},
				{ ExpTarget.HeavyWeapons, 10},
				{ ExpTarget.ItemEnchantment, .2},
				{ ExpTarget.ItemTinkering, 0},
				{ ExpTarget.Jump, .02},
				{ ExpTarget.Leadership, .1},
				{ ExpTarget.LifeMagic, 1},
				{ ExpTarget.LightWeapons, 10},
				{ ExpTarget.Lockpick, 0},
				{ ExpTarget.Loyalty, .1},
				{ ExpTarget.MagicDefense, .1},
				{ ExpTarget.MagicItemTinkering, 0},
				{ ExpTarget.ManaConversion, .1},
				{ ExpTarget.MeleeDefense, 5},
				{ ExpTarget.MissileDefense, 5},
				{ ExpTarget.MissileWeapons, 10},
				{ ExpTarget.Recklessness, .1},
				{ ExpTarget.Run, .1},
				{ ExpTarget.Salvaging, .1},
				{ ExpTarget.Shield, 0},
				{ ExpTarget.SneakAttack, .1},
				{ ExpTarget.Summoning, 1},
				{ ExpTarget.TwoHandedCombat, 10},
				{ ExpTarget.VoidMagic, 10},
				{ ExpTarget.WarMagic, 10},
				{ ExpTarget.WeaponTinkering, 0}
			}
				};
			}
		}

	}
}
