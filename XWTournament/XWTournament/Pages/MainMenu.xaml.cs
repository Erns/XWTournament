using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XWTournament.Classes;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XWTournament.Models;
using XWTournament.ViewModel;
using System.Threading;

namespace XWTournament.Pages
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainMenu : MasterDetailPage
    {
        public List<MainMenuItem> MainMenuItems { get; set; }

        private static int round_time = 0;
        private static System.Timers.Timer ROUND_TIMER = null;

        public MainMenu()
        {

            // Set the binding context to this code behind.
            BindingContext = this;

            // Build the Menu
            MainMenuItems = new List<MainMenuItem>()
            {
                new MainMenuItem() { Title = "Players", Icon = "\uf0c0", TargetType = typeof(Players_Main) },
                new MainMenuItem() { Title = "Tournaments", Icon = "\uf02d", TargetType = typeof(Tournaments_Main) }
            };

            // Set the default page, this is the "home" page.
            Detail = new NavigationPage(new Players_Main());

            InitializeComponent();

            //Create all the needed tables out the gate
            Utilities.InitializeTournamentMain(new SQLite.SQLiteConnection(App.DB_PATH));
        }

        // When a MenuItem is selected.
        public void MainMenuItem_Selected(object sender, SelectedItemChangedEventArgs e)
        {
            var item = e.SelectedItem as MainMenuItem;
            if (item != null)
            {
                switch (item.Title.ToString())
                {

                    case "Players":
                        Detail = new NavigationPage(new Players_Main());
                        break;

                    case "Tournaments":
                        Detail = new NavigationPage(new Tournaments_Main());
                        break;
                }

                MenuListView.SelectedItem = null;
                IsPresented = false;
            }
        }

        #region "Global timer shit"
        TournamentMainRoundInfoTimer_ViewModel tmpVM = null;
        public void RoundTimer(TimeSpan time, int intTime, ref TournamentMainRoundInfoTimer_ViewModel timerRoundBtn_VM)
        {
            //Set timer to the round's end time, cancelling any existing timer and starting with the new time in mind.
            round_time = intTime;
            if (timerRoundBtn_VM != null)
            {
                tmpVM = timerRoundBtn_VM;
                tmpVM.TimerValue = round_time.ToString();
            }

            if (ROUND_TIMER != null)
            {
                ROUND_TIMER.Stop();
                ROUND_TIMER.Enabled = false;
                ROUND_TIMER = null;
            }

            ROUND_TIMER = new System.Timers.Timer();

            //Trigger event every second
            ROUND_TIMER.Interval = 1000;
            ROUND_TIMER.Elapsed += roundTimer_Tick;
            ROUND_TIMER.Enabled = true;
            ROUND_TIMER.Start();
        }

        public void CancelRoundTimer()
        {
            round_time = 0;
        }


        private void roundTimer_Tick(object sender, System.Timers.ElapsedEventArgs e)
        {
            round_time--;

            if (tmpVM != null)
                tmpVM.TimerValue = round_time.ToString();

            if (round_time <= 0)
            {
                ROUND_TIMER.Stop();
                ROUND_TIMER.Enabled = false;
                ROUND_TIMER = null;

                if (tmpVM != null)
                {
                    tmpVM.TimerValue = "0";
                    tmpVM = null;
                }

                RoundOver();
            }
        }

        private void RoundOver()
        {
            DisplayAlert("Oy!", "Round over!", "yup");
        }
        #endregion

    }
}