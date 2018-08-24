using Plugin.LocalNotifications;
using System;
using System.Threading;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation (XamlCompilationOptions.Compile)]
namespace XWTournament
{
	public partial class App : Application
	{
        public static string DB_PATH = string.Empty;

        public static Pages.MainMenu MasterMainPage;

        public App()
        {
            InitializeComponent();

            MainPage = new Pages.MainMenu();
        }

        //Separate constructor that Android/iOS will use to help set the database path
        //For Android, see Android\MainActivity.cs updated call to set pathways to the phone itself
        public App(string DB_Path)
        {
            InitializeComponent();

            DB_PATH = DB_Path;

            //Set an accessible variable for the root main page so we can access it directly if need be (namely so round timers complete correctly)
            MasterMainPage = new Pages.MainMenu();

            MainPage = MasterMainPage;


        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }

    }
}
