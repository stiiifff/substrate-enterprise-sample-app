using System.Threading.Tasks;

namespace Parity.Substrate.EnterpriseSample.Services
{
    public interface IAccountService
    {
        Task<(Polkadot.DataStructs.Address, string, string)> GenerateSr25519KeyPairAsync(string name, string password);
    }
}
