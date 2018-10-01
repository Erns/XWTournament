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
        private DateTime dteRoundTimeEnd = DateTime.Now;

        private const int cintMidNotifyId = 101;
        private const int cintEndNotifyId = 102;

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
                        dteRoundTimeEnd = round.RoundTimeEnd;
                        TimeSpan time = round.RoundTimeEnd - DateTime.Now;
                        App.MasterMainPage.RoundTimer(time, Convert.ToInt32(time.TotalSeconds), ref timerRoundBtn_VM);
                    }

                }
            }
        }

        #region "ListView Utilities"

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

        private bool isRowEven = false;
        private void ViewCell_Appearing(object sender, EventArgs e)
        {
            if (this.isRowEven)
            {
                var viewCell = (ViewCell)sender;
                if (viewCell.View != null)
                {
                    viewCell.View.BackgroundColor = Color.LightGray;
                }
            }

            this.isRowEven = !this.isRowEven;
        }

        #endregion

        #region "Buttons"               
        async private void timerRoundBtn_Clicked(object sender, EventArgs e)
        {
            if (dteRoundTimeEnd > DateTime.Now)
            {
                var answer = await DisplayAlert("Round In Progress", "Would you like to cancel the current timer?", "Yes", "No");
                if (answer)
                {
                    //Update round's end-time
                    using (SQLite.SQLiteConnection conn = new SQLite.SQLiteConnection(App.DB_PATH))
                    {
                        TournamentMainRound round = new TournamentMainRound();
                        round = conn.GetWithChildren<TournamentMainRound>(intRoundId);
                        round.RoundTimeEnd = DateTime.Now;
                        conn.Update(round);
                        dteRoundTimeEnd = round.RoundTimeEnd;
                    }

                    //Cancel any pending notifications
                    CrossLocalNotifications.Current.Cancel(cintMidNotifyId);
                    CrossLocalNotifications.Current.Cancel(cintEndNotifyId);

                    App.MasterMainPage.CancelRoundTimer();
                }
                return;
            }
            else
            {
                timerPopup.IsVisible = true;
            }
        }

        private void saveTimerRoundBtn_Clicked(object sender, EventArgs e)
        {
            this.IsBusy = true;

            int intTime = Convert.ToInt16(timerOptionsPicker.Items[timerOptionsPicker.SelectedIndex]);
            intTime *= 60; //Converting to seconds

            //Set the end time for the round
            DateTime roundTimeMid = DateTime.Now.AddSeconds(intTime / 2);
            DateTime roundTimeEnd = DateTime.Now.AddSeconds(intTime);

            //Set phone notification
            CrossLocalNotifications.Current.Cancel(cintMidNotifyId);
            CrossLocalNotifications.Current.Cancel(cintEndNotifyId);

            CrossLocalNotifications.Current.Show("Round " + intRoundNumber.ToString() + " is halfway over.", "Almost there!", cintMidNotifyId, roundTimeMid);
            CrossLocalNotifications.Current.Show("Round " + intRoundNumber.ToString() + " is over.", "Finish your round.", cintEndNotifyId, roundTimeEnd);

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

                dteRoundTimeEnd = roundTimeEnd;
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