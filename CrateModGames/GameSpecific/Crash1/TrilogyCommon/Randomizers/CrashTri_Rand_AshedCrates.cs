﻿using Crash;
using System;
using System.Collections.Generic;
using CrateModGames.GameSpecific.Crash1.TrilogyCommon;

namespace CrateModLoader.GameSpecific.Crash1.TrilogyCommon
{
    public class CrashTri_Rand_AshedCrates : ModStruct<NSF_Pair>
    {
        public override string Name => "All Crates Are Covered Up";
        public override string Description => "All crates are covered up to look the same on the outside in each level.";

        private Random rand;
        private bool isRandom;

        public CrashTri_Rand_AshedCrates(bool isrand)
        {
            isRandom = isrand;
        }

        public override void BeforeModPass()
        {
            rand = new Random(ModLoaderGlobals.RandomizerSeed);
        }

        public override void ModPass(NSF_Pair pair)
        {
            // todo: Crash 1 support
            /*
            foreach (OldModelEntry model in nsf.GetEntries<OldModelEntry>())
            {
                
            }
            */
            // easier to just cover up the model when all are ashed
            // todo: per entity model switch (using outline crate flag stuff?)
            foreach (ModelEntry model in pair.nsf.GetEntries<ModelEntry>())
            {
                if (model.EName.StartsWith("B") && (model.EName.EndsWith("10G") || model.EName.EndsWith("20G") || model.EName.EndsWith("30G") || model.EName.EndsWith("40G")))
                {
                    if (!isRandom || (isRandom && rand.Next(2) == 0))
                    {
                        for (int i = 0; i < model.Colors.Count; ++i)
                        {
                            model.Colors[i] = new SceneryColor(0, 0, 0, model.Colors[i].Extra);
                        }
                    }
                }
            }
        }
    }
}