using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Essentials;

namespace Delivery
{
    public static class GlobalVar
    {
       public static string _ConnectionString = @"***USE YOUR DB***";
       public static List<Order> _GlobalOrder { get; set; }

        public static Location _RestaurantLocation = new Location(52.97997, -1.15285);
        public static string _RestaurantAddress = "162 Haydn Road, Nottingham";


    }
}
