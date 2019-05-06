using System;
using System.Collections.Generic;
using System.Text;

namespace FallApp.Models
{
    public enum MenuItemType
    {
        Settings,
        Application,
        Accelerometer,
        About
    }
    class HomeMenuItem
    {
        public MenuItemType Id {get; set;}

        public string Title { get; set; }
    }
}
