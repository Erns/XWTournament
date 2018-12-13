using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XWTournament.Classes;
using XWTournament.Models;
using XWTournament.ViewModel;

namespace XWTournament.Pages.Online
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class OnlineTournaments : TabbedPage
	{
        RestClient client = Utilities.InitializeRestClient();

        private static List<TournamentMainRoundTable> associatedLogScoreTables = new List<TournamentMainRoundTable>();

        #region "Search Tournaments"


        public OnlineTournaments()
		{
			InitializeComponent ();

            //Shouldn't be able to even get to this point if not logged in, but just in case
            if (App.IsUserLoggedIn)
            {
                searchButton.IsVisible = true;
                onlineTournamentsLogScorePage.IsVisible = true;
                LoadOnlineActiveTournamentsAsync();
            }
            else
            {
                searchButton.IsVisible = false;
                onlineTournamentsLogScorePage.IsVisible = false;
                associatedLogScoreTables = new List<TournamentMainRoundTable>();
            }
		}


        private void searchButton_Pressed(object sender, EventArgs e)
        {
            this.IsBusy = true;
            loadingOverlay.IsVisible = true;
        }

        private async void searchButton_ClickedAsync(object sender, EventArgs e)
        {
            this.IsBusy = true;
            loadingOverlay.IsVisible = true;

            TournamentMain tournament = new TournamentMain()
            {
                Name = nameEntry.Text,
                StartDate = dateEntry.Date
            };


            //Search tournaments open to the public
            //Using POST for the sake of passing a tournament object info to search with
            IRestRequest request = new RestRequest("TournamentsSearch", Method.POST);
            //request.AddUrlSegment("userid", App.CurrentUser.Id);
            request.AddJsonBody(JsonConvert.SerializeObject(tournament));

            // execute the request
            var response = await client.ExecuteTaskAsync(request);
            string content = response.Content;

            //List<TournamentMain> returnedTournaments = JsonConvert.DeserializeObject<List<TournamentMain>>(JsonConvert.DeserializeObject(content).ToString());
            List<TournamentMain> returnedTournaments = new List<TournamentMain>();

            for (int i = 1; i < 15; i++){
                TournamentMain tmpTournament = new TournamentMain()
                {
                    Id = i,
                    Name = "Testing " + i,
                    StartDate = DateTime.Now
                };
                returnedTournaments.Add(tmpTournament);
            };


            if (returnedTournaments.Count > 0)
            {
                searchResultsListView.ItemsSource = returnedTournaments;
                searchResultsListView.IsVisible = true;
            }

            searchResultsLabel.IsVisible = true;

            this.IsBusy = false;
            loadingOverlay.IsVisible = false;
        }

        private async void searchTournamentItem_TappedAsync(TextCell sender, EventArgs e)
        {
            var answer = await DisplayAlert("Register", "Would you like to register for tournament " + sender.Text + " Id " + sender.CommandParameter + "?", "Yes", "No");
            if (answer)
            {
                //Register register
                await DisplayAlert("Confirmed", "Successfully registered for tournament!", "Sweet");

            }
            //Navigation.PushAsync(new Players_AddEdit(Convert.ToInt32(sender.CommandParameter.ToString())));
        }

        #endregion


        #region "Log Scores"

        private async void LoadOnlineActiveTournamentsAsync()
        {
            logScoreLoadingOverlay.IsVisible = true;

            ////Search tournaments open to the public
            ////Using POST for the sake of passing a tournament object info to search with
            //IRestRequest request = new RestRequest("TournamentsSearch", Method.GET);
            //request.AddUrlSegment("userid", App.CurrentUser.Id.ToString());

            //// execute the request
            //var response = await client.ExecuteTaskAsync(request);
            //string content = response.Content;

            //List<TournamentMain> returnedTournaments = JsonConvert.DeserializeObject<List<TournamentMain>>(JsonConvert.DeserializeObject(content).ToString());
            List<TournamentMain> returnedTournaments = new List<TournamentMain>();

            associatedLogScoreTables = new List<TournamentMainRoundTable>();

            //Go through the returned associated tournaments, grab the latest round's info and ensure the user is at one of the tables
            foreach (TournamentMain tournament in returnedTournaments)
            {
                if (tournament.Rounds.Count > 0)
                {
                    TournamentMainRound lastestRound = tournament.Rounds[tournament.Rounds.Count - 1];

                    int intTournPlayerId = 0;
                    foreach (TournamentMainPlayer player in tournament.Players)
                    {
                        if (player.API_UserAccountId == App.CurrentUser.Id)
                        {
                            intTournPlayerId = player.PlayerId;
                            break;
                        }
                    }

                    if (intTournPlayerId > 0)
                    {
                        foreach (TournamentMainRoundTable table in lastestRound.Tables)
                        {
                            if (table.Player1Id == intTournPlayerId || table.Player2Id == intTournPlayerId)
                            {
                                table.TableName = string.Format("{0}: {1}", tournament.Name, table.TableName);
                                associatedLogScoreTables.Add(table);
                                break;
                            }
                        }
                    }
                }               
            }

            //Test data
            for(int i = 1; i < 5; i++)
            {
                TournamentMainRoundTable tmpTable = new TournamentMainRoundTable()
                {
                    Id = i,
                    RoundId = (i * 100),
                    TableName = string.Format("{0}: {1}", "TournTest" + i, "TableTest" + i),
                    Player1Name = "player 1 - " + i,
                    Player2Name = "player 2 - " + i
                };
                associatedLogScoreTables.Add(tmpTable);
            }


            logScoreTableListView.ItemsSource = associatedLogScoreTables;

            logScoreLoadingOverlay.IsVisible = false;
        }

        #endregion

        private void logScoreTable_Tapped(TextCell sender, EventArgs e)
        {
            int intTableId = Convert.ToInt32(sender.CommandParameter.ToString());
            if (intTableId > 0)
            {
                foreach (TournamentMainRoundTable table in associatedLogScoreTables)
                {
                    if (table.Id == intTableId)
                    {
                        logScoreWindowOverlayGrid.BindingContext = new TournamentMainRoundTable_ViewModel(table, false);
                        logScoreWindowOverlay.IsVisible = true;
                        break;
                    }
                }

            }
        }

        //Hide timer popup when hitting the back button
        protected override bool OnBackButtonPressed()
        {
            if (logScoreWindowOverlay.IsVisible)
            {
                logScoreWindowOverlay.IsVisible = false;
                return true;    //Prevent back button from continuing
            }
            else
            {
                return base.OnBackButtonPressed();
            }
        }
    }
}