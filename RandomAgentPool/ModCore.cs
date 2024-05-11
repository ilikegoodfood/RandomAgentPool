using Assets.Code;
using Assets.Code.Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandomAgentPool
{
    public class ModCore : ModKernel
    {
        public bool opt_rollIndividuallyUnique = false;

        public static bool opt_rollIndividuallyGeneric = false;

        public static bool opt_exactCountUnique = false;

        public static bool opt_exactCountGeneric = false;

        public static int opt_keepPercentageUnique = 70;

        public static int opt_keepPercentageGeneric = 100;

        public static int opt_keepNumberUnique = 7;

        public static int opt_keepNumberGeeric = 3;

        public List<UAE_Abstraction> uniqueAgents_Master;

        public List<UAE_Abstraction> genericAgents_Master;

        public God_Eternity brokenMaker = null;

        public bool acceleratedTime = false;

        public bool brokenMakerSleeping = false;

        public int brokenMakerSleepDuration;

        public int sleepDurationBase() => 50;

        public override void receiveModConfigOpts_bool(string optName, bool value)
        {
            switch (optName)
            {
                case "Roll Individually for Unique Agents":
                    opt_rollIndividuallyUnique = value;
                    break;
                case "Roll Individually for Generic Agents":
                    opt_rollIndividuallyGeneric = value;
                    break;
                case "Exact Count for Unique Agents":
                    opt_exactCountUnique = value;
                    break;
                case "Exact Count for Generic Agents":
                    opt_exactCountGeneric = value;
                    break;
                default:
                    break;
            }
        }

        public override void receiveModConfigOpts_int(string optName, int value)
        {
            switch (optName)
            {
                case "Percentage of Unique Agents Kept":
                    opt_keepPercentageUnique = value;
                    break;
                case "Percentage of Generic Agents Kept":
                    opt_keepPercentageGeneric = value;
                    break;
                case "Number of Unique Agents Kept":
                    opt_keepNumberUnique = value;
                    break;
                case "Number of Generic Agents Kept":
                    opt_keepNumberGeeric = value;
                    break;
                default:
                    break;
            }
        }

        public override void beforeMapGen(Map map)
        {
            if (map.overmind.god is God_Eternity eternal)
            {
                brokenMaker = eternal;

                brokenMakerSleepDuration = sleepDurationBase();
            }
        }

        public override void afterMapGenAfterHistorical(Map map)
        {
            if (brokenMaker != null)
            {
                uniqueAgents_Master = new List<UAE_Abstraction>();
                uniqueAgents_Master.AddRange(map.overmind.agentsUnique);

                genericAgents_Master = new List<UAE_Abstraction>();
                genericAgents_Master.AddRange(map.overmind.agentsGeneric);
            }

            
            if (opt_rollIndividuallyUnique)
            {
                trimIndividuallyToPercentage(map.overmind.agentsUnique, opt_keepPercentageUnique);
            }
            else if (opt_exactCountUnique)
            {
                trimToCount(map.overmind.agentsUnique, opt_keepNumberUnique);
            }
            else
            {
                trimToPercentage(map.overmind.agentsUnique, opt_keepPercentageUnique);
            }

            if (opt_rollIndividuallyGeneric)
            {
                trimIndividuallyToPercentage(map.overmind.agentsGeneric, opt_keepPercentageGeneric, true);
            }
            else if (opt_exactCountGeneric)
            {
                trimToCount(map.overmind.agentsGeneric, opt_keepNumberGeeric, true);
            }
            else
            {
                trimToPercentage(map.overmind.agentsGeneric, opt_keepPercentageGeneric, true);
            }
        }

        public void trimToPercentage(List<UAE_Abstraction> agentPool, int percentage, bool keepOne = false)
        {
            if (percentage >= 100)
            {
                return;
            }

            int agentCount = agentPool.Count;
            int removalCount = agentCount - (int)Math.Floor(agentCount * percentage * 0.01);

            if (removalCount < agentCount)
            {
                while (removalCount > 0)
                {
                    agentPool.RemoveAt(Eleven.random.Next(agentPool.Count));

                    removalCount--;
                }
            }
            else
            {
                UAE_Abstraction keptUnique = agentPool[Eleven.random.Next(agentCount)];
                agentPool.Clear();

                if (percentage > 0 || keepOne)
                {
                    agentPool.Add(keptUnique);
                }
            }
        }

        public void trimToCount(List<UAE_Abstraction> agentPool, int count, bool keepOne = false)
        {
            if (count >= agentPool.Count)
            {
                return;
            }

            UAE_Abstraction keptAgent = agentPool[Eleven.random.Next(agentPool.Count)];

            if (count == 0)
            {
                agentPool.Clear();

                if (keepOne)
                {
                    agentPool.Add(keptAgent);
                }
            }
            else
            {
                while (agentPool.Count > count)
                {
                    agentPool.RemoveAt(Eleven.random.Next(agentPool.Count));
                }
            }
        }

        public void trimIndividuallyToPercentage(List<UAE_Abstraction> agentPool, int percentage, bool keepOne = false)
        {
            if (percentage >= 100)
            {
                return;
            }

            UAE_Abstraction keptAgent = agentPool[Eleven.random.Next(agentPool.Count)];

            foreach (UAE_Abstraction uniqueAgent in agentPool.ToList())
            {
                if (Eleven.random.Next(100) >= percentage)
                {
                    agentPool.Remove(uniqueAgent);
                }
            }

            if (agentPool.Count == 0 && (percentage > 0 || keepOne))
            {
                agentPool.Add(keptAgent);
            }
        }

        public override void onTurnEnd(Map map)
        {
            if (brokenMaker != null)
            {
                if (map.acceleratedTime != acceleratedTime)
                {
                    acceleratedTime = map.acceleratedTime;

                    if (acceleratedTime)
                    {
                        brokenMakerSleeping = true;
                        onBrokenMakerSleep_StartOfSleep(map);
                    }
                }

                if (brokenMakerSleeping)
                {
                    brokenMakerSleepDuration--;
                    onBrokenMakerSleep_TurnTick(map);

                    if (brokenMakerSleepDuration == 0)
                    {
                        brokenMakerSleeping = false;
                        onBrokenMakerSleep_EndOfSleep(map);
                        brokenMakerSleepDuration = sleepDurationBase();
                    }
                }
            }
        }

        public void onBrokenMakerSleep_StartOfSleep(Map map)
        {
            List<UAE_Abstraction> agentPool = new List<UAE_Abstraction>();
            agentPool.AddRange(uniqueAgents_Master);

            foreach (UAE_Abstraction agent in brokenMaker.agentBuffer2)
            {
                agentPool.Remove(agent);
            }

            if (opt_rollIndividuallyUnique)
            {
                trimIndividuallyToPercentage(agentPool, opt_keepPercentageUnique);
            }
            else if (opt_exactCountUnique)
            {
                trimToCount(agentPool, opt_keepNumberUnique);
            }
            else
            {
                trimToPercentage(agentPool, opt_keepPercentageUnique);
            }

            map.overmind.agentsUnique.Clear();
            map.overmind.agentsUnique.AddRange(agentPool);

            map.overmind.agentsGeneric.Clear();
            map.overmind.agentsGeneric.AddRange(genericAgents_Master);

            if (opt_rollIndividuallyGeneric)
            {
                trimIndividuallyToPercentage(map.overmind.agentsGeneric, opt_keepPercentageGeneric, true);
            }
            else if (opt_exactCountGeneric)
            {
                trimToCount(map.overmind.agentsGeneric, opt_keepNumberGeeric, true);
            }
            else
            {
                trimToPercentage(map.overmind.agentsGeneric, opt_keepPercentageGeneric, true);
            }
        }

        public void onBrokenMakerSleep_TurnTick(Map map)
        {

        }

        public void onBrokenMakerSleep_EndOfSleep(Map map)
        {

        }
    }
}
