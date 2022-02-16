﻿using System;
using System.Collections.Generic;
using System.Numerics;
using CrateModLoader.LevelAPI;
using Crash;

namespace CrateModLoader.GameSpecific.Crash1.TrilogyCommon
{
    public class SceneryData_NSF_Old : CollisionData<OldSceneryEntry>
    {
        public override void Load(OldSceneryEntry entry)
        {

            Vertices = new List<Vector4>();
            for (int i = 0; i < entry.Vertices.Count; i++)
            {
                float X = (entry.Vertices[i].X + entry.XOffset) / 100f;
                float Y = (entry.Vertices[i].Y + entry.YOffset) / 100f;
                float Z = (entry.Vertices[i].Z + entry.ZOffset) / 100f;
                Vertices.Add(new Vector4(X, Y, Z, 1));
            }
            Indices = new List<int>();
            for (int i = 0; i < entry.Polygons.Count; i++)
            {
                Indices.Add(entry.Polygons[i].VertexA);
                Indices.Add(entry.Polygons[i].VertexB);
                Indices.Add(entry.Polygons[i].VertexC);
            }

            Normals = new List<System.Numerics.Vector3>();
            for (int i = 0; i < Vertices.Count; i++)
            {
                Vector3 normal = new Vector3(0.5f, 0.5f, 0.5f);
                Normals.Add(normal);
            }

        }
    }
}
