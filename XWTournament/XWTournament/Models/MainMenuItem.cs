using System;
using System.Collections.Generic;
using System.Text;

namespace XWTournament.Models
{
    public class MainMenuGroup
    {
        public string GroupName { get; set; }
        public List<MainMenuItem> Items { get; set; } = new List<MainMenuItem>();
    }

    public class MainMenuItem
    {
        public string Title { get; set; }
        public Type TargetType { get; set; }
        public string Icon { get; set; }
    }
}
