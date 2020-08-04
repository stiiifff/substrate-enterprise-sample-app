using Prism.Mvvm;

namespace Parity.Substrate.EnterpriseSample.ViewModels
{
    public class ShipmentInfoViewModel : BindableBase
    {
        public ShipmentInfoViewModel(string shipmentId)
        {
            ShipmentId = shipmentId;
        }

        private string shipmentId;
        public string ShipmentId
        {
            get { return shipmentId; }
            set { SetProperty(ref shipmentId, value); }
        }
    }
}
