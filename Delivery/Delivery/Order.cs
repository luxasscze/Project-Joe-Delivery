using System;
using System.Collections.Generic;
using System.Text;

namespace Delivery
{
    public class Order
    {
        public int OrderId { get; set; }
        public string Created { get; set; }
        public string Customer { get; set; }
        public decimal TotalAmount { get; set; }
        public bool IsActive { get; set; }
        public string[] Items { get; set; }
        public decimal DeliveryFee { get; set; }
        public string Status { get; set; }
        public string StatusChanged { get; set; }
        public string Eta { get; set; }
        public string Staff { get; set; }
        public int Score { get; set; }
        public string OrderColor { get; set; }
        public string OrderTextOneColor { get; set; }
        public string OrderTextTwoColor { get; set; }
        public string OrderIdColor { get; set; }
        public string OrderImage { get; set; }
        public string CustomerAddress { get; set; }
        public string CustomerPostCode { get; set; }

        

    }
}
