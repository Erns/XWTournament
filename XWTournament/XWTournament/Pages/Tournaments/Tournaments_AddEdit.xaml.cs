using SQLiteNetExtensions.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XWTournament.Classes;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XWTournament.Models;

namespace XWTournament.Pages.Tournaments
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class Tournaments_AddEdit : ContentPage
	{

        private TournamentMain openTournament;

        //Open new tournament
		public Tournaments_AddEdit ()
		{
			InitializeComponent ();
            openTournament = new TournamentMain();
            deleteButton.IsVisible = false;
		}

        //Open existing tournament
        public Tournaments_AddEdit(int intTournID)
        {
            InitializeComponent();
            openTournament = new TournamentMain();

            using (SQLite.SQLiteConnection conn = new SQLite.SQLiteConnection(App.DB_PATH))
            {
                openTournament = conn.GetWithChildren<TournamentMain>(intTournID);

                nameEntry.Text = openTournament.Name;
                pointMaxEntry.Text = openTournament.MaxPoints.ToString();
                dateEntry.Date = openTournament.StartDate;
                minutesEntry.Text = openTournament.RoundTimeLength.ToString();
            }
        }
        

        private void saveButton_Clicked(object sender, EventArgs e)
        {

            openTournament.Name = nameEntry.Text;
            openTournament.StartDate = dateEntry.Date;
            if (!string.IsNullOrWhiteSpace(pointMaxEntry.Text)) openTournament.MaxPoints = Convert.ToInt32(Math.Round(Convert.ToDecimal(pointMaxEntry.Text)));
            if (!string.IsNullOrWhiteSpace(minutesEntry.Text)) openTournament.RoundTimeLength = Convert.ToInt32(Math.Round(Convert.ToDecimal(minutesEntry.Text)));

            //Check Name
            if (openTournament.Name == null || openTournament.Name.ToString().Trim() == "")
            {
                DisplayAlert("Warning!", "Please enter a tournament name!", "OK");
                return;
            }

            //Check points
            if (openTournament.MaxPoints <= 0)
            {
                DisplayAlert("Warning!", "Please enter a valid point number!", "OK");
                return;
            }

            //Check time
            if (openTournament.RoundTimeLength <= 0)
            {
                DisplayAlert("Warning!", "Please enter a valid round length!", "OK");
                return;
            }

            //Check Date
            try
            {
                DateTime.Parse(openTournament.StartDate.ToString());
            }
            catch
            {
                DisplayAlert("Warning!", "Please enter a valid tournament date!", "OK");
                return;
            }

            //Update database
            using (SQLite.SQLiteConnection conn = new SQLite.SQLiteConnection(App.DB_PATH))
            {

                //Create
                try
                {
                    
                    if (openTournament.Id == 0)
                    {
                        conn.InsertWithChildren(openTournament);
                        DisplayAlert("Success", "Tournament successfully created", "OK");

                    }
                    else
                    {
                        conn.UpdateWithChildren(openTournament);
                        DisplayAlert("Success", "Tournament successfully updated", "OK");

                    }
                }
                catch
                {
                    DisplayAlert("Failure", "An error occurred when creating this tournament", "OK");

                }

                Navigation.PopAsync();
            }
            

        }

        //Delete
        async void deleteButton_Clicked(object sender, EventArgs e)
        {
            var confirmed = await DisplayAlert("Confirm", "Do you want to delete this tournament?", "Yes", "No");
            if (confirmed)
            {
                using (SQLite.SQLiteConnection conn = new SQLite.SQLiteConnection(App.DB_PATH))
                {
                    conn.CreateTable<TournamentMain>();
                    openTournament.DateDeleted = DateTime.Now;
                    conn.Update(openTournament);
                }

                //Remove this page and the previous page from navigation stack (remove edit page, remove tournament info page, returning to tournament main list)
                Navigation.RemovePage(Navigation.NavigationStack[Navigation.NavigationStack.Count - 1]);
                Navigation.RemovePage(Navigation.NavigationStack[Navigation.NavigationStack.Count - 1]);
            }

        }
    }
}