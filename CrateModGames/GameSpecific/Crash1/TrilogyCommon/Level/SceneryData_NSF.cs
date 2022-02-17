﻿using System;
using System.Collections.Generic;
using System.Numerics;
using System.ComponentModel;
using CrateModLoader.LevelAPI;
using Crash;

namespace CrateModLoader.GameSpecific.Crash2
{
    public class SceneryData_NSF : LevelObjectData<SceneryEntry>
    {
        [Browsable(false)]
        public override ObjectVector3 Scale { get; set; } = new ObjectVector3(1, 1, 1);
        [Browsable(false)]
        public override ObjectVector3 Rotation { get; set; } = new ObjectVector3(0, 0, 0);
        [Browsable(false)]
        public override ObjectVector3 WorldScale => new ObjectVector3(0.004f);

        public override void Load(SceneryEntry entry)
        {
            Name = entry.EName;

            Visual = EditorVisual.Custom;
            VisualData = new CollisionDataBase();

            VisualData.Vertices = new List<Vector4>();
            for (int i = 0; i < entry.Vertices.Count; i++)
            {
                float X = (entry.Vertices[i].X << 4);
                float Y = (entry.Vertices[i].Y << 4);
                float Z = (entry.Vertices[i].Z << 4);
                VisualData.Vertices.Add(new Vector4(X, Y, Z, 1));
            }
            VisualData.Indices = new List<int>();
            for (int i = 0; i < entry.Triangles.Count; i++)
            {
                VisualData.Indices.Add(entry.Triangles[i].VertexA);
                VisualData.Indices.Add(entry.Triangles[i].VertexB);
                VisualData.Indices.Add(entry.Triangles[i].VertexC);
            }

            VisualData.Normals = new List<System.Numerics.Vector3>();
            for (int i = 0; i < VisualData.Vertices.Count; i++)
            {
                Vector3 normal = new Vector3(0.5f, 0.5f, 0.5f);
                VisualData.Normals.Add(normal);
            }

            Position = new ObjectVector3(entry.XOffset, entry.YOffset, entry.ZOffset);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
