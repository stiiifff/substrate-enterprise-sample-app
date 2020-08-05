namespace Parity.Substrate.EnterpriseSample.Services
{
    public interface IToastService
    {
        void ShowShortToast(string message);
        void ShowLongToast(string message);
    }
}
