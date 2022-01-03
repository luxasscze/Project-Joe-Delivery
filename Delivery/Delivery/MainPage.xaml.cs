using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using static Delivery.SQL;
using static Delivery.GlobalVar;
using System.Data.SqlClient;
using Xamarin.Essentials;
using Xamarin.Forms.GoogleMaps;

namespace Delivery
{
    public partial class MainPage : ContentPage
    {
        public IList<Order> Orders { get; set; }
        public bool IsUpdating { get; set; }
        public string Status { get; set; }
        public string Staff { get; set; }
        public Location DeliveryLocation { get; set; }
        public bool IsLocationUpdating { get; set; }




        


        public MainPage()
        {
            InitializeComponent();
            IsUpdating = false;
            IsLocationUpdating = false;
            Status = "ReadyForDelivery";

            RequestLocationPermission();

            MessagingCenter.Subscribe<App>((App)Application.Current, "OrderTaken", (sender) =>
            {
                IsLocationUpdating = true;
                GetDeliveryLocation(IsLocationUpdating);
            });
        }

        public async void GetDeliveryLocation(bool updatingDatabase)
        {
            var request = new GeolocationRequest(GeolocationAccuracy.Best);
            var location = await Geolocation.GetLocationAsync(request);

            if (updatingDatabase)
            {
                DeliveryLocation.Latitude = location.Latitude;
                DeliveryLocation.Longitude = location.Longitude;                
                UpdateCurrentStaffOrdersLocation(location.Latitude, location.Longitude, Staff);
            }
            else
            {
                DeliveryLocation.Latitude = location.Latitude;
                DeliveryLocation.Longitude = location.Longitude;
            }
        }

        public async void RequestLocationPermission()
        {
            var permissions = await Permissions.CheckStatusAsync<Permissions.LocationAlways>();
            if (permissions != PermissionStatus.Granted)
            {
                permissions = await Permissions.RequestAsync<Permissions.LocationAlways>();
            }
            if (permissions != PermissionStatus.Granted)
            {
                await DisplayAlert("CUSTOMER MOBILE", "You need to grant request for location. Please, restart the app and try it again.", "OK");
            }
            else
            {

            }

        }

        public async void OnLoginClicked(object sender, EventArgs e)
        {
            busyIndicator.Color = Color.GreenYellow;
            busyIndicator.IsRunning = true;
            loginFrame.IsVisible = false;
            try
            {
                await Device.InvokeOnMainThreadAsync(async () =>
                {
                    if (CheckLogin(userNameText.Text, passwordText.Text))
                    {
                        
                        List<Staff> staff = new List<Staff>();
                        staff = GetStaffDetails(userNameText.Text);
                        GetOrders(Status);
                        IsUpdating = true;
                        Staff = staff[0].Surname;
                        StartUpdating();                        
                        busyIndicator.IsRunning = false;
                        loginFrame.IsVisible = false;
                        mainFrame.IsVisible = true;
                        captionText.Text = "Hello " + staff[0].FirstName;
                    }
                    else
                    {
                        IsUpdating = false;
                        busyIndicator.IsRunning = false;
                        loginFrame.IsVisible = true;
                        await DisplayAlert("LOGIN", "LOGIN FAILED", "OK");
                    }
                });
                
            }
            catch(Exception ex)
            {
                IsUpdating = false;
                busyIndicator.IsRunning = false;
                loginFrame.IsVisible = true;
                await DisplayAlert("ALERT", "CONNECTION ERROR\n" + ex.ToString(), "OK");
            }
            
        }

        public void StartUpdating()
        {

            GetOrders(Status);
            Device.StartTimer(TimeSpan.FromSeconds(3), () =>
            {
               
                GetOrders(Status);
                //GetDeliveryLocation(true);

                Task.Run(async () =>
                {
                    var request = new GeolocationRequest(GeolocationAccuracy.Best);
                    var location = await Geolocation.GetLocationAsync(request);
                    UpdateCurrentStaffOrdersLocation(location.Latitude, location.Longitude, Staff);
                });

                if (IsUpdating)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            });
            //IsUpdating = true;
        }

        public void OnOrderTapped(object sender, ItemTappedEventArgs e)
        {
            Order selectedOrder = e.Item as Order;
            Navigation.PushModalAsync(new OrderDetail(selectedOrder, Staff));
        }

        public void OnPendingClicked(object sender, EventArgs e)
        {
            Status = "ReadyForDelivery";
            GetOrders(Status);
            pendingButton.BackgroundColor = Color.FromRgb(33, 55, 33);
            deliveringButton.BackgroundColor = Color.FromRgb(44, 44, 44);
            finishedButton.BackgroundColor = Color.FromRgb(44, 44, 44);
        }

        public void OnDeliveringClicked(object sender, EventArgs e)
        {
            Status = "OnTheWay";
            GetOrders(Status);
            pendingButton.BackgroundColor = Color.FromRgb(44, 44, 44);
            deliveringButton.BackgroundColor = Color.FromRgb(33, 55, 33);
            finishedButton.BackgroundColor = Color.FromRgb(44, 44, 44);
        }

        public void OnFinishedClicked(object sender, EventArgs e)
        {
            Status = "Finished";
            GetOrders(Status);
            pendingButton.BackgroundColor = Color.FromRgb(44, 44, 44);
            deliveringButton.BackgroundColor = Color.FromRgb(44, 44, 44);
            finishedButton.BackgroundColor = Color.FromRgb(33, 55, 33);
        }

