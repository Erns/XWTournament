using SQLiteNetExtensions.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XWTournament.Classes;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XWTournament.Models;

namespace XWTournament.Pages.Tournaments
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class Tournaments_Standings : ContentPage
	{

        private TournamentMain objTournMain = new TournamentMain();


        public Tournaments_Standings (int intTournamentId)
		{
			InitializeComponent ();
            Title = "Current Standings - ";

            using (SQLite.SQLiteConnection conn = new SQLite.SQLiteConnection(App.DB_PATH))
            {
                SetStandings(conn.GetWithChildren<TournamentMain>(intTournamentId, true));
            }
        }

        public Tournaments_Standings(TournamentMain tourn)
        {
            InitializeComponent();
            Title = "Current Standings - ";
            SetStandings(tourn);
        }

        private void SetStandings(TournamentMain tourn)
        {
            objTournMain = new TournamentMain();
            objTournMain = tourn;
            Title += objTournMain.Name;

            Utilities.CalculatePlayerScores(ref objTournMain);

            List<TournamentMainPlayer> lstPlayerStandings = new List<TournamentMainPlayer>();
            foreach (TournamentMainPlayer player in objTournMain.Players.OrderBy(obj => obj.Rank).ToList())
            {
                //Separating out as to not tempt fate and inadvertently change any data unintentionally
                TournamentMainPlayer tmpPlayer = new TournamentMainPlayer();
                tmpPlayer.Rank = player.Rank;
                tmpPlayer.PlayerName = player.PlayerName;
                tmpPlayer.Score = player.Score;
                tmpPlayer.MOV = player.MOV;
                tmpPlayer.SOS = player.SOS;

                if (!player.Active) tmpPlayer.PlayerName += " (D)";

                lstPlayerStandings.Add(tmpPlayer);
            }

            tournamentStandingsListView.ItemsSource = lstPlayerStandings;
        }
    }
}