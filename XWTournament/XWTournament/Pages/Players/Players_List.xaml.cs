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

		public Players_List (string strTitle, bool blnActive)
		{
			InitializeComponent ();
            Title = strTitle;
            this.blnActive = blnActive;
		}

        protected override void OnAppearing()
        {
            base.OnAppearing();

            using (SQLite.SQLiteConnection conn = new SQLite.SQLiteConnection(App.DB_PATH))
            {
                conn.CreateTable<Player>();

                var lstPlayers = conn.Query<Player>("SELECT * FROM Player WHERE Active = ? AND DateDeleted IS NULL", blnActive);
                playersListView.ItemsSource = lstPlayers;
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

    }
}