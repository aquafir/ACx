using Decal.Adapter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace ACxPlugin
{
    public class ExperiencePolicy
    {
        private const int MAX_XP_CHUNK = 1000000000;

        private Timer timer;
        private List<KeyValuePair<ExpTarget, int>> flatPlan { get; set; }
        private int planIndex { get; set; } = 0;

        /// <summary>
        /// Weights of how to spend the 
        /// </summary>
        public Dictionary<ExpTarget, double> Weights { get; set; } = new Dictionary<ExpTarget, double>();

        public ExperiencePolicy() { }

        /// <summary>
        /// Spends experience on the next target in the plan
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SpendExperienceTick(object sender, ElapsedEventArgs e)
        {
            //Utils.WriteToChat($"{FlatPlan.Count} | {PlanIndex} | {FlatPlan.Count < PlanIndex}");
            //Check if plan finished
            if (flatPlan.Count <= planIndex)
            {
                Utils.WriteToChat($"Finished leveling {planIndex} targets.");
                timer.Enabled = false;
            }
            //Otherwise level the next thing
            else
            {
                var target = flatPlan[planIndex].Key;
                var exp = flatPlan[planIndex].Value;
                //Utils.WriteToChat($"Adding {exp} to {target}");
                target.AddExp(exp);
                planIndex++;
            }
        }

        public Dictionary<ExpTarget, List<int>> GetPlan()
        {
            return GetPlan(CoreManager.Current.CharacterFilter.UnassignedXP);
        }
        public Dictionary<ExpTarget, List<int>> GetPlan(long expToSpend, int maxSteps = 9999)
        {
            var plan = new Dictionary<ExpTarget, List<int>>();
            var weightedCosts = new Dictionary<ExpTarget, double>();
            //ExpTarget.Alchemy.CostToLevel()

            //Utils.WriteToChat($"Creating a plan to spend {expToSpend} up to {maxSteps} times on {Weights.Count} targets of exp.");


            //Utils.WriteToChat($"Initial costs/weights/weighted costs:");
            //Find what exp targets are candidates to be leveled
            foreach (var t in Weights)
            {
                //Skip invalid weights
                if (t.Value <= 0)
                    continue;

                try
                {
                    var cost = t.Key.CostToLevel() ?? -1;

                    //Continue if no known cost to level
                    if (cost < 0)
                    {
                        //Utils.WriteToChat($"  {t.Key}: n/a");
                        continue;
                    }

                    //Otherwise consider it for spending exp on
                    plan.Add(t.Key, new List<int>());

                    //Figure out initial weighted cost of exp target
                    weightedCosts.Add(t.Key, cost / Weights[t.Key]);

                    //Utils.WriteToChat($"  {t.Key}: {cost} \t {Weights[t.Key]} \t {cost / Weights[t.Key]}");
                }
                catch (Exception e)
                {
                    Utils.LogError(e);
                }
            }

            for (var i = 0; i < maxSteps; i++)
            {
                //Get the most efficient thing to spend exp on as determined by weighted cost
                var nextTarget = weightedCosts.OrderBy(t => t.Value).First().Key;

                //Find cost of leveling that skill after the steps previously taken in the plan
                var timesLeveled = plan[nextTarget].Count;
                var cost = nextTarget.CostToLevel(timesLeveled) ?? -1;

                //Halt if there is insufficient exp or no more levels
                if (expToSpend < cost || cost == -1)
                {
                    break;
                }

                //Add to plan
                plan[nextTarget].Add(cost);
                //Simulate use of that exp
                expToSpend -= cost;

                //Update weighted cost
                var nextCost = nextTarget.CostToLevel(timesLeveled + 1) ?? -1;
                //TODO: Improve logic here.  If there's no next level, set weight cost to max value
                if (nextCost == -1)
                {
                    weightedCosts[nextTarget] = double.PositiveInfinity;
                }
                else
                {
                    var newWeightedCost = nextCost / Weights[nextTarget];
                    weightedCosts[nextTarget] = newWeightedCost;
                }
            }

            return plan;
        }

        public void PrintExperiencePlan()
        {
            PrintExperiencePlan(CoreManager.Current.CharacterFilter.UnassignedXP);
        }

        public void PrintExperiencePlan(long expToSpend)
        {
            var plan = GetPlan(expToSpend);

            Utils.WriteToChat($"Experience plan for {expToSpend} exp:");
            foreach (var t in plan)
            {
                var steps = t.Value.Count;
                var description = new StringBuilder($"{t.Key.ToString()} ({steps}): ");

                for (var i = 0; i < t.Value.Count; i++)
                {
                    description.Append($"{t.Value[i]}\t");
                }

                Utils.WriteToChat(description.ToString());
            }
        }

        internal void SpendExperience(bool batchLevels = false)
        {
            SpendExperience(CoreManager.Current.CharacterFilter.UnassignedXP, batchLevels);
        }

        internal void SpendExperience(long expToSpend, bool batchLevels = false)
        {
            //Already spending experience?  Continue?
            if (timer.Enabled)
            {
                Utils.WriteToChat("Stopping spending experience.");
                timer.Enabled = false;
                flatPlan = null;
                return;
            }

            //Get plan for leveling
            flatPlan = new List<KeyValuePair<ExpTarget, int>>();
            foreach (var steps in GetPlan(expToSpend))
            {
                if (batchLevels)
                {                  
                    long totalXp = 0;
                    foreach (var step in steps.Value)
                    {
                        totalXp += step;
                    }
                    Utils.WriteToChat($"Spending {totalXp:n0} to level {steps.Key.GetName()} {steps.Value.Count()} times from {steps.Key.GetTimesLeveled()} to {steps.Key.GetTimesLeveled() + steps.Value.Count()}");
                    while (totalXp > MAX_XP_CHUNK)
					{
                        flatPlan.Add(new KeyValuePair<ExpTarget, int>(steps.Key, MAX_XP_CHUNK));
                        totalXp -= MAX_XP_CHUNK;
                    }
                    if (totalXp > 0)
                    {
                        flatPlan.Add(new KeyValuePair<ExpTarget, int>(steps.Key, (int)totalXp));
                    }
                }
                else
                {
                    for (var i = 0; i < steps.Value.Count; i++)
                    {
                        flatPlan.Add(new KeyValuePair<ExpTarget, int>(steps.Key, steps.Value[i]));
                    }
                }
            }

            //Sort by cost?
            flatPlan = flatPlan.OrderBy(t => t.Value).ToList();

            //Start at beginning of plan
            planIndex = 0;

            Utils.WriteToChat($"Spending on a plan consisting of {flatPlan.Count} steps with {expToSpend} available exp.");

            //Start leveling
            timer.Enabled = true;
        }

        internal void PrintPolicy()
        {
            Utils.WriteToChat("Current experience policy weights:");
            foreach (var kvp in Weights)
                Utils.WriteToChat(kvp.Key + ": " + kvp.Value);
        }

        /// <summary>
        /// Behavior when logging off.
        /// </summary>
        internal void Shutdown()
        {
            timer.Enabled = false;
            timer.Elapsed -= SpendExperienceTick;
        }
        public void Startup(int interval)
        {
            timer = new Timer() { AutoReset = true, Enabled = false, Interval = interval };
            timer.Elapsed += SpendExperienceTick;
        }

        public static ExperiencePolicy Default
        {
            get
            {
                return new ExperiencePolicy()
                {
                    Weights = new Dictionary<ExpTarget, double>
                    {
                //Attributes
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
                //Skills
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
