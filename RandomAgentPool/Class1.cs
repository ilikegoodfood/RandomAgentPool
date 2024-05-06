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

        public static int opt_keepPercentageUnique = 70;

        public static int opt_keepPercentageGeneric = 100;

        public static bool opt_brokenMaker = false;

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
                case "Enable for Broken Maker":
                    opt_brokenMaker = value;
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
                default:
                    break;
            }
        }

        public override void afterMapGenAfterHistorical(Map map)
        {
            if (map.overmind.god is God_Eternity && !opt_brokenMaker)
            {
                return;
            }

            if (opt_keepPercentageUnique < 100)
            {
                if (opt_rollIndividuallyUnique)
                {
                    UAE_Abstraction keptUnique = map.overmind.agentsUnique[Eleven.random.Next(map.overmind.agentsUnique.Count)];

                    foreach (UAE_Abstraction uniqueAgent in map.overmind.agentsUnique.ToList())
                    {
                        if (Eleven.random.Next(100) >= opt_keepPercentageUnique)
                        {
                            map.overmind.agentsUnique.Remove(uniqueAgent);
                        }
                    }

                    if (map.overmind.agentsUnique.Count == 0 && opt_keepPercentageUnique > 0)
                    {
                        map.overmind.agentsUnique.Add(keptUnique);
                    }
                }
                else
                {
                    int uniqueCount = map.overmind.agentsUnique.Count;
                    int removalCount = uniqueCount - (int)Math.Floor(uniqueCount * opt_keepPercentageUnique * 0.01);

                    if (removalCount < uniqueCount)
                    {
                        while (removalCount > 0)
                        {
                            map.overmind.agentsUnique.RemoveAt(Eleven.random.Next(map.overmind.agents.Count));

                            removalCount--;
                        }
                    }
                    else
                    {
                        UAE_Abstraction keptUnique = map.overmind.agentsUnique[Eleven.random.Next(uniqueCount)];
                        map.overmind.agentsUnique.Clear();

                        if (opt_keepPercentageUnique > 0)
                        {
                            map.overmind.agentsUnique.Add(keptUnique);
                        }
                    }
                }
            }

            if (opt_keepPercentageGeneric < 100)
            {
                if (opt_rollIndividuallyGeneric)
                {
                    UAE_Abstraction keptGeneric = map.overmind.agentsGeneric[Eleven.random.Next(map.overmind.agentsGeneric.Count)];

                    foreach (UAE_Abstraction genericAgent in map.overmind.agentsGeneric.ToList())
                    {
                        if (Eleven.random.Next(100) >= opt_keepPercentageGeneric)
                        {
                            map.overmind.agentsGeneric.Remove(genericAgent);
                        }
                    }

                    if (map.overmind.agentsGeneric.Count == 0)
                    {
                        map.overmind.agentsGeneric.Add(keptGeneric);
                    }
                }
                else
                {
                    int genericCount = map.overmind.agentsGeneric.Count;
                    int removalCount = genericCount - (int)Math.Floor(genericCount * opt_keepPercentageGeneric * 0.01);

                    if (removalCount < genericCount)
                    {
                        while (removalCount > 0)
                        {
                            map.overmind.agentsGeneric.RemoveAt(Eleven.random.Next(map.overmind.agents.Count));

                            removalCount--;
                        }
                    }
                    else
                    {
                        UAE_Abstraction keptGeneric = map.overmind.agentsGeneric[Eleven.random.Next(genericCount)];
                        map.overmind.agentsGeneric.Clear();
                        map.overmind.agentsGeneric.Add(keptGeneric);
                    }
                }
            }
        }
    }
}
