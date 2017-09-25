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
using System.ComponentModel;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace WeatherCustomControl
{
    public sealed partial class WeatherControl : UserControl
    {
        private readonly String APIKEY = "ac0cb38f985c1560e474bfab1f947b71";
        private readonly String URLPREFIX = "http://api.openweathermap.org/data/2.5/weather?";

        public String ZipCode { get; set; }
        public bool UnitsInCelcius { get; set; }
        public double CurrentTemperatureKelvin { get; set; }

        public WeatherControl()
        {
            ZipCode = "98052";
            UnitsInCelcius = true;
            UpdateDisplay();
            this.InitializeComponent();
        }

        private bool IsUSorCanadianZipCode(String zipCode)
        {
            if (String.IsNullOrEmpty(zipCode)) return false;
            bool isValidUsOrCanadianZip = false;
            String pattern = @"^\d{5}-\d{4}|\d{5}|[A-Z]\d[A-Z] \d[A-Z]\d$";
            Regex regex = new Regex(pattern);
            return isValidUsOrCanadianZip = regex.IsMatch(zipCode);
        }

        public async void UpdateDisplay()
        {
            if (!IsUSorCanadianZipCode(ZipCode))
            {
                //throw new Exception("Invalid zip code: a valid US zip code must be provided");
                IconImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/no-internet-3.png"));
            }
            
            String url = URLPREFIX + "zip=" + ZipCode + ",us&appid=" + APIKEY;

            CurrentWeatherInfo currentWeather;
            String json = await DownloadWeatherDataAsync(url);

            if (!string.IsNullOrEmpty(json))
            {
                currentWeather = JsonConvert.DeserializeObject<CurrentWeatherInfo>(json);

                Debug.WriteLine(currentWeather.weather[0].description);


                CurrentTemperatureKelvin = currentWeather.main.temp;
                SetDisplayTemperature();

                City.Text = currentWeather.name;
                Humidity.Text = "Humidity: " + currentWeather.main.humidity.ToString() + "%";
                UpdateIcon(currentWeather.weather[0].icon);
            }

            // If data is not found, display "no connection" icon
            else
            {
                IconImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/no-internet-3.png"));
            }

            DateTime currentDate = DateTime.Now;
            String longMonthName = currentDate.ToString("MMMM", new CultureInfo("en-us"));
            Date.Text = longMonthName + " " + currentDate.Day;
        }

        public void SetDisplayTemperature()
        {
            int displayTemperature;
            if (UnitsInCelcius)
            {
                displayTemperature = (int)kelvinToCelcius(CurrentTemperatureKelvin);
                Temp.Text = displayTemperature.ToString() + " °C";
            }
            else
            {
                displayTemperature = (int)kelvinToFahrenheit(CurrentTemperatureKelvin);
                Temp.Text = displayTemperature.ToString() + " °F";
            }
        }

        public void UpdateIcon(String iconID)
        {
            // Uncomment the code below to use the default OpenWeatherMap icons

            //String imageURI = "http://openweathermap.org/img/w/" + iconID + ".png";
            //var bitmapImage = new BitmapImage();
            //bitmapImage.UriSource = new Uri(imageURI);
            //IconImage.Source = bitmapImage;
            //return;

            String iconAssetName;
            Debug.WriteLine(iconID);

            switch (iconID)
            {
                case "01d":
                    iconAssetName = "icon-sunny";
                    break;
                case "o1n":
                    iconAssetName = "icon-night-clear";
                    break;
                case "02d":
                    iconAssetName = "icon-sunny-fewclouds";
                    break;
                case "02n":
                    iconAssetName = "icon-night-fewclouds";
                    break;
                case "03d":
                    iconAssetName = "icon-sunny-scatteredclouds";
                    break;
                case "03n":
                    iconAssetName = "icon-night-scatteredclouds";
                    break;
                case "04d":
                case "04n":
                    iconAssetName = "icon-brokenclouds";
                    break;
                case "09d":
                case "09n":
                    iconAssetName = "icon-lightrain";
                    break;
                case "10d":
                case "10n":
                    iconAssetName = "icon-rain";
                    break;
                case "11d":
                case "11n":
                    iconAssetName = "icon-thunderstorm";
                    break;
                case "13d":
                case "13n":
                    iconAssetName = "icon-snow";
                    break;
                case "50d":
                case "50n":
                    iconAssetName = "icon-mist";
                    break;
                default:
                    iconAssetName = "no-internet-3";
                    break;

            }

            Debug.WriteLine("ms-appx:///Assets/WeatherIcons" + iconAssetName + ".png");

            IconImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/WeatherIcons/" + iconAssetName + ".png"));

        }

        public double kelvinToFahrenheit(double kelvin)
        {
            return 1.8 * (kelvin - 273) + 32;
        }

        public double kelvinToCelcius(double kelvin)
        {
            return kelvin - 272.15;
        }

        public double fahrenheitToCelcius(double fahrenheit)
        {
            return (fahrenheit - 32) / 1.8;
        }

        public double celciusToFahrenheit(double celcius)
        {
            return (celcius * 1.8) + 32;
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
        
        private void OptionsClick(object sender, RoutedEventArgs e)
        {
            if (!InfoSplitView.IsPaneOpen)
            {
                ZipCodeTextBox.Text = ZipCode;
            }
            InfoSplitView.IsPaneOpen = !InfoSplitView.IsPaneOpen;
        }

        private void FahrenheitChecked(object sender, RoutedEventArgs e)
        {
            UnitsInCelcius = false;
            SetDisplayTemperature();
        }

        private void CelciusChecked(object sender, RoutedEventArgs e)
        {
            UnitsInCelcius = true;
            SetDisplayTemperature();
        }

        private void InfoSplitView_PaneClosed(SplitView sender, object args)
        {
            Temp.Text = "-----";
            Humidity.Text = "-----";
            City.Text = "-----";
            this.ZipCode = ZipCodeTextBox.Text;
            UpdateDisplay();
        }
    }
}
