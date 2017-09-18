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
using Windows.UI.Xaml.Media.Imaging;
using System.Globalization;
using System.Text.RegularExpressions;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace WeatherCustomControl
{



    public sealed partial class WeatherControl : UserControl
    {
        public static readonly DependencyProperty ZipCodeProperty = DependencyProperty.Register(
            "ZipCode",
            typeof(string),
            typeof(WeatherControl),
            new PropertyMetadata("94040")
        );

        public string ZipCode
        {
            get { return (string)GetValue(ZipCodeProperty); }
            set { SetValue(ZipCodeProperty, value); }
        }

        private readonly string APIKEY = "ac0cb38f985c1560e474bfab1f947b71";
        private readonly string URLPREFIX = "http://api.openweathermap.org/data/2.5/weather?";

        public WeatherControl()
        {
            UpdateDisplay();
            this.InitializeComponent();
        }

        private bool IsUSorCanadianZipCode(string zipCode)
        {
            if (string.IsNullOrEmpty(zipCode)) return false;
            bool isValidUsOrCanadianZip = false;
            string pattern = @"^\d{5}-\d{4}|\d{5}|[A-Z]\d[A-Z] \d[A-Z]\d$";
            Regex regex = new Regex(pattern);
            return isValidUsOrCanadianZip = regex.IsMatch(zipCode);
        }

        public async void UpdateDisplay()
        {
            if (!IsUSorCanadianZipCode(ZipCode))
            {
                throw new Exception("Invalid zip code: a valid US zip code must be provided");
            }
            
            string url = URLPREFIX + "zip=" + ZipCode + ",us&appid=" + APIKEY;

            CurrentWeatherInfo currentWeather;
            String json = await DownloadWeatherDataAsync(url);

            if (!string.IsNullOrEmpty(json))
            {
                currentWeather = JsonConvert.DeserializeObject<CurrentWeatherInfo>(json);

                Debug.WriteLine(currentWeather.weather[0].description);

                int tempInt = (int)kelvinToFahrenheit(currentWeather.main.temp);
                Temp.Text = tempInt.ToString() + "°F";
                City.Text = currentWeather.name;
                Humidity.Text = "Humidity: " + currentWeather.main.humidity.ToString() + "%";
                UpdateIcon("http://openweathermap.org/img/w/" + currentWeather.weather[0].icon + ".png");
            }

            // If data is not found, display "no connection" icon
            else
            {
                IconImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/no-internet-3.png"));
            }

            DateTime currentDate = DateTime.Now;
            string longMonthName = currentDate.ToString("MMMM", new CultureInfo("en-us"));
            Date.Text = longMonthName + " " + currentDate.Day;
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

        public void UpdateIcon(string imageURI)
        {
            var bitmapImage = new BitmapImage();
            bitmapImage.UriSource = new Uri(imageURI);
            IconImage.Source = bitmapImage;
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
            Temp.Text = "-----";
            Humidity.Text = "-----";
            UpdateDisplay();
        }
    }
}
