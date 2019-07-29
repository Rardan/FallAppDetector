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
            //MessagingCenter.Send<SettingsPage, string>(this, "lab", L_A_B.Text);
            //MessagingCenter.Send<SettingsPage, string>(this, "uab", U_A_B.Text);
            //MessagingCenter.Send<SettingsPage, string>(this, "mra", M_R_A.Text);
            //MessagingCenter.Send<SettingsPage, string>(this, "mav", M_A_V.Text);
        }
    }
}