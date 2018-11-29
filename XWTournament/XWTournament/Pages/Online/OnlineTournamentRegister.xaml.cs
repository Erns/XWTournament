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

            //List<Player> lstApiPlayers = JsonConvert.DeserializeObject<List<Player>>(JsonConvert.DeserializeObject(content).ToString());

            this.IsBusy = false;
            loadingOverlay.IsVisible = false;
        }
    }
}