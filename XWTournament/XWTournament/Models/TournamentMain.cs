using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace XWTournament.Models
{
    public class TournamentMain
    {

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public int MaxPoints { get; set; }
        public int RoundTimeLength { get; set; }
        public Nullable<DateTime> DateDeleted { get; set; } = null;

        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public List<TournamentMainPlayer> Players { get; set; } = new List<TournamentMainPlayer>();

        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public List<TournamentMainRound> Rounds { get; set; } = new List<TournamentMainRound>();

        public string ActivePlayersList()
        {
            List<string> lstIDs = new List<string>();
            foreach (TournamentMainPlayer item in Players)
            {
                if (item.Active) lstIDs.Add(item.PlayerId.ToString());
            }

            return String.Join(",", lstIDs.ToArray());
        }
    }

    public class TournamentMainPlayer
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [ForeignKey(typeof(TournamentMain))]
        public int TournmentId { get; set; }

        [ForeignKey(typeof(Player))]
        public int PlayerId { get; set; }

        public string PlayerName { get; set; }
        public bool Active { get; set; } = true;
        public bool Bye { get; set; } = false;
        public int ByeCount { get; set; }

        public int RoundsPlayed { get; set; }
        public int Rank { get; set; }
        public int Score { get; set; }
        public int MOV { get; set; }
        public decimal SOS { get; set; }

    }

    public class TournamentMainRound
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [ForeignKey(typeof(TournamentMain))]
        public int TournmentId { get; set; }

        public int Number { get; set; }

        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public List<TournamentMainRoundTable> Tables { get; set; } = new List<TournamentMainRoundTable>();

    }


    public class TournamentMainRoundTable
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        //Table general info
        [ForeignKey(typeof(TournamentMainRound))]
        public int RoundId { get; set; }
        public int Number { get; set; }
        public string TableName { get; set; }
        public bool ScoreTied { get; set; } = false;
        public bool Bye { get; set; }

        //Player 1 Info
        [ForeignKey(typeof(Player))]
        public int Player1Id { get; set; } = 0;
        public string Player1Name { get; set; } = "N/A";
        public int Player1Score { get; set; }
        public bool Player1Winner { get; set; } = false;

        //Player 2 info
        [ForeignKey(typeof(Player))]
        public int Player2Id { get; set; } = 0;
        public string Player2Name { get; set; } = "N/A";
        public int Player2Score { get; set; }
        public bool Player2Winner { get; set; } = false;

    }

}
