﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using FallApp.Models;

namespace FallApp.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class MenuPage : ContentPage
	{
        MainPage RootPage { get => Application.Current.MainPage as MainPage; }
        List<HomeMenuItem> menuItems;
		public MenuPage ()
		{
			InitializeComponent ();

            menuItems = new List<HomeMenuItem>
            {
                new HomeMenuItem {Id = MenuItemType.Application, Title = "Aplikacja"},
                new HomeMenuItem {Id = MenuItemType.Settings, Title = "Ustawienia"},
                new HomeMenuItem {Id = MenuItemType.Accelerometer, Title = "Akcelerometr"},
                new HomeMenuItem {Id = MenuItemType.About, Title = "O aplikacji"}
            };

            ListViewMenu.ItemsSource = menuItems;

            ListViewMenu.SelectedItem = menuItems[0];
            ListViewMenu.ItemSelected += async (sender, e) =>
            {
                if (e.SelectedItem == null)
                    return;

                var id = (int)((HomeMenuItem)e.SelectedItem).Id;
                await RootPage.NavigateFromMenu(id);
            };
		}
	}
}