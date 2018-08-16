using SQLiteNetExtensions.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XWTournament.ViewModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XWTournament.Models;
using XWTournament.Classes;

namespace XWTournament.Pages.Tournaments
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Tournaments_AllInfo : TabbedPage
    {
        private int intTournID;

        private TournamentMain objTournMain = new TournamentMain();
        private List<int> lstPlayerIds = new List<int>();

        private ObservableCollection<PlayerToTournamentMainPlayer_ViewModel> lstViewPlayers = new ObservableCollection<PlayerToTournamentMainPlayer_ViewModel>();

        //New
        public Tournaments_AllInfo (string strTitle, int tournamentId)
        {
            InitializeComponent();
            Title = strTitle;
            intTournID = tournamentId;
        }

        //Opening
        protected override void OnAppearing()
        {
            base.OnAppearing();

            using (SQLite.SQLiteConnection conn = new SQLite.SQLiteConnection(App.DB_PATH))
            {

                objTournMain = new TournamentMain();
                objTournMain = conn.GetWithChildren<TournamentMain>(intTournID, true);
                Title = objTournMain.Name;

                UpdatePlayerList(conn);

                //Get full list of players
                List<Player> lstPlayers = conn.Query<Player>("SELECT * FROM Player WHERE (Active = 1 AND DateDeleted IS NULL) OR Id IN (" + objTournMain.ActivePlayersList() + ")");

                //Get list of currently active players in tournament
                string[] arrActivePlayers = objTournMain.ActivePlayersList().Split(',');


                //Set using the ViewModel version.  This allows being able to manipulate back and forth across the class properties, while displaying as intended on the GUI
                //while also ensuring none of the goings ons of the properties touching each other don't occur without this specific view model (such as the SQL table updates)
                int intRow = 0;
                lstViewPlayers = new ObservableCollection<PlayerToTournamentMainPlayer_ViewModel>();
                TournamentMainPlayer tmpTournamentMainPlayer = null;
                foreach (Player player in lstPlayers)
                {
                    player.Active = false;
                    if (arrActivePlayers.Contains<string>(player.Id.ToString())) player.Active = true;
                    intRow++;

                    //Set the tournament player equivalent
                    tmpTournamentMainPlayer = null;
                    foreach (TournamentMainPlayer tournamentPlayer in objTournMain.Players)
                    {
                        if (tournamentPlayer.PlayerId == player.Id)
                        {
                            tmpTournamentMainPlayer = tournamentPlayer;
                            break;
                        }
                    }

                    lstViewPlayers.Add(new PlayerToTournamentMainPlayer_ViewModel(player, intTournID, intRow, tmpTournamentMainPlayer));
                }
                playersListView.ItemsSource = lstViewPlayers;
            }


            //Remove all the "Round" tabs and then repopulate them
            for (int index = this.Children.Count - 1; index > 0; index--)
            {
                if (index > 0)
                    this.Children.RemoveAt(index);
            }

            //Add each round as tabs
            foreach (TournamentMainRound round in objTournMain.Rounds)
            {
                Children.Add(new Tournaments_RoundInfo("Rd " + round.Number, round.Id, objTournMain.Rounds.Count));
            }

            //Remove option to delete round if there are no rounds
            if (objTournMain.Rounds.Count == 0)
            {
                ToolbarItems.Remove(deleteRoundBtn);
            }

        }


        #region 'Page-specific utilities'
        private void UpdatePlayerList(SQLite.SQLiteConnection conn)
        {
            List<Player> lstActivePlayers = conn.Query<Player>("SELECT * FROM Player WHERE Id IN (" + objTournMain.ActivePlayersList() + ")");
            activePlayersListView.ItemsSource = lstActivePlayers;
        }

        private void setRoundTableNames(ref TournamentMainRoundTable roundTable)
        {
            using (SQLite.SQLiteConnection conn = new SQLite.SQLiteConnection(App.DB_PATH))
            {

                Player player;

                string strPlayer1Name = "N/A";
                string strPlayer2Name = "N/A";

                if (roundTable.Player1Id > 0)
                {
                    player = conn.Get<Player>(roundTable.Player1Id);
                    strPlayer1Name = player.Name;
                }

                if (roundTable.Player2Id > 0)
                {
                    player = conn.Get<Player>(roundTable.Player2Id);
                    strPlayer2Name = player.Name;
                }

                roundTable.Player1Name = strPlayer1Name;
                roundTable.Player2Name = strPlayer2Name;
                roundTable.TableName = string.Format("{0} vs {1}", strPlayer1Name, strPlayer2Name);
            }
        }
        #endregion

        #region 'Add Players Popup'

        //Open add player popup
        void addPlayers()
        {
            playersListView_SearchBar.Text = "";
            addPlayersPopup.IsVisible = true;
        }

        //Close add player popup
        void closePlayers()
        {
            addPlayersPopup.IsVisible = false;
            OnAppearing();  //Refresh page so that the popup has all the correct players selected and set
        }


        //Hide add player popup when hitting the back button
        protected override bool OnBackButtonPressed()
        {
            if (addPlayersPopup.IsVisible)
            {
                closePlayers();
                return true;    //Prevent back button from continuing
            }
            else
            {
                return base.OnBackButtonPressed();
            }
        }

        //Players search bar
        private void SearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            playersListView.BeginRefresh();

            if (string.IsNullOrWhiteSpace(e.NewTextValue))
                playersListView.ItemsSource = lstViewPlayers;
            else
                playersListView.ItemsSource = lstViewPlayers.Where(i => i.TournamentMainPlayer.PlayerName.Contains(e.NewTextValue));

            playersListView.EndRefresh();

        }
        #endregion

        #region 'Buttons'

        //Secondary toolbar items
        private void editTournmentBtn_Activated(object sender, EventArgs e)
        {
            Navigation.PushAsync(new Tournaments_AddEdit(intTournID));
        }


        //Start next round
        private void startRoundBtn_ToolbarItem_Activated(object sender, EventArgs e)
        {
            //Make sure we actually have players
            if (objTournMain.Players.Count == 0)
            {
                DisplayAlert("Warning!", "You must add players first to this tournament!", "D'oh!");
                return;
            }

            //Housekeeping with the latest round
            if (objTournMain.Rounds.Count > 0)
            {

                //Grab the latest information before checking the scores and calculating them
                using (SQLite.SQLiteConnection conn = new SQLite.SQLiteConnection(App.DB_PATH))
                {
                    objTournMain = new TournamentMain();
                    objTournMain = conn.GetWithChildren<TournamentMain>(intTournID, true);
                }

                TournamentMainRound latestRound = objTournMain.Rounds[objTournMain.Rounds.Count - 1];

                //Before starting a new round, make sure we're not missing any scores
                foreach (TournamentMainRoundTable table in latestRound.Tables)
                {
                    if (!table.Player1Winner && !table.Player2Winner)
                    {
                        DisplayAlert("Warning!", "Verify that all tables are completed for the current round!", "D'oh!");
                        return;
                    }
                }

                Utilities.CalculatePlayerScores(ref objTournMain);
            }

            //Grab list of currently active players in the tournament
            Dictionary<int, List<TournamentMainPlayer>> dctActiveTournamentPlayerScores = new Dictionary<int, List<TournamentMainPlayer>>();

            List<TournamentMainPlayer> lstActiveTournamentPlayers = new List<TournamentMainPlayer>();
            List<TournamentMainPlayer> lstActiveTournamentPlayers_Byes = new List<TournamentMainPlayer>();

            foreach (TournamentMainPlayer player in objTournMain.Players)
            {
                if (player.Active)
                {
                    TournamentMainPlayer roundPlayer = new TournamentMainPlayer();
                    roundPlayer.PlayerId = player.PlayerId;
                    roundPlayer.Score = player.Score;

                    if (!player.Bye)
                        lstActiveTournamentPlayers.Add(roundPlayer);
                    else
                    {
                        lstActiveTournamentPlayers_Byes.Add(roundPlayer);
                        player.Bye = false;  //No longer has a Bye for the next round
                        player.ByeCount++;
                    }
                }
            }

            

            //Create a new round
            TournamentMainRound round = new TournamentMainRound();
            round.TournmentId = intTournID;
            round.Number = objTournMain.Rounds.Count + 1;

            if (objTournMain.Rounds.Count == 0)
            {
                //First round, completely random player pairings
                lstActiveTournamentPlayers.Shuffle();
            }
            else
            {
                //Subsequent rounds, group up players with same win count as much as possible and randomize 
                dctActiveTournamentPlayerScores = new Dictionary<int, List<TournamentMainPlayer>>();
                for (int i = objTournMain.Rounds.Count; i >= 0; i--)
                {
                    dctActiveTournamentPlayerScores.Add(i, new List<TournamentMainPlayer>());
                    foreach (TournamentMainPlayer activePlayer in lstActiveTournamentPlayers)
                    {
                        if (i == activePlayer.Score)
                        {
                            dctActiveTournamentPlayerScores[i].Add(activePlayer);
                        }
                    }

                    dctActiveTournamentPlayerScores[i].Shuffle(); //Shuffle all the players in each win bracket
                }

                //Clear out the active list, then go down the list and re-add them back in.
                lstActiveTournamentPlayers.Clear();
                for (int i = objTournMain.Rounds.Count; i >= 0; i--)
                {
                    if (dctActiveTournamentPlayerScores.ContainsKey(i))
                    {
                        foreach (TournamentMainPlayer activePlayer in dctActiveTournamentPlayerScores[i])
                        {
                            lstActiveTournamentPlayers.Add(activePlayer);
                        }
                    }
                }

                //If odd number of players, the last in the list will get a Bye
                //Get the lowest ranked player that hasn't had a bye already
                if (lstActiveTournamentPlayers.Count % 2 != 0)
                {                   
                    foreach(TournamentMainPlayer player in objTournMain.Players.OrderByDescending(obj => obj.Rank).ToList())
                    {
                        if (player.ByeCount == 0)
                        {
                            for (int i = lstActiveTournamentPlayers.Count - 1; i > 0; i--)
                            {
                                if (lstActiveTournamentPlayers[i].PlayerId == player.PlayerId)
                                {
                                    TournamentMainPlayer roundPlayer = lstActiveTournamentPlayers[i];
                                    lstActiveTournamentPlayers.RemoveAt(i);
                                    lstActiveTournamentPlayers.Add(roundPlayer);
                                    break;
                                }
                            }
                            break;
                        }
                    }
                }
            }

            //Create each table, pair 'em up
            TournamentMainRoundTable roundTable = new TournamentMainRoundTable();
            foreach (TournamentMainPlayer player in lstActiveTournamentPlayers)
            {
                if (roundTable.Player1Id != 0 && roundTable.Player2Id != 0)
                {
                    setRoundTableNames(ref roundTable);
                    round.Tables.Add(roundTable);
                    roundTable = new TournamentMainRoundTable();
                }

                if (roundTable.Player1Id == 0)
                {
                    roundTable.Number = round.Tables.Count + 1;
                    roundTable.Player1Id = player.PlayerId;
                }
                else if (roundTable.Player2Id == 0)
                {
                    roundTable.Player2Id = player.PlayerId;
                }
            }

            //If the last table of non-manual byes is an odd-man, set table/player as a bye
            if (roundTable.Player2Id == 0)
            {
                roundTable.Bye = true;
                roundTable.Player1Score = objTournMain.MaxPoints / 2;
                roundTable.Player1Winner = true;

                foreach (TournamentMainPlayer player in objTournMain.Players)
                {
                    if (player.Id == roundTable.Player2Id)
                    {
                        player.ByeCount++;
                        break;
                    }                    
                }
            }

            setRoundTableNames(ref roundTable);
            round.Tables.Add(roundTable);


            //If a manual bye (such as first-round byes at a tournament), add these players now
            if (lstActiveTournamentPlayers_Byes.Count > 0)
            {
                lstActiveTournamentPlayers_Byes.Shuffle();
                foreach (TournamentMainPlayer player in lstActiveTournamentPlayers_Byes)
                {
                    roundTable = new TournamentMainRoundTable();
                    roundTable.Number = round.Tables.Count + 1;
                    roundTable.Player1Id = player.PlayerId;
                    roundTable.Bye = true;
                    roundTable.Player1Score = objTournMain.MaxPoints / 2;
                    roundTable.Player1Winner = true;
                    setRoundTableNames(ref roundTable);
                    round.Tables.Add(roundTable);
                }
            }

            //Add/Save the round
            using (SQLite.SQLiteConnection conn = new SQLite.SQLiteConnection(App.DB_PATH))
            {
                try
                {
                    conn.Update(objTournMain); //Update any other information that was saved such as Bye counts and such
                    conn.InsertWithChildren(round, true);
                }
                catch (Exception ex)
                {
                    DisplayAlert("Warning!", "Error adding round to tournament! " + ex.Message, "OK");
                }

                OnAppearing();
            }
        }

        //Show Standings
        private void currentStandingsBtn_Activated(object sender, EventArgs e)
        {
            Navigation.PushAsync(new Tournaments_Standings(intTournID));
        }



        //Delete the last round
        async private void deleteRoundBtn_Activated(object sender, EventArgs e)
        {

            var confirmed = await DisplayAlert("Confirm", "Do you want to delete Round " + objTournMain.Rounds.Count + "?  This cannot be undone!", "Yes", "No");
            if (confirmed)
            {
                using (SQLite.SQLiteConnection conn = new SQLite.SQLiteConnection(App.DB_PATH))
                {

                    try
                    {
                        //Grab the latest round and delete it
                        TournamentMainRound round = objTournMain.Rounds[objTournMain.Rounds.Count - 1];
                        conn.Delete(round, true);
                    }
                    catch (Exception ex)
                    {
                        await DisplayAlert("Warning!", "Error deleting round from tournament! " + ex.Message, "OK");
                    }

                    OnAppearing();
                }
            }
        }
        #endregion

    }
}