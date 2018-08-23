using SQLiteNetExtensions.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XWTournament.Classes;
using XWTournament.ViewModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XWTournament.Models;

namespace XWTournament.Pages.Tournaments
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class Tournaments_RoundInfo : ContentPage
	{

        private int intRoundId = 0;
        static double dblScrollY = 0;

        public Tournaments_RoundInfo (Tournaments_AllInfo allInfoPage, string strTitle, int intRoundId, int intRoundCount)
		{
			InitializeComponent ();
            Title = strTitle;
            this.intRoundId = intRoundId;

            //Tie the loading Overlay to the main page since this is what will be flagged as "IsBusy" when generating new rounds etc.
            loadingOverlay.BindingContext = allInfoPage;


            using (SQLite.SQLiteConnection conn = new SQLite.SQLiteConnection(App.DB_PATH))
            {
                TournamentMainRound round = new TournamentMainRound();
                round = conn.GetWithChildren<TournamentMainRound>(intRoundId);

                bool blnEnableRows = (round.Number < intRoundCount ? false : true);

                //Set using the ViewModel version.  This allows being able to manipulate back and forth across the class properties, while displaying as intended on the GUI
                //while also ensuring none of the goings ons of the properties touching each other don't occur without this specific view model (such as the SQL table updates)
                ObservableCollection<TournamentMainRoundTable_ViewModel> lstTables = new ObservableCollection<TournamentMainRoundTable_ViewModel>();
                foreach (TournamentMainRoundTable table in round.Tables)
                {
                    lstTables.Add(new TournamentMainRoundTable_ViewModel(table, blnEnableRows));
                }
                tournamentTableListView.ItemsSource = lstTables;

                if (!blnEnableRows)
                {
                    timerRoundBtn.IsVisible = false;
                    tournamentTableListView.ItemTapped -= tournamentTableListView_ItemTapped;
                } 
                else 
                {
                    timerRoundBtn.IsVisible = true;
                }
            }
        }

        //Show/Hide button when scrolling up/down
        private void tournamentTableListView_Scrolled(object sender, ScrolledEventArgs e)
        {
            if (e.ScrollY > dblScrollY)
            {
                timerRoundBtn.TranslateTo(0, 200, 200);             
            }
            else if (e.ScrollY == 0 || e.ScrollY < dblScrollY)
            {
                timerRoundBtn.TranslateTo(0, 0, 200);
            }

            dblScrollY = e.ScrollY;
        }

        private void tournamentTableListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            TournamentMainRoundTable_ViewModel tmp = (TournamentMainRoundTable_ViewModel)e.Item;

            Navigation.PushAsync(new Tournaments_RoundInfoTableEdit(intRoundId, tmp.TournamentMainRoundTable.Id));

        }
    }
}