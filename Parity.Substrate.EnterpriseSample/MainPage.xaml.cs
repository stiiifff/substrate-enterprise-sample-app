using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Parity.Substrate.EnterpriseSample
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            var response = await new HttpClient().PostAsync("http://localhost:9933",
                new StringContent("{\"id\":1, \"jsonrpc\":\"2.0\", \"method\": \"system_version\"}", Encoding.UTF8, "application/json"));
            if (response.IsSuccessStatusCode)
            {
                var json = JObject.Parse(await response.Content.ReadAsStringAsync());
                label.Text = json.Value<string>("result");
            }
        }
    }
}
