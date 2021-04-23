﻿using System;
using System.Collections.Generic;
using CrateModLoader.ModProperties;
using CrateModGames.GameSpecific.Crash1.TrilogyCommon;
using CrateModGames.GameSpecific.Crash3;
using CrateModLoader.GameSpecific.Crash1.TrilogyCommon;
//using CrateModLoader.GameSpecific.Crash3.Mods;

namespace CrateModLoader.GameSpecific.Crash3
{
    [ModCategory(1), ModMenuOnly]
    static class Crash3_Props_Misc
    {
        public static ModPropColor Prop_PantsColor = new ModPropColor(new CrashTri_Rand_PantsColor(false), new int[4] { 0, 0, 255, 255 }, CrashTri_Text.Prop_PantsColor, CrashTri_Text.Prop_PantsColorDesc);

        // less used
        public static ModPropOption Option_RandCrates = new ModPropOption(new CrashTri_Rand_WoodenCrates());
        public static ModPropOption Option_AllEnemiesMissing = new ModPropOption(new CrashTri_Rand_RemoveEnemies(false), CrashTri_Text.Mod_RemoveEnemies, CrashTri_Text.Mod_RemoveEnemies);
        public static ModPropOption Option_RandObjCol = new ModPropOption(new CrashTri_Rand_ObjectColors());
        public static ModPropOption Option_RandObjTex = new ModPropOption(new CrashTri_Rand_ObjectTextures());
        public static ModPropOption Option_UncoloredObj = new ModPropOption(new CrashTri_Objects_Greyscale());
        public static ModPropOption Option_AllCratesBlank = new ModPropOption(new CrashTri_Crates_AllBlank());
        public static ModPropOption Option_AllCratesWumpa = new ModPropOption(new CrashTri_Rand_CratesIntoWumpa(false));
        public static ModPropOption Option_RandCameraFOV = new ModPropOption(new CrashTri_Rand_CameraFOV(true), CrashTri_Text.Rand_CameraFOV, CrashTri_Text.Rand_CameraFOVDesc);
        public static ModPropOption Option_InvisibleWorld = new ModPropOption(new CrashTri_Scenery_Invisible());

        //unfinished
        public static ModPropOption Option_AllEnemiesAreCrates = new ModPropOption(new CrashTri_Rand_EnemyCrates(false), Crash3_Text.Mod_EnemyCrates, Crash3_Text.Mod_EnemyCratesDesc) { Hidden = true };
    }
}
