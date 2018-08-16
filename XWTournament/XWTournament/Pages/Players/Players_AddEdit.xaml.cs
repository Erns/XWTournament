using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XWTournament.Classes;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XWTournament.Models;

namespace XWTournament.Pages.Players
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class Players_AddEdit : ContentPage
	{
        private Player openPlayer;

        //Starting a new player
		public Players_AddEdit (bool blnActive = true)
		{
			InitializeComponent();
            openPlayer = new Player();
            activeSwitch.IsToggled = blnActive;
            deleteButton.IsVisible = false;
        }

        //Opening an existing player
        public Players_AddEdit(int Id)
        {
            InitializeComponent();
            openPlayer = new Player();

            using (SQLite.SQLiteConnection conn = new SQLite.SQLiteConnection(App.DB_PATH))
            {
                conn.CreateTable<Player>();
                //List<Player> player = conn.Query<Player>("SELECT * FROM Player WHERE Id = ?", Id);

                openPlayer = conn.Get<Player>(Id);

                nameEntry.Text = openPlayer.Name;
                emailEntry.Text = openPlayer.Email;
                activeSwitch.IsToggled = openPlayer.Active;
            }

        }

        //Save
        private void saveButton_Clicked(object sender, EventArgs e)
        {
            Player player = new Player()
            {
                Name = nameEntry.Text,
                Email = emailEntry.Text,
                Active = activeSwitch.IsToggled
            };

            player.Id = openPlayer.Id;

            if (player.Name == null || player.Name.ToString().Trim() == "")
            {
                DisplayAlert("Warning!", "Please enter a player name!", "OK");
                return;
            }

            using (SQLite.SQLiteConnection conn = new SQLite.SQLiteConnection(App.DB_PATH))
            {

                int numberOfRows = 0;

                if (player.Id > 0)
                {
                    numberOfRows = conn.Update(player);

                    //Update player names within the tournament player rosters
                    conn.ExecuteScalar<TournamentMainPlayer>("UPDATE TournamentMainPlayer SET PlayerName = ? WHERE PlayerId = ?", player.Name, player.Id);

                    if (numberOfRows > 0) DisplayAlert("Success", "Player successfully updated", "Great!");
                    else DisplayAlert("Failure", "Player failed to be updated", "Oops!");
                }
                else
                {
                    numberOfRows = conn.Insert(player);
                    //if (numberOfRows > 0) DisplayAlert("Success", "Player successfully created", "Great!");
                    //else DisplayAlert("Failure", "Player failed to be created", "Oops!");

                    if (numberOfRows == 0) DisplayAlert("Failure", "Player failed to be created", "Oops!");

                }

                Navigation.PopAsync();
            }
        }

        //Delete
        async void deleteButton_Clicked(object sender, EventArgs e)
        {
            var confirmed = await DisplayAlert("Confirm", "Do you want to delete this player?", "Yes", "No");
            if (confirmed)
            {
                using (SQLite.SQLiteConnection conn = new SQLite.SQLiteConnection(App.DB_PATH))
                {
                    conn.CreateTable<Player>();
                    openPlayer.DateDeleted = DateTime.Now;
                    conn.Update(openPlayer);
                }
                await Navigation.PopAsync();
            }

        }
    }
}