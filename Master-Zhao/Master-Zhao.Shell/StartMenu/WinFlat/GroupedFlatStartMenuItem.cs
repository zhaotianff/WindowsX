﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Master_Zhao.Shell.StartMenu.WinFlat
{
    public class GroupedFlatStartMenuItem
    {
        public string GroupName { get; set; }

        public List<WinFlatStartMenuItem> MenuItems { get; set; } = new List<WinFlatStartMenuItem>();
    }
}
