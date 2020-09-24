using dotnetstandard_bip39.System.Security.Cryptography;
using Polkadot.DataStructs;
using Polkadot.Utils;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Parity.Substrate.EnterpriseSample.Services
{
    public class AccountService : IAccountService
    {
        public async Task<(Address, string)> GenerateSr25519KeyPairAsync(string name, string password)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException($"'{nameof(name)}' cannot be null or whitespace", nameof(name));
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException($"'{nameof(password)}' cannot be null or whitespace", nameof(password));
            }

            var bip39 = new dotnetstandard_bip39.BIP39();
            var mnemonic = bip39.GenerateMnemonic(128, dotnetstandard_bip39.BIP39Wordlist.English);
            var entropy = bip39.MnemonicToEntropy(mnemonic, dotnetstandard_bip39.BIP39Wordlist.English);
            var seed = SeedFromEntropy(entropy, password);
            var seedHex = seed.ToHexString();

            var sr25519Keypair = sr25519_dotnet.lib.SR25519.GenerateKeypairFromSeed(seedHex);
            var address = AddressUtils.GetAddrFromPublicKey(new PublicKey { Bytes = sr25519Keypair.Public });

            await Xamarin.Essentials.SecureStorage.SetAsync(BuildAccountSecretKey(name), sr25519Keypair.Secret.ToPrefixedHexString());

            return (new Address(address), mnemonic);
        }

        public async Task<SecureString> RetrieveAccountSecretAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException($"'{nameof(name)}' cannot be null or whitespace", nameof(name));
            }

            var secret = await Xamarin.Essentials.SecureStorage.GetAsync(BuildAccountSecretKey(name));
            if (secret == null)
                return null;

            var result = new SecureString();
            Array.ForEach(secret.ToArray(), result.AppendChar);
            result.MakeReadOnly();
            return result;
        }

        private string BuildAccountSecretKey(string name) => $"{name}_secret";

        private byte[] SeedFromEntropy(string entropy, string password)
        {
            if (entropy.Length < 32 || entropy.Length > 64 || entropy.Length % 4 != 0)
            {
                throw new InvalidOperationException("InvalidEntropy");
            }

            var entropyBytes = entropy.HexToByteArray();
            var saltBytes = Encoding.UTF8.GetBytes($"mnemonic{password}");

            var rfc2898DerivedBytes = new Rfc2898DeriveBytesExtended(entropyBytes, saltBytes, 2048, HashAlgorithmName.SHA512);
            var seed = rfc2898DerivedBytes.GetBytes(64);

            return seed;
        }
    }
}
