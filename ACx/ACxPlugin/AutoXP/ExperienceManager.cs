using ACxPlugin.AutoXP;
using Decal.Adapter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace ACxPlugin
{
    public class ExperienceManager : Module
    {
        private const int MAX_XP_CHUNK = 1000000000;
        private Timer timer;
        private List<KeyValuePair<ExpTarget, int>> flatPlan { get; set; }
        private int planIndex { get; set; } = 0;
        private ExperiencePolicy policy;

        public ExperienceManager(PluginLogic plugin) : base(plugin) { }

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

        /// <summary>
        /// Creates a plan to spend all unassigned experience
        /// </summary>
        /// <returns></returns>
        public Dictionary<ExpTarget, List<int>> GetPlan()
        {
            return GetPlan(CoreManager.Current.CharacterFilter.UnassignedXP);
        }

        //Plan to spend a set amount of XP, up to some max steps in the plan
        public Dictionary<ExpTarget, List<int>> GetPlan(long expToSpend, int maxSteps = 9999)
        {
            var plan = new Dictionary<ExpTarget, List<int>>();
            var weightedCosts = new Dictionary<ExpTarget, double>();

			//Utils.WriteToChat($"Creating a plan to spend {expToSpend} up to {maxSteps} times on {policy.Weights.Count} targets of exp.");

			//Utils.WriteToChat($"Initial costs/weights/weighted costs:");
			//Find what exp targets are candidates to be leveled
			foreach (var t in policy.Weights)
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
                    weightedCosts.Add(t.Key, cost / policy.Weights[t.Key]);

                    //Utils.WriteToChat($"  {t.Key}: {cost} \t {Weights[t.Key]} \t {cost / Weights[t.Key]}");
                }
                catch (Exception e)
                {
                    Utils.LogError(e);
                }
            }

            //Break if nothing left to level
            if (flatPlan.Count == 0)
                return plan;


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
                    var newWeightedCost = nextCost / policy.Weights[nextTarget];
                    weightedCosts[nextTarget] = newWeightedCost;
                }
            }

            return plan;
        }

        /// <summary>
        /// Prints out what would be leveled using all unassigned xp
        /// </summary>
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

        /// <summary>
        /// Creates and initiates a plan to spend experience, or halts an already running plan.
        /// </summary>
        /// <param name="batchLevels">Spends up to MAX_XP_CHUNK to level multiple times at once</param>
        public void SpendExperience(bool batchLevels = false)
        {
            SpendExperience(CoreManager.Current.CharacterFilter.UnassignedXP, batchLevels);
        }
        public void SpendExperience(long expToSpend, bool batchLevels = false)
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

            //Check for nothing left to level
            if(flatPlan.Count == 0)
			{
                Utils.WriteToChat($"Found nothing left to level from {policy.Weights.Count} targets.");
                return;
			}

            //Sort by cost?
            flatPlan = flatPlan.OrderBy(t => t.Value).ToList();

            //Start at beginning of plan
            planIndex = 0;

            Utils.WriteToChat($"Spending on a plan consisting of {flatPlan.Count} steps with {expToSpend} available exp.");

            //Start leveling
            timer.Enabled = true;
        }

        public void PrintPolicy()
        {
            Utils.WriteToChat("Current experience policy weights:");
            foreach (var kvp in policy.Weights)
                Utils.WriteToChat(kvp.Key + ": " + kvp.Value);
        }



        //Set instance on startup for lazy access through CommandManager
        public static ExperienceManager Instance { get; set; }
        public override void Startup()
        {
            Utils.WriteToChat("Setting up experience policy...");
            Instance = this;
            timer = new Timer() { AutoReset = true, Enabled = false, Interval = Plugin.Config.Interval};
            timer.Elapsed += SpendExperienceTick;
            policy = Plugin.Profile.ExpPolicy;
        }
        public override void Shutdown()
        {
            Utils.WriteToChat("Shutting down experience policy...");
            Instance = null;
            timer.Enabled = false;
            timer.Elapsed -= SpendExperienceTick;
        }

    }
}
