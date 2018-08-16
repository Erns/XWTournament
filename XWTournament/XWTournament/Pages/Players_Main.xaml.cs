using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XWTournament.Classes;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace XWTournament.Pages
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class Players_Main : TabbedPage
	{
		public Players_Main ()
		{
			InitializeComponent ();

            this.Children.Clear();
            Children.Add(new Players.Players_List("Active", true));
            Children.Add(new Players.Players_List("Inactive", false));
        }
    }
}