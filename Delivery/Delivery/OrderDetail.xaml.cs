using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.GoogleMaps;
using Xamarin.Forms.Xaml;
using static Delivery.GlobalVar;
using static Delivery.SQL;

namespace Delivery
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class OrderDetail : ContentPage
    {
        private Pin pinCustomer;
        private Pin pinRestaurant;
        public int OrderId { get; set; }
        public string Staff { get; set; }
        public string CustomerEmail { get; set; }
        public string TotalSpending { get; set; }




        public OrderDetail(Order order, string staff)
        {
            InitializeComponent();
            
                
                captionText.Text = "ORDER #" + order.OrderId + "\nAddress: " + order.CustomerAddress + ", " + order.CustomerPostCode;
                OrderId = order.OrderId;
                Staff = staff;
                CustomerEmail = order.Customer;
                TotalSpending = order.TotalAmount.ToString();
                GetCustomerPosition(order.CustomerAddress);
            if(order.Status == "ReadyForDelivery")
            {
                actionButton.Text = "TAKE ORDER";
            }
            else if(order.Status == "OnTheWay")
            {
                actionButton.Text = "FINISH ORDER";
                actionButton.BackgroundColor = Color.FromRgb(0, 0, 70);
            }
            
        }

        public async void GetCustomerPosition(string orderAddress)
        {
            var geocoder = new Xamarin.Forms.GoogleMaps.Geocoder();
            var positions = await geocoder.GetPositionsForAddressAsync(orderAddress);
            Position customerPosition = positions.First();

            pinCustomer = new Pin()
            {
                Type = PinType.SavedPin,
                Icon = BitmapDescriptorFactory.DefaultMarker(Color.DarkRed),
                Label = "CUSTOMER",
                Address = orderAddress,
                Position = customerPosition,
                Rotation = 33.3f,
                Tag = "id_customer",
                IsVisible = true,
                IsDraggable = true

            };

            pinRestaurant = new Pin()
            {
                Type = PinType.SavedPin,
                Icon = BitmapDescriptorFactory.DefaultMarker(Color.CadetBlue),
                Label = "RESTAURANT",
                Address = _RestaurantAddress,
                Position = new Position(_RestaurantLocation.Latitude, _RestaurantLocation.Longitude),
                Rotation = 33.3f,
                Tag = "id_restaurant",
                IsVisible = true,
                IsDraggable = false
                
            };

            await Task.Run(() => Thread.Sleep(100));
            trackMap.Pins.Add(pinCustomer);
            trackMap.Pins.Add(pinRestaurant);
            await trackMap.MoveCamera(CameraUpdateFactory.NewCameraPosition(new CameraPosition(customerPosition, 17d)));
        }

        public void AdjustMap(Pin customer, Pin restaurant)
        {

            //MAIN ADRRESS: 162 Haydn Road, Nottingham

            //SCENARIO 1: 34 Hazelwood Rd, Nottingham
            //SCENARIO 2: 75 Britannia Ave, Nottingham
            //SCENARIO 3: 402A Woodborough Rd, Nottingham
            //SCENARIO 4: 2 Brookdale Ct, Nottingham

            if (customer.Position.Latitude < restaurant.Position.Latitude && customer.Position.Longitude < restaurant.Position.Longitude) // SCENARIO 1
            {
                Position southwestBound = new Position(customer.Position.Latitude - 0.003, customer.Position.Longitude - 0.006);
                Position northeastBound = new Position(restaurant.Position.Latitude + 0.003, restaurant.Position.Longitude + 0.006);
                var bounds = new Bounds(southwestBound, northeastBound);
                trackMap.MoveToRegion(MapSpan.FromBounds(bounds));
                //DisplayAlert("SET BOUNDS", "SCENARIO 1", "OK");
            }
            else if (customer.Position.Latitude > restaurant.Position.Latitude && customer.Position.Longitude < restaurant.Position.Longitude) // SCENARIO 2
            {
                Position southwestBound = new Position(customer.Position.Latitude + 0.003, customer.Position.Longitude - 0.006);
                Position northeastBound = new Position(restaurant.Position.Latitude - 0.003, restaurant.Position.Longitude + 0.006);
                var bounds = new Bounds(northeastBound, southwestBound);
                trackMap.MoveToRegion(MapSpan.FromBounds(bounds));
                //DisplayAlert("SET BOUNDS", "SCENARIO 2", "OK");
            }
            else if (customer.Position.Latitude < restaurant.Position.Latitude && customer.Position.Longitude > restaurant.Position.Longitude) // SCENARIO 3
            {
                Position southwestBound = new Position(customer.Position.Latitude - 0.004, customer.Position.Longitude + 0.003);
                Position northeastBound = new Position(restaurant.Position.Latitude + 0.004, restaurant.Position.Longitude - 0.003);
                var bounds = new Bounds(southwestBound, northeastBound);
                trackMap.MoveToRegion(MapSpan.FromBounds(bounds));
                //DisplayAlert("SET BOUNDS", "SCENARIO 3", "OK");
            }
            else if (customer.Position.Latitude > restaurant.Position.Latitude && customer.Position.Longitude > restaurant.Position.Longitude) // SCENARIO 4
            {
                Position southwestBound = new Position(customer.Position.Latitude + 0.003, customer.Position.Longitude + 0.006);
                Position northeastBound = new Position(restaurant.Position.Latitude - 0.003, restaurant.Position.Longitude - 0.006);
                var bounds = new Bounds(southwestBound, northeastBound);
                trackMap.MoveToRegion(MapSpan.FromBounds(bounds));
                //DisplayAlert("SET BOUNDS", "SCENARIO 4", "OK");
            }

        }

        public async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync(true);
        }

        public async void OnTakeOrderClicked(object sender, EventArgs e)
        {
            if (actionButton.Text == "TAKE ORDER")
            {
                var request = new GeolocationRequest(GeolocationAccuracy.Best);
                var location = await Geolocation.GetLocationAsync(request);

                mainGrid.IsVisible = false;
                UpdateOrder(OrderId, "Status", "OnTheWay");
                UpdateOrder(OrderId, "Staff", Staff);
                UpdateOrder(OrderId, "StatusChanged", DateTime.Now.ToString("HH:mm:ss"));
                UpdateCurrentStaffOrdersLocation(location.Latitude, location.Longitude, Staff);
                await Task.WhenAll
                (
                captionFrame.TranslateTo(-2000, 0, 400),
                buttonsFrame.TranslateTo(-2000, 0, 400)
                );
                AdjustMap(pinCustomer, pinRestaurant);
            }
            else if(actionButton.Text == "FINISH ORDER")
            {
                UpdateOrder(OrderId, "Status", "Finished");
                UpdateOrder(OrderId, "StatusChanged", DateTime.Now.ToString("HH:mm:ss"));
                UpdateCustomerOrdersAmount(CustomerEmail);
                UpdateCustomerTotalSpending(CustomerEmail, TotalSpending);
                UpdateCustomer(CustomerEmail, "HasActiveOrder", "False");
                await Navigation.PopModalAsync(true);
            }
        }
    }
}