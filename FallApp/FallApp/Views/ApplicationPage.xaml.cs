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
    public partial class ApplicationPage : ContentPage
    {
		public ApplicationPage ()
		{
			InitializeComponent ();

            MessagingCenter.Subscribe<SettingsPage, string>(this, "telefon", (sender, arg) =>
            {
                telephone.Text = arg;
            });
		}
	}
}