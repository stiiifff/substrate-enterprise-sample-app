using System.Security;
using System.Threading.Tasks;

namespace Parity.Substrate.EnterpriseSample.Services
{
    public interface IAccountService
    {
        Task<(Polkadot.DataStructs.Address, string)> GenerateSr25519KeyPairAsync(string name, string password);
        Task<SecureString> RetrieveAccountSecretAsync(string name);
    }
}
