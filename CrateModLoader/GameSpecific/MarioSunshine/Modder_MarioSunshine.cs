﻿using System.Collections.Generic;

namespace CrateModLoader.GameSpecific.MarioSunshine
{
    public sealed class Modder_MarioSunshine : Modder
    {
        public Modder_MarioSunshine()
        {
            Game = new Game()
            {
                Name = "Super Mario Sunshine",
                ShortName = "MarioSunshine",
                Consoles = new List<ConsoleMode>
                {
                    ConsoleMode.GCN
                },
                API_Credit = string.Empty,
                API_Link = string.Empty,
                Icon = null,
                ModCratesSupported = true,
                RegionID_GCN = new RegionCode[] {
                    new RegionCode() {
                    Name = "GMSE",
                    Region = RegionType.NTSC_U },
                    new RegionCode() {
                    Name = "GMSP",
                    Region = RegionType.PAL },
                    new RegionCode() {
                    Name = "GMSJ",
                    Region = RegionType.NTSC_J },
                },
            };
        }

        public override void StartModProcess()
        {
            // TODO
            ModProcess();
        }

        protected override void ModProcess()
        {
            // TODO
            EndModProcess();
        }

        protected override void EndModProcess()
        {
            // TODO
        }
    }
}
