using Android.Content.PM;
using Java.IO;
using Java.Lang;
using System;
using System.IO;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Parity.Substrate.EnterpriseSample.Services
{
    public class AccountService : IAccountService
    {
        public AccountService(ApplicationInfo applicationInfo)
        {
            ApplicationInfo = applicationInfo;
        }

        public ApplicationInfo ApplicationInfo { get; }

        public async Task<(Polkadot.DataStructs.Address, string, string)> GenerateSr25519KeyPairAsync(string name, string password)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException($"'{nameof(name)}' cannot be null or whitespace", nameof(name));
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException($"'{nameof(password)}' cannot be null or whitespace", nameof(password));
            }

            var subkey = Path.Combine(ApplicationInfo.NativeLibraryDir, "subkey");
            var output = await RunCommandAsync($"{subkey} generate");

            var lines = output.Split(Environment.NewLine);
            if (string.IsNullOrEmpty(output))
                throw new InvalidOperationException("Error generating sr25519 keypair.");

            var mnemonicStart = lines[0].IndexOf('`') + 1;
            var mnemonic = lines[0].Substring(mnemonicStart, lines[0].LastIndexOf('`') - mnemonicStart);
            var secret = lines[1].Substring(lines[1].IndexOf(':') + 1).Trim();
            var pubkey = lines[2].Substring(lines[2].IndexOf(':') + 1).Trim();
            var account = lines[3].Substring(lines[3].IndexOf(':') + 1).Trim();
            var address = lines[4].Substring(lines[4].IndexOf(':') + 1).Trim();



            return (new Polkadot.DataStructs.Address(address), mnemonic, secret);
        }

        private async Task<string> RunCommandAsync(string command)
        {
            try
            {
                var process = Runtime.GetRuntime().Exec(command);
                using var input = new InputStreamReader(process.InputStream);
                using var reader = new BufferedReader(input);

                string line;
                var output = new System.Text.StringBuilder();
                while (!string.IsNullOrEmpty(line = await reader.ReadLineAsync()))
                {
                    output.AppendLine(line);
                }
                reader.Close();
                await process.WaitForAsync();

                return output.ToString();
            }
            catch (Java.IO.IOException e)
            {
                throw new RuntimeException(e);
            }
            catch (InterruptedException e)
            {
                throw new RuntimeException(e);
            }
        }
    }
}
