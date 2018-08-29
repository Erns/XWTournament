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
	public partial class Players_List : ContentPage
	{
        private bool blnActive;
        private List<Player> lstViewPlayers;

		public Players_List (string strTitle, bool blnActive)
		{
			InitializeComponent ();
            Title = strTitle;
            this.blnActive = blnActive;
		}

        protected override void OnAppearing()
        {
            base.OnAppearing();
            lstViewPlayers = new List<Player>();

            using (SQLite.SQLiteConnection conn = new SQLite.SQLiteConnection(App.DB_PATH))
            {
                conn.CreateTable<Player>();

                lstViewPlayers = conn.Query<Player>("SELECT * FROM Player WHERE Active = ? AND DateDeleted IS NULL ORDER BY Name", blnActive);
                playersListView.ItemsSource = lstViewPlayers;
            }
        }

        public void OpenPlayer(TextCell sender, EventArgs e)
        {
            Navigation.PushAsync(new Players_AddEdit(Convert.ToInt32(sender.CommandParameter.ToString())));
        }

        void Handle_FabClicked(object sender, System.EventArgs e)
        {
            Navigation.PushAsync(new Players_AddEdit(blnActive));
        }

        private void playersListView_SearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            playersListView.BeginRefresh();

            if (string.IsNullOrWhiteSpace(e.NewTextValue))
                playersListView.ItemsSource = lstViewPlayers;
            else
                playersListView.ItemsSource = lstViewPlayers.Where(i => i.Name.ToUpper().Contains(e.NewTextValue.ToUpper()));

            playersListView.EndRefresh();
        }
    }
}