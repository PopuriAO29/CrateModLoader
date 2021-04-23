﻿using System;
using System.Collections.Generic;
using System.IO;
using CrateModGames.GameSpecific.Rayman3;

namespace CrateModLoader.GameSpecific.Rayman3
{
    public class Ray3_RemoveIntroVideo : ModStruct<string>
    {
        public override string Name => Rayman3_Text.Mod_RemoveIntroVideo;
        public override string Description => Rayman3_Text.Mod_RemoveIntroVideoDesc;

        public override void ModPass(string basePath)
        {
            if (Directory.Exists(basePath + "videos"))
            {
                if (File.Exists(basePath + @"videos\trailer.h4m"))
                {
                    File.Delete(basePath + @"videos\trailer.h4m");
                }
            }
        }
    }
}
