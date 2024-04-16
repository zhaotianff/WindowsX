using System;
using System.Collections.Generic;
using System.Text;

namespace WindowsX.Config.Model.StartMenu
{
    public class FlatStartMenuGroupedItem
    {
        public string GroupName { get; set; }

        public List<FlatStartMenuItem> MenuItems { get; set; }
    }
}
