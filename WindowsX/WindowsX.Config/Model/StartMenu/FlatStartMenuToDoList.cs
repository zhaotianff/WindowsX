using System;
using System.Collections.Generic;
using System.Text;

namespace WindowsX.Config.Model.StartMenu
{
    public class TodoDateItem
    {
        public DateTime Date { get; set; }

        public List<TodoItem> TodoList { get; set; }
    }

    public class TodoItem
    {
        public string Description { get; set; }
        public bool FinishStatus { get; set; }
    }
}
