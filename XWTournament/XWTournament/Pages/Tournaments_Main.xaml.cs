using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace XWTournament.Pages
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class Tournaments_Main : TabbedPage
	{
		public Tournaments_Main ()
		{
			InitializeComponent ();

            this.Children.Clear();
            Children.Add(new Pages.Tournaments.Tournaments_List("Current / Upcoming", true));
            Children.Add(new Pages.Tournaments.Tournaments_List("Past", false));
        }
    }
}