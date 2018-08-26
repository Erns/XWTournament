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
using Plugin.LocalNotifications;

namespace XWTournament.Pages.Tournaments
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class Tournaments_RoundInfo : ContentPage
	{

        private int intRoundId = 0;
        private int intRoundNumber = 0;
        static double dblScrollY = 0;
        TournamentMainRoundInfoTimer_ViewModel timerRoundBtn_VM;

        public Tournaments_RoundInfo (Tournaments_AllInfo allInfoPage, string strTitle, int intRoundId, int intRoundCount)
		{
			InitializeComponent ();
            Title = strTitle;
            this.intRoundId = intRoundId;

            //Tie the loading Overlay to the main page since this is what will be flagged as "IsBusy" when generating new rounds etc.
            loadingOverlay.BindingContext = allInfoPage;

            timerRoundBtn_VM = new TournamentMainRoundInfoTimer_ViewModel();
            timerRoundBtn.BindingContext = timerRoundBtn_VM;

            using (SQLite.SQLiteConnection conn = new SQLite.SQLiteConnection(App.DB_PATH))
            {
                TournamentMainRound round = new TournamentMainRound();
                round = conn.GetWithChildren<TournamentMainRound>(intRoundId);
                intRoundNumber = round.Number;

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

                    //If the round time started previously, keep it going
                    if (round.RoundTimeEnd > DateTime.Now)
                    {
                        TimeSpan time = round.RoundTimeEnd - DateTime.Now;
                        App.MasterMainPage.RoundTimer(time, time.Seconds, ref timerRoundBtn_VM, true);
                    }

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

        #region "Buttons"               
        private void timerRoundBtn_Clicked(object sender, EventArgs e)
        {
            timerPopup.IsVisible = true;
        }

        private void saveTimerRoundBtn_Clicked(object sender, EventArgs e)
        {
            this.IsBusy = true;

            int intTime = Convert.ToInt16(timerOptionsPicker.Items[timerOptionsPicker.SelectedIndex]);

            //Set the end time for the round
            DateTime roundTimeMid = DateTime.Now.AddSeconds(intTime / 2);
            DateTime roundTimeEnd = DateTime.Now.AddSeconds(intTime);

            //Set phone notification
            CrossLocalNotifications.Current.Cancel(101);
            CrossLocalNotifications.Current.Show("Round " + intRoundNumber.ToString() + " is halfway over.", "Almost there!", 101, roundTimeMid);

            CrossLocalNotifications.Current.Cancel(102);
            CrossLocalNotifications.Current.Show("Round " + intRoundNumber.ToString() + " is over.", "Finish your round.", 102, roundTimeEnd);

            //Get the TimeSpan and set the round timer right away
            TimeSpan time = roundTimeEnd - DateTime.Now;
            App.MasterMainPage.RoundTimer(time, intTime, ref timerRoundBtn_VM);

            //Save what the end time is at so that the timer is accurate if returning later
            using (SQLite.SQLiteConnection conn = new SQLite.SQLiteConnection(App.DB_PATH))
            {
                TournamentMainRound round = new TournamentMainRound();
                round = conn.GetWithChildren<TournamentMainRound>(intRoundId);
                round.RoundTimeEnd = roundTimeEnd;
                conn.Update(round);
            }

            timerPopup.IsVisible = false;
            this.IsBusy = false;
        }
        
        //Hide timer popup when hitting the back button
        protected override bool OnBackButtonPressed()
        {
            if (timerPopup.IsVisible)
            {
                timerPopup.IsVisible = false;
                return true;    //Prevent back button from continuing
            }
            else
            {
                return base.OnBackButtonPressed();
            }
        }
        #endregion
    }
}