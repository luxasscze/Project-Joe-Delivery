using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Essentials;

namespace Delivery
{
    public static class GlobalVar
    {
       public static string _ConnectionString = @"Data Source=SQL5097.site4now.net;Initial Catalog=DB_A63C5C_Joe;User Id=DB_A63C5C_Joe_admin;Password=Kasumi2Goto";
       public static List<Order> _GlobalOrder { get; set; }

        public static Location _RestaurantLocation = new Location(52.97997, -1.15285);
        public static string _RestaurantAddress = "162 Haydn Road, Nottingham";


    }
}
