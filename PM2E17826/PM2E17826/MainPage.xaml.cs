using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using PM2E17826.Controller;
using PM2E17826.Models;
using Plugin.Geolocator;
using Xamarin.Essentials;
using Plugin.Media;
using Xamarin.Forms.Xaml;
using System.IO;

namespace PM2E17826
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : ContentPage
    {
        string base64ball = "";
        public bool takedfoto = false;
        public MainPage()
        {
            InitializeComponent();
            Conexion();
        }

        private async void Conexion()
        {
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                await DisplayAlert("Sin Internet", "Por favor active su conexion a internet", "Ok");
                return;
            }
            else
            {
                await DisplayAlert("Bienvenido amigo", "Cuenta con Internet", "Ok");
                Ubicacion();
            }
        }//FIN


        // INICIO UBICACION
        private async void Ubicacion()
        {
            if (!CrossGeolocator.IsSupported)
            {
                await DisplayAlert("Error", "Ha ocurrido un error al cargar el plugin", "OK");
                return;
            }

            if (CrossGeolocator.Current.IsGeolocationEnabled)
            {
                CrossGeolocator.Current.PositionChanged += Current_PositionChanged;

                await CrossGeolocator.Current.StartListeningAsync(new TimeSpan(0, 0, 1), 0.5);

            }
            else
            {
                await DisplayAlert("Error", "El GPS no esta activo en este dispositivo, por favor enciendalo para usar el GPS", "OK");
            }

        }

        private void Current_PositionChanged(object sender, Plugin.Geolocator.Abstractions.PositionEventArgs e)
        {
            if (!CrossGeolocator.Current.IsListening)
            {

                return;
            }
            var position = CrossGeolocator.Current.GetPositionAsync();

            txtlatitud.Text = position.Result.Latitude.ToString();
            txtlongitud.Text = position.Result.Longitude.ToString();
        }

        private async void btncargarimg_Clicked(object sender, EventArgs e)
        {
            var tomarfoto = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
            {
                Directory = "miApp",
                Name = "Image.jpg"

            });

            if (tomarfoto != null)
            {
                imagen.Source = ImageSource.FromStream(() => { return tomarfoto.GetStream(); });
            }

            Byte[] imagenByte = null;

            using (var stream = new MemoryStream())
            {
                tomarfoto.GetStream().CopyTo(stream);
                tomarfoto.Dispose();
                imagenByte = stream.ToArray();

                base64ball = Convert.ToBase64String(imagenByte);

            }

        }
        public async void GetLocation()
        {
            Location Location = await Geolocation.GetLastKnownLocationAsync();

            if (Location == null)
            {
                Location = await Geolocation.GetLocationAsync(new GeolocationRequest
                {
                    DesiredAccuracy = GeolocationAccuracy.Medium,
                    Timeout = TimeSpan.FromSeconds(30)
                }); ;
            }

            txtlatitud.Text = Location.Latitude.ToString();
            txtlongitud.Text = Location.Longitude.ToString();


        }

       

        private void btn01_Clicked(object sender, EventArgs e)
        {
            GetLocation();
        }

        private async void btn02_Clicked(object sender, EventArgs e)
        {
            
            await Navigation.PushAsync(new Views.ubicacionesPage());
        }

        private async void btnguardar_Clicked(object sender, EventArgs e)
        {
            try
            {
               
                    var ubicaciones = new Models.Ubicaciones
                {
                    latitud = Convert.ToDouble(this.txtlatitud.Text),
                    longitud = Convert.ToDouble(this.txtlongitud.Text),
                    descripcion = this.txtdescripcion.Text,
                    base64 = base64ball

                };

                var resultado = await App.Basedatos02.GrabarUbicacion(ubicaciones);


                if (resultado == 1)
                {
                    await DisplayAlert("Mensaje", "Los datos fueron agregados Exitosamente", "OK");
                }
                
                else
                {
                    await DisplayAlert("Mensaje", "Error, No se logro guardar Ubicacion", "OK");

                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Mensaje", ex.Message.ToString(), "OK");

            }
        }

        private void Btnsalir_Clicked(object sender, EventArgs e)
        {
            System.Environment.Exit(0);
        }
    }
}
