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

namespace XWTournament.Pages
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class OnlineAccount_Main : ContentPage
	{
        RestClient client = Utilities.InitializeRestClient();

        public OnlineAccount_Main ()
		{
			InitializeComponent ();

            loadingOverlay.BindingContext = this;

            if (App.IsUserLoggedIn)
            {
                string strGreeting = string.Format("Welcome, {0}!", (App.CurrentUser.Name != null ? App.CurrentUser.Name : App.CurrentUser.UserName));
                userGreeting.Text = strGreeting;
                logoutUserStack.IsVisible = true;
                loginUserStack.IsVisible = false;
            }

        }

        private async void loginButton_ClickedAsync(object sender, EventArgs e)
        {
            this.IsBusy = true;
            loginFailEntry.IsVisible = false;

            LogoutUser();

            if (string.IsNullOrEmpty(userEntry.Text) || string.IsNullOrEmpty(passwordEntry.Text))
            {
                loginFailEntry.IsVisible = true;
                this.IsBusy = false;
                return;
            }

            //Send API request to login info
            UserAccount user = new UserAccount()
            {
                UserName = userEntry.Text
                , Password = SHA1.Encode(passwordEntry.Text)
            };

            var requestLogin = new RestRequest("UserAccount", Method.GET);
            requestLogin.AddParameter("value", JsonConvert.SerializeObject(user));

            user.Password = "";

            // execute the request
            var responseLogin = await client.ExecuteTaskAsync(requestLogin);
            var contentLogin = responseLogin.Content;

            //Check if we get a fail from API
            if (contentLogin.ToUpper().Contains("GET: FALSE"))
            {
                loginFailEntry.IsVisible = true;
                this.IsBusy = false;
                return;
            }

            try
            {
                UserAccount result = JsonConvert.DeserializeObject<UserAccount>(JsonConvert.DeserializeObject(contentLogin).ToString());
                if (result.Id != 0)
                {
                    using (SQLite.SQLiteConnection conn = new SQLite.SQLiteConnection(App.DB_PATH))
                    {
                        conn.Insert(result);
                        App.IsUserLoggedIn = true;
                        App.CurrentUser = result;

                        App.MasterMainPage.Detail = new NavigationPage(new Players_Main());
                    }               
                }
            }
            catch
            {
                //If we couldn't interpret the response, it failed
                loginFailEntry.IsVisible = true;
                this.IsBusy = false;
                return;
            }

            this.IsBusy = false;

        }

        private void logoutButton_Clicked(object sender, EventArgs e)
        {
            LogoutUser();
            logoutUserStack.IsVisible = false;
            loginUserStack.IsVisible = true;
        }

        private void LogoutUser()
        {
            //Remove UserAccount information off device
            using (SQLite.SQLiteConnection conn = new SQLite.SQLiteConnection(App.DB_PATH))
            {
                conn.CreateTable<UserAccount>();
                conn.ExecuteScalar<UserAccount>("DELETE FROM UserAccount"); //Remove any/all user account info
            }

            App.IsUserLoggedIn = false;
            App.CurrentUser = null;
        }

        private void registerButton_Clicked(object sender, EventArgs e)
        {
            registerUser.IsVisible = true;
            registerUser.Source = "https://xwtweb.gear.host/Account/LoginRegister";
        }
    }
}