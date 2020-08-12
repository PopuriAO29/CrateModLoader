﻿using System.Windows.Forms;

namespace CrateModLoader.ModProperties
{
    public class ModPropBool : ModProperty<bool>
    {

        public ModPropBool(bool b) : base(b)
        {

        }
        public ModPropBool(bool b, string name, string desc) : base(b, name, desc)
        {

        }

        public override void GenerateUI(TabPage page, ref int offset)
        {
            //base.GenerateUI(page, ref offset);
            
            CheckBox checkBox = new CheckBox();
            checkBox.Text = Name;
            checkBox.Checked = (bool)Value;
            checkBox.Parent = page;
            checkBox.Location = new System.Drawing.Point(10, offset);
            checkBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            checkBox.Size = new System.Drawing.Size(page.Width - 30, checkBox.Size.Height);
            checkBox.CheckedChanged += ValueChange;
            

        }

    }
}