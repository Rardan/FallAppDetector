using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace FallApp.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class SettingsPage : ContentPage
	{
		public SettingsPage ()
		{
			InitializeComponent ();
		}

        private void Save_Clicked(object sender, EventArgs e)
        {
            //TelephoneNumber = telephone.Text;
            MessagingCenter.Send<SettingsPage, string>(this, "telefon", telephone.Text);
        }
    }
}