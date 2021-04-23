﻿using System;
using System.Collections.Generic;
using System.IO;

namespace CrateModLoader.GameSpecific.CrashTeamRacing
{
    public class CTR_Rand_Tracks101 : ModStruct<string>
    {
        public override string Name => "Randomize Track Order (101%)";
        public override string Description => "Shuffles all tracks (except bonus tracks) around.";

        public List<string> TrackFolderNames = new List<string>()
        {
            "arena2",
            "blimp1",
            "castle1",
            "cave1",
            "coco1",
            "desert2",
            "ice1",
            "island1",
            "labs1",
            "proto8",
            "proto9",
            "sewer1",
            "space",
            "temple1",
            "temple2",
            "tube1",
        };

        public override void ModPass(string path_extr)
        {
            if (!Directory.Exists(path_extr + @"levels\tracks\island1"))
            {
                return;
            }
            Random rand = new Random(ModLoaderGlobals.RandomizerSeed);

            List<int> LevelInd = new List<int>();
            List<int> LevelRand = new List<int>();
            int maxLevel = TrackFolderNames.Count;

            for (int i = 0; i < maxLevel; i++)
            {
                Directory.Move(path_extr + @"levels\tracks\" + TrackFolderNames[i], path_extr + @"levels\tracks\level" + i);
                LevelInd.Add(i);
            }

            while (LevelInd.Count > 0)
            {
                int r = rand.Next(LevelInd.Count);
                LevelRand.Add(LevelInd[r]);
                LevelInd.RemoveAt(r);
            }

            for (int i = 0; i < LevelRand.Count; i++)
            {
                Directory.Move(path_extr + @"levels\tracks\level" + i, path_extr + @"levels\tracks\" + TrackFolderNames[LevelRand[i]]);
            }
        }
    }
}
