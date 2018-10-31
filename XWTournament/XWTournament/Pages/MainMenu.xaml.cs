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
using System.Collections.ObjectModel;
using RestSharp;
using Newtonsoft.Json;
using SQLiteNetExtensions.Extensions;

namespace XWTournament.Pages
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainMenu : MasterDetailPage
    {

        public List<MainMenuGroup> MainMenuGroups { get; set; }

        private static int round_time = 0;
        private static System.Timers.Timer ROUND_TIMER = null;

        public MainMenu()
        {
            MainMenuGroups = new List<MainMenuGroup>();

            // Set the binding context to this code behind.
            BindingContext = this;

            var allListItemGroups = new List<List<MainMenuItem>>();

            MainMenuGroup mainMenuGroup;

            //Offline grouping
            mainMenuGroup = new MainMenuGroup()
            {
                new MainMenuItem() { Title = "Players", Icon = "\uf0c0" },
                new MainMenuItem() { Title = "Tournaments", Icon = "\uf02d" }
            };
            mainMenuGroup.GroupName = "Local";

            MainMenuGroups.Add(mainMenuGroup);

            //Online grouping
            mainMenuGroup = new MainMenuGroup()
            {
                new MainMenuItem() { Title = "Account", Icon = "\uf0ac" },
                new MainMenuItem() { Title = "Import To Local", Icon = "\uf019" },
            };
            mainMenuGroup.GroupName = "Online";

            MainMenuGroups.Add(mainMenuGroup);


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
                    case "Account":
                        Detail = new NavigationPage(new OnlineAccount_Main());
                        break;

                    case "Import To Local":
                        if (App.IsUserLoggedIn)
                        {
                            var confirmed = ImportPromptAsync();
                        }
                        else
                        {
                            DisplayAlert("Action Needed", "Please log into your user account first.", "OK");
                        }
                        break;

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

        private async Task<bool> ImportPromptAsync()
        {
            var confirmed = await DisplayAlert("Proceed with Import?", "This will import all players and tournaments associated with your online account onto this device.", "Import", "Cancel");

            if (confirmed)
            {
                string strImportMsg = Online_Import.ImportAll();

                await DisplayAlert("Import", strImportMsg, "OK");

                Detail = new NavigationPage(new Players_Main());
            }
            return confirmed;
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