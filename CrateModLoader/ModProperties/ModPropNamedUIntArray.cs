﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;

namespace CrateModLoader.ModProperties
{
    public class ModPropNamedUIntArray : ModProperty<uint[]>
    {

        public string[] PropNames;

        public ModPropNamedUIntArray(uint[] f, string[] names) : base(f)
        {
            PropNames = names;
            DefaultValue = new uint[Value.Length];
            for (int i = 0; i < Value.Length; i++)
            {
                DefaultValue[i] = Value[i];
            }
        }
        public ModPropNamedUIntArray(uint[] f, string[] names, string name, string desc) : base(f, name, desc)
        {
            PropNames = names;
            DefaultValue = new uint[Value.Length];
            for (int i = 0; i < Value.Length; i++)
            {
                DefaultValue[i] = Value[i];
            }
        }

        private List<NumericUpDown> nums = new List<NumericUpDown>();

        public override void GenerateUI(Control parent, ref int offset)
        {
            base.GenerateUI(parent, ref offset);

            nums.Clear();

            TabControl tabControl = new TabControl();
            tabControl.TabPages.Clear();
            tabControl.Parent = parent;
            //tabControl.Multiline = true;
            tabControl.Dock = DockStyle.Fill;
            tabControl.MouseEnter += FocusUI;

            int x_offset = 10;
            int size = 150;

            for (int i = 0; i < Value.Length; i++)
            {
                tabControl.TabPages.Add(PropNames[i]);
                tabControl.TabPages[tabControl.TabPages.Count - 1].AutoScroll = false;

                NumericUpDown num = new NumericUpDown();

                num.DecimalPlaces = 0;
                num.Minimum = uint.MinValue;
                num.Maximum = uint.MaxValue;

                num.Value = (decimal)Value[i];
                num.Parent = tabControl.TabPages[tabControl.TabPages.Count - 1];
                num.Dock = DockStyle.Fill;
                num.ValueChanged += ValueChange;
                num.MouseCaptureChanged += FocusUI;

                nums.Add(num);

            }

            TableLayoutPanel table = (TableLayoutPanel)parent;
            table.RowStyles[offset] = new RowStyle(SizeType.Absolute, 54);

            table.SetColumn(tabControl, 1);
            table.SetRow(tabControl, offset);

        }

        public override void ValueChange(object sender, EventArgs e)
        {
            base.ValueChange(sender, e);

            NumericUpDown box = (NumericUpDown)sender;

            if (nums.Contains(box))
            {
                int pos = nums.IndexOf(box);
                Value[pos] = (uint)box.Value;
            }
            else
            {
                Console.WriteLine("Error: Numeric up down not linked to a value!");
            }

        }

        public override void ResetToDefault()
        {
            for (int i = 0; i < Value.Length; i++)
            {
                Value[i] = DefaultValue[i];
            }
            HasChanged = false;
        }

        public override void Serialize(ref string line)
        {
            base.Serialize(ref line);

            for (int i = 0; i < Value.Length; i++)
            {
                line += Value[i];
                line += ";";
            }
        }

        public override void DeSerialize(string input)
        {
            string[] vals = input.Split(';');

            if (vals.Length != Value.Length + 1 && vals.Length != Value.Length)
            {
                Console.WriteLine("Error: Input UInt array length mismatch!");
                return;
            }

            for (int i = 0; i < vals.Length - 1; i++)
            {
                uint val;
                if (uint.TryParse(vals[i], out val))
                {
                    Value[i] = val;
                }
            }


        }

    }
}