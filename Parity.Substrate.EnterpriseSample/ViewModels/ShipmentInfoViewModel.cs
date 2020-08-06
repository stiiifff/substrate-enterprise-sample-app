using System;
using Prism.Mvvm;

namespace Parity.Substrate.EnterpriseSample.ViewModels
{
    public class ShipmentInfoViewModel : BindableBase
    {
        public ShipmentInfoViewModel(string shipmentId,
            string owner = null, string status = null,
            string[] products = null, DateTime? registered = null)
        {
            ShipmentId = shipmentId;
            Owner = owner;
            Status = status;
            Products = products;
            Registered = registered;
        }

        private string shipmentId;
        public string ShipmentId
        {
            get { return shipmentId; }
            set { SetProperty(ref shipmentId, value); }
        }

        private string owner;
        public string Owner
        {
            get { return owner; }
            set { SetProperty(ref owner, value); }
        }

        private string status;
        public string Status
        {
            get { return status; }
            set { SetProperty(ref status, value); }
        }

        public string[] products;
        public string[] Products
        {
            get { return products; }
            set { SetProperty(ref products, value); }
        }

        private DateTime? registered;
        public DateTime? Registered
        {
            get { return registered; }
            set { SetProperty(ref registered, value); }
        }
    }
}
