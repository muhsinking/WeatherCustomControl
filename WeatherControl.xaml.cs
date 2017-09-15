using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace WeatherCustomControl
{
    public sealed partial class WeatherControl : UserControl
    {
        private string url = "http://api.openweathermap.org/data/2.5/weather?zip=98122,us&appid=ac0cb38f985c1560e474bfab1f947b71";

        public WeatherControl()
        {
            UpdateDisplay();

            this.InitializeComponent();
        }

        public async void UpdateDisplay()
        {
            CurrentWeatherInfo currentWeather;
            String json = await DownloadWeatherDataAsync(url);
            currentWeather = !string.IsNullOrEmpty(json) ? JsonConvert.DeserializeObject<CurrentWeatherInfo>(json) : new CurrentWeatherInfo();

            Debug.WriteLine(currentWeather.weather[0].description);

            int tempInt = (int)kelvinToFahrenheit(currentWeather.main.temp);
            Temp.Text = tempInt.ToString() + "°F";
            City.Text = currentWeather.name;
            Precipitation.Text = "Humidity: " + currentWeather.main.humidity.ToString() + "%";
            //UpdateIcon(currentWeather.weather[0]);
        }


        // Update the icon photo based on the icon ID
        /* 
         * TODO look up icon source with hashset
         * TODO filename convention: 
         * 01   icon-clear.png
         * 02   icon-fewclouds
         * 03   icon-scattered-clouds
         * 04   icon-broken-clouds
         * 09   icon-shower-rain
         * 10   icon-rain
         * 11   icon-thunderstorm
         * 13   icon-snow
         * 50   icon-mist
        */

        public void UpdateIcon(string iconID)
        {

        }

        public double kelvinToFahrenheit(double kelvin)
        {
            return 1.8 * (kelvin - 273) + 32;
        }

        public double kelvinToCelcius(double kelvin)
        {
            return kelvin - 272.15;
        }

        private async Task<string> DownloadWeatherDataAsync(string url)
        {
            using (var h = new HttpClient())
            {
                string json_data = string.Empty;
                HttpResponseMessage httpResponse = new HttpResponseMessage();
                json_data = string.Empty;
                Uri requestUri = new Uri(url);

                // attempt to download JSON data as a string
                try
                {
                    httpResponse = await h.GetAsync(requestUri);
                    httpResponse.EnsureSuccessStatusCode();
                    json_data = await httpResponse.Content.ReadAsStringAsync();
                }
                catch (Exception) { }

                Debug.WriteLine("\n\n\n");
                Debug.WriteLine(json_data);
                Debug.WriteLine("\n\n\n");

                return json_data;
            }
        }

        // Update display when weather icon is clicked
        private void IconClick(object sender, RoutedEventArgs e)
        {
            Temp.Text = "--";
            //City.Text = "--";
            Precipitation.Text = "--";
            UpdateDisplay();
        }
    }
}
