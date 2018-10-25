using System;
using System.Collections.Generic;
using System.Text;

namespace XWTournament.Models
{
    public class MainMenuGroup: List<MainMenuItem>
    {
        public string GroupName { get; set; }
        public List<MainMenuItem> Items => this;
    }

    public class MainMenuItem
    {
        public string Title { get; set; }
        public Type TargetType { get; set; }
        public string Icon { get; set; }
    }
}
