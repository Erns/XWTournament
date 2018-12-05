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

namespace XWTournament.Pages.Online
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class OnlineTournamentRegister : ContentPage
	{
        RestClient client = Utilities.InitializeRestClient();

        public OnlineTournamentRegister ()
		{
			InitializeComponent ();

            if (App.IsUserLoggedIn)
            {
                searchButton.IsVisible = true;
            }
            else
            {
                searchButton.IsVisible = false;
            }
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
                searchResultsStack.IsVisible = true;
            }

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
    }
}