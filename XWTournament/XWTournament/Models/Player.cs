using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace XWTournament.Models
{
    public class Player
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public bool Active { get; set; }
        public Nullable<DateTime> DateDeleted { get; set; } = null;
    }
}