        public void GetOrders(string status)
        {
            string commString;
            if (status == "NewOrder")
            {
                commString = "SELECT * FROM Orders WHERE IsActive='True' AND Status='NewOrder' ORDER BY Id DESC";
            }
            else if (status == "InTheKitchen")
            {
                commString = "SELECT * FROM Orders WHERE IsActive='True' AND Status='InTheKitchen' ORDER BY Id DESC";
            }
            else if (status == "ReadyForDelivery")
            {
                commString = "SELECT * FROM Orders WHERE IsActive='True' AND Status='ReadyForDelivery' ORDER BY Id DESC";
            }
            else if (status == "OnTheWay")
            {
                commString = "SELECT * FROM Orders WHERE IsActive='True' AND Status='OnTheWay' AND Staff='" + Staff + "' ORDER BY Id DESC";
            }
            else if (status == "Finished")
            {
                commString = "SELECT * FROM Orders WHERE IsActive='True' AND Status='Finished' ORDER BY Id DESC";
            }
            else
            {
                commString = "SELECT * FROM Orders WHERE IsActive = 'True' ORDER BY Id DESC";
            }
            ordersList.BindingContext = null;
            Orders = new List<Order>();

            using (SqlConnection connection = new SqlConnection(_ConnectionString))
            {
                connection.Open();


                SqlCommand cmd = new SqlCommand
                {

                    CommandText = commString,
                    Connection = connection
                };


                SqlDataReader reader = cmd.ExecuteReader();


                Orders.Clear();

                while (reader.Read())
                {
                    Orders.Add(new Order
                    {
                        OrderId = int.Parse(reader["Id"].ToString()),
                        Created = reader["Created"].ToString(),
                        Customer = reader["Customer"].ToString(),
                        TotalAmount = decimal.Parse(reader["TotalAmount"].ToString()),
                        IsActive = bool.Parse(reader["IsActive"].ToString()),
                        Items = reader["Items"].ToString().Split(';'),
                        DeliveryFee = decimal.Parse(reader["DeliveryFee"].ToString()),
                        Status = reader["Status"].ToString(),
                        StatusChanged = reader["StatusChanged"].ToString(),
                        Eta = reader["Eta"].ToString(),
                        Staff = reader["Staff"].ToString(),
                        Score = int.Parse(reader["Score"].ToString()),
                        CustomerAddress = reader["DeliveryAddress"].ToString() + ", " + reader["DeliveryPostCode"].ToString()


                    });
                }

                connection.Close();
                

                for (int i = 0; i < Orders.Count; i++)
                {
                    if (Orders[i].Status == "NewOrder")
                    {
                        Orders[i].OrderColor = "YellowGreen";
                        Orders[i].OrderTextOneColor = "YellowGreen";
                        Orders[i].OrderTextTwoColor = "Black";
                        Orders[i].OrderIdColor = "DarkGreen";
                        Orders[i].OrderImage = "NewOrder.png";

                    }
                    else if (Orders[i].Status == "InTheKitchen")
                    {
                        Orders[i].OrderColor = "Cyan";
                        Orders[i].OrderTextOneColor = "Cyan";
                        Orders[i].OrderTextTwoColor = "Black";
                        Orders[i].OrderIdColor = "CadetBlue";
                        Orders[i].OrderImage = "InTheKitchen.png";

                    }
                    else if (Orders[i].Status == "ReadyForDelivery")
                    {
                        Orders[i].OrderColor = "DarkGray";
                        Orders[i].OrderTextOneColor = "DarkGray";
                        Orders[i].OrderTextTwoColor = "Black";
                        Orders[i].OrderIdColor = "DimGray";
                        Orders[i].OrderImage = "ReadyForDelivery.png";
                    }
                    else if (Orders[i].Status == "OnTheWay")
                    {
                        Orders[i].OrderColor = "DimGray";
                        Orders[i].OrderTextOneColor = "DimGray";
                        Orders[i].OrderTextTwoColor = "Black";
                        Orders[i].OrderIdColor = "DarkGray";
                        Orders[i].OrderImage = "OnTheWay.png";
                    }
                    else if (Orders[i].Status == "Finished")
                    {
                        Orders[i].OrderColor = "Orange";
                        Orders[i].OrderTextOneColor = "Orange";
                        Orders[i].OrderTextTwoColor = "Black";
                        Orders[i].OrderIdColor = "OrangeRed";
                        Orders[i].OrderImage = "Finished.png";
                    }
                    else if (Orders[i].Status == "Declined")
                    {
                        Orders[i].OrderColor = "Maroon";
                        Orders[i].OrderTextOneColor = "Cyan";
                        Orders[i].OrderTextTwoColor = "Black";
                        Orders[i].OrderIdColor = "DarkBlue";
                    }
                    else
                    {
                        Orders[i].OrderColor = "Black";
                        Orders[i].OrderTextOneColor = "Cyan";
                        Orders[i].OrderTextTwoColor = "DarkBlue";
                        Orders[i].OrderIdColor = "DarkBlue";
                    }
                   
                }

                if (Orders.Count == 0)
                {

                    
                }
                else
                {

                    

                }

                BindingContext = this;
                ordersList.BindingContext = this;

            }
            //ordersList.ForceReload();
        }
    }
}
