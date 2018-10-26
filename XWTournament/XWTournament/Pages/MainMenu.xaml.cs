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
        RestClient client = Utilities.InitializeRestClient();

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
            mainMenuGroup.GroupName = "Information";

            MainMenuGroups.Add(mainMenuGroup);

            //Online grouping
            mainMenuGroup = new MainMenuGroup()
            {
                new MainMenuItem() { Title = "Account", Icon = "\uf0ac" },
                new MainMenuItem() { Title = "Import All", Icon = "\uf019" },
                new MainMenuItem() { Title = "Export All", Icon = "\uf093" }

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

                    case "Import All":
                        if (App.IsUserLoggedIn)
                        {
                            var confirmed = ImportPromptAsync();
                        }
                        else
                        {
                            DisplayAlert("Action Needed", "Please log into your user account first.", "OK");
                        }
                        break;

                    case "Export All":
                        if (App.IsUserLoggedIn)
                        {

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

        private async Task<bool> ImportPromptAsync()
        {
            var confirmed = await DisplayAlert("Proceed with Import?", "This will import all players and tournaments associated with your online account onto this device.", "Import", "Cancel");

            if (confirmed)
            {
                ImportAll();
            }
            return confirmed;
        }

        private void ImportAll()
        {
            if (App.IsUserLoggedIn)
            {
                //Get the current players saved locally
                List<Player> lstCurrentPlayers = new List<Player>();
                List<TournamentMain> lstCurrentTournaments = new List<TournamentMain>();

                using (SQLite.SQLiteConnection conn = new SQLite.SQLiteConnection(App.DB_PATH))
                {
                    Utilities.InitializeTournamentMain(conn);

                    lstCurrentPlayers = conn.Query<Player>("SELECT * FROM Player WHERE Active = ? AND DateDeleted IS NULL ORDER BY Name", true);
                    lstCurrentTournaments = conn.Query<TournamentMain>("SELECT * FROM TournamentMain WHERE DateDeleted IS NULL ORDER BY StartDate");
                }

                ImportPlayers(lstCurrentPlayers);
                ImportTournaments(lstCurrentTournaments);

                using (SQLite.SQLiteConnection conn = new SQLite.SQLiteConnection(App.DB_PATH))
                {
                    lstCurrentPlayers = new List<Player>();
                    lstCurrentPlayers = conn.Query<Player>("SELECT * FROM Player WHERE Active = ? AND DateDeleted IS NULL ORDER BY Name", true);

                    Dictionary<int, int> dctAPIPlayerId = new Dictionary<int, int>();
                    foreach (Player player in lstCurrentPlayers)
                    {
                        if (!dctAPIPlayerId.ContainsKey(player.API_Id))
                        {
                            dctAPIPlayerId.Add(player.API_Id, player.Id);
                        }
                    }

                    lstCurrentTournaments = new List<TournamentMain>();
                    lstCurrentTournaments = conn.Query<TournamentMain>("SELECT * FROM TournamentMain WHERE DateDeleted IS NULL ORDER BY StartDate");

                    List<TournamentMainRound> lstCurrentRounds = new List<TournamentMainRound>();

                    TournamentMain objTournMain;
                    foreach (TournamentMain localTournament in lstCurrentTournaments)
                    {
                        objTournMain = new TournamentMain();
                        objTournMain = conn.GetWithChildren<TournamentMain>(localTournament.Id, true);

                        foreach (TournamentMainRound localRound in objTournMain.Rounds)
                        {
                            lstCurrentRounds.Add(localRound);
                        }

                        if (objTournMain.API_Id > 0)
                        {
                            var request = new RestRequest("Tournaments/{userid}/{id}", Method.GET);
                            request.AddUrlSegment("userid", App.CurrentUser.Id);
                            request.AddUrlSegment("id", objTournMain.API_Id);

                            // execute the request
                            IRestResponse response = client.Execute(request);
                            var content = response.Content;

                            List<TournamentMain> result = JsonConvert.DeserializeObject<List<TournamentMain>>(JsonConvert.DeserializeObject(content).ToString());
                            foreach (TournamentMain ApiTournament in result)
                            {
                                foreach(TournamentMainRound ApiRound in ApiTournament.Rounds)
                                {
                                    TournamentMainRound updateRound = new TournamentMainRound()
                                    {
                                        API_Id = ApiRound.Id,
                                        Number = ApiRound.Number,
                                        RoundTimeEnd = ApiRound.RoundTimeEnd,
                                        Swiss = ApiRound.Swiss,
                                        TournmentId = objTournMain.Id
                                    };

                                    foreach (TournamentMainRound localRound in objTournMain.Rounds)
                                    {
                                        if (localRound.API_Id == updateRound.API_Id)
                                        {
                                            updateRound.Id = localRound.Id;
                                            break;
                                        }
                                    }
                                    if (updateRound.Id == 0)
                                    {
                                        conn.Insert(updateRound);
                                        TournamentMainRound tmp = conn.Query<TournamentMainRound>("SELECT * FROM TournamentMainRound WHERE TournamentId = ? ORDER BY Id DESC", updateRound.TournmentId)[0];
                                        updateRound.Id = tmp.Id;
                                    }
                                    else
                                    {
                                        conn.Update(updateRound);
                                    }
                                    
                                    
                                    foreach(TournamentMainRoundTable ApiTable in ApiRound.Tables)
                                    {
                                        TournamentMainRoundTable updateTable = new TournamentMainRoundTable()
                                        {
                                            API_Id = ApiTable.Id,
                                            Bye = ApiTable.Bye,
                                            Number = ApiTable.Number,
                                            Player1Id = (dctAPIPlayerId.ContainsKey(ApiTable.Player1Id) ? dctAPIPlayerId[ApiTable.Player1Id] : 0),
                                            Player1Name = ApiTable.Player1Name,
                                            Player1Score = ApiTable.Player1Score,
                                            Player1Winner = ApiTable.Player1Winner,
                                            Player2Id = (dctAPIPlayerId.ContainsKey(ApiTable.Player2Id) ? dctAPIPlayerId[ApiTable.Player2Id] : 0),
                                            Player2Name = ApiTable.Player2Name,
                                            Player2Score = ApiTable.Player2Score,
                                            Player2Winner = ApiTable.Player2Winner,
                                            ScoreTied = ApiTable.ScoreTied,
                                            TableName = ApiTable.TableName
                                        };
                                    }
                                }
                                break;
                            }

                            ////Didn't actually get a result (such as trying to access a tournament not "owned" by user).  Kick back to Main page
                            //if (result.Count == 0)
                            //    return RedirectToAction("TournamentMain", "Tournament");
                        }
                    }

                }

                
            }

            Detail = new NavigationPage(new Players_Main());
        }

        private void ImportPlayers(List<Player> lstCurrentPlayers)
        {
            //Get Players
            RestRequest request = new RestRequest("Players/{userid}", Method.GET);
            request.AddUrlSegment("userid", App.CurrentUser.Id);

            // execute the request
            IRestResponse response = client.Execute(request);
            string content = response.Content;

            List<Player> lstApiPlayers = JsonConvert.DeserializeObject<List<Player>>(JsonConvert.DeserializeObject(content).ToString());

            //Compare players from API with what's saved locally, insert/updated as needed
            using (SQLite.SQLiteConnection conn = new SQLite.SQLiteConnection(App.DB_PATH))
            {
                foreach (Player apiPlayer in lstApiPlayers)
                {
                    if (apiPlayer.Active && apiPlayer.DateDeleted == null)
                    {
                    
                        Player updatePlayer = new Player()
                        {
                            Name = apiPlayer.Name,
                            Email = apiPlayer.Email,
                            Group = apiPlayer.Group,
                            Active = true,
                            API_Id = apiPlayer.Id
                        };

                        //Attempt to associate a player from the API with one saved locally
                        foreach (Player localPlayer in lstCurrentPlayers)
                        {
                            if (localPlayer.API_Id == apiPlayer.Id || (apiPlayer.Name.ToUpper() == localPlayer.Name.ToUpper() && apiPlayer.Email.ToUpper() == localPlayer.Email.ToUpper()))
                            {
                                updatePlayer.Id = localPlayer.Id;
                                break;
                            }
                        }

                        if (updatePlayer.Id == 0)
                        {
                            conn.Insert(updatePlayer);
                        }
                        else
                        {
                            conn.Update(updatePlayer);
                        }
                    }
                }
            }
        }

        private void ImportTournaments(List<TournamentMain> lstCurrentTournaments)
        {
            //Get Tournaments
            RestRequest request = new RestRequest("Tournaments/{userid}", Method.GET);
            request.AddUrlSegment("userid", App.CurrentUser.Id);

            // execute the request
            IRestResponse response = client.Execute(request);
            string content = response.Content;

            List<TournamentMain> lstApiTournaments = JsonConvert.DeserializeObject<List<TournamentMain>>(JsonConvert.DeserializeObject(content).ToString());
            using (SQLite.SQLiteConnection conn = new SQLite.SQLiteConnection(App.DB_PATH))
            {
                foreach (TournamentMain apiTournament in lstApiTournaments)
                {
                    TournamentMain updateTournament = new TournamentMain()
                    {
                        API_Id = apiTournament.Id,
                        Name = apiTournament.Name,
                        MaxPoints = apiTournament.MaxPoints,
                        Players = apiTournament.Players,
                        Rounds = apiTournament.Rounds,
                        RoundTimeLength = apiTournament.RoundTimeLength,
                        StartDate = apiTournament.StartDate
                    };

                    foreach (TournamentMain localTournament in lstCurrentTournaments)
                    {
                        if (localTournament.Name == apiTournament.Name && localTournament.StartDate == apiTournament.StartDate)
                        {
                            updateTournament.Id = localTournament.Id;
                            break;
                        }
                    }

                    if (updateTournament.Id == 0)
                    {
                        conn.Insert(updateTournament);
                    }
                    else
                    {
                        conn.Update(updateTournament);
                    }
                }
            }
        }
    }
}