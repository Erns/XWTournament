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
                objTournMain = new TournamentMain();
                objTournMain = conn.GetWithChildren<TournamentMain>(intTournamentId, true);
                Title += objTournMain.Name;

                Utilities.CalculatePlayerScores(ref objTournMain);

                tournamentStandingsListView.ItemsSource = objTournMain.Players.OrderBy(obj => obj.Rank).ToList();

            }
        }
	}
}