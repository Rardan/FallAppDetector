using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;
using Plugin.Sensors;


namespace FallApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ApplicationPage : ContentPage
    {
        //public const int SAMPLING_FREQ = 10; // W milisekundach
        //public const double LOWER_ACCEL_BOUNDRY = 0.836;
        //public const double UPPER_ACCEL_BOUNDRY = 1.652;
        //public const double MAX_RESTING_AMPLITUDE = 0.1;
        //public const double MAX_ANGULAR_VELOCITY = 5; // w radianach/s

        public const int SAMPLING_FREQ = 10; // W milisekundach
        public const double LOWER_ACCEL_BOUNDRY = 0.885;
        public const double UPPER_ACCEL_BOUNDRY = 1.456;
        public const double MAX_RESTING_AMPLITUDE = 0.3;
        public const double MAX_ANGULAR_VELOCITY = 3.5; // w radianach/s

        //public const int SAMPLING_FREQ = 10; // W milisekundach
        //public double LOWER_ACCEL_BOUNDRY { get; set; }
        //public double UPPER_ACCEL_BOUNDRY { get; set; }
        //public double MAX_RESTING_AMPLITUDE { get; set; }
        //public double MAX_ANGULAR_VELOCITY { get; set; }

        public float x, y, z, gyroX, gyroY, gyroZ;
        private bool stopPressed;
        public ApplicationPage()
        {
            stopPressed = false;
            InitializeComponent();

            MessagingCenter.Subscribe<SettingsPage, string>(this, "telefon", (sender, arg) =>
            {
                telephone.Text = arg;
            });

            //MessagingCenter.Subscribe<SettingsPage, string>(this, "lab", (sender, arg) =>
            //{
            //    LOWER_ACCEL_BOUNDRY = Convert.ToDouble(arg);
            //});

            //MessagingCenter.Subscribe<SettingsPage, string>(this, "uab", (sender, arg) =>
            //{
            //    UPPER_ACCEL_BOUNDRY = Convert.ToDouble(arg);
            //});

            //MessagingCenter.Subscribe<SettingsPage, string>(this, "mra", (sender, arg) =>
            //{
            //    MAX_RESTING_AMPLITUDE = Convert.ToDouble(arg);
            //});

            //MessagingCenter.Subscribe<SettingsPage, string>(this, "mav", (sender, arg) =>
            //{
            //    MAX_ANGULAR_VELOCITY = Convert.ToDouble(arg);
            //});

            if (!Accelerometer.IsMonitoring)
            {
                Accelerometer.ReadingChanged += Accelerometer_ReadingChanged;
                Accelerometer.Start(SensorSpeed.UI);
            }
            if (!Gyroscope.IsMonitoring)
            {
                Gyroscope.ReadingChanged += Gyroscope_ReadingChanged;
                Gyroscope.Start(SensorSpeed.UI);
            }

            FallTest();
        }

        private void Accelerometer_ReadingChanged(object sender, AccelerometerChangedEventArgs e)
        {
            x = e.Reading.Acceleration.X;
            y = e.Reading.Acceleration.Y;
            z = e.Reading.Acceleration.Z;
        }
        private void Gyroscope_ReadingChanged(object sender, GyroscopeChangedEventArgs e)
        {
            gyroX = e.Reading.AngularVelocity.X;
            gyroY = e.Reading.AngularVelocity.Y;
            gyroZ = e.Reading.AngularVelocity.Z;
        }

        private async Task FallTest()
        {
            CircularBuffer<double> AccelAmps = new CircularBuffer<double>(2000 / SAMPLING_FREQ);   //pamietamy wartosci z akcelometra przez ostatnie dwie sekundy
            CircularBuffer<double> GyroAmps = new CircularBuffer<double>(2000 / SAMPLING_FREQ);   //pamietamy wartosci z gyroskopu przez ostatnie dwie sekund
            {
                while (true)
                {
                    await Task.Delay(SAMPLING_FREQ);
                    AccelAmps.Enqueue(Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2) + Math.Pow(z, 2)));    // amplituda przyspieszen x,y,z w accelometrze
                    GyroAmps.Enqueue(Math.Sqrt(Math.Pow(gyroX, 2) + Math.Pow(gyroY, 2) + Math.Pow(gyroZ, 2)));    // amplituda prędkości x,y,z w żyroskopie
                    if (AccelAmps.q.Min() < LOWER_ACCEL_BOUNDRY && AccelAmps.q.Max() > UPPER_ACCEL_BOUNDRY && AccelAmps.q.Max() > MAX_ANGULAR_VELOCITY) // Sprawdzenie czy zostaly przekroczone dolne i gorne progi przyspieszenia, oraz gorny prog prędkości kątowej gyroskopu
                    {
                        for (int i = 0; i < 3000 / SAMPLING_FREQ; i++)
                        {
                            await Task.Delay(SAMPLING_FREQ);
                            AccelAmps.Enqueue(Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2) + Math.Pow(z, 2)));    // amplituda przyspieszen x,y,z w accelometrze
                            GyroAmps.Enqueue(Math.Sqrt(Math.Pow(gyroX, 2) + Math.Pow(gyroY, 2) + Math.Pow(gyroZ, 2)));    // amplituda prędkości x,y,z w żyroskopie
                        }
                        if (Math.Abs(AccelAmps.q.Min() - AccelAmps.q.Max()) < MAX_RESTING_AMPLITUDE) // i sprawdzamy czy telefon byl nieruchomy przez ostatnie dwie aby potwierdzic
                        {
                            /************************************ UPADEK ******************************/
                            CrossSensors.Proximity.WhenReadingTaken().Subscribe(async reading =>
                            {
                                if (reading == true)
                                {
                                    await FallDetectedAsync();
                                }
                            });
                        }

                        //await Task.Delay(3000);                                              // po mozliwym upadku czekamy trzy sekundy
                        //if (Math.Abs(AccelAmps.q .Min() - AccelAmps.q.Max()) < MAX_RESTING_AMPLITUDE) // i sprawdzamy czy telefon byl nieruchomy przez ostatnie dwie aby potwierdzic
                        //{
                        //    /************************************ UPADEK ******************************/
                        //    await FallDetectedAsync();
                        //    //CrossSensors.Proximity.WhenReadingTaken().Subscribe(async reading => {
                        //    //    if (reading == true)
                        //    //    {

                            //    //    }
                            //    //});
                            //}
                        }
                }
            }
        }

        private void StopButton_Clicked(object sender, EventArgs e)
        {
            stopPressed = true;
            Vibration.Cancel();
            stopButton.IsEnabled = false;
            fallDetectedLabel.IsVisible = false;

        }

        private async Task FallDetectedAsync()
        {
            stopPressed = false;
            stopButton.IsEnabled = true;
            fallDetectedLabel.IsVisible = true;
            //System.Threading.Thread.Sleep(10000);

            var duration = TimeSpan.FromSeconds(10);
            Vibration.Vibrate(duration);

            await Task.Delay(10000);
            if (stopPressed == false)
            {
                if (telephone.Text != null)
                {
                    string latitude = null;
                    string longitude = null;
                    try
                    {
                        var request = new GeolocationRequest(GeolocationAccuracy.Best);
                        var location = await Geolocation.GetLocationAsync(request);

                        if (location != null)
                        {
                            latitude = location.Latitude.ToString();
                            longitude = location.Longitude.ToString();

                            latitude = latitude.Replace(',', '.');
                            longitude = longitude.Replace(',', '.');
                        }
                    }
                    catch (FeatureNotSupportedException fnsEx)
                    { }
                    catch (FeatureNotEnabledException fneEx)
                    { }
                    catch (PermissionException pEx)
                    { }
                    catch (Exception ex)
                    { }

                    if (latitude != null && longitude != null)
                    {
                        string link = "https://www.google.com/maps/search/?api=1&query=" + latitude + "," + longitude;
                        string msg = "Pomocy, upadłem! Moja lokalizacja: " + link;
                        await SendSms(msg, telephone.Text);
                    }
                }
            }
        }
        //life cycle xamarin background task
        public async Task SendSms(string messageText, string recipient)
        {
            try
            {
                var message = new SmsMessage(messageText, new[] { recipient });
                await Sms.ComposeAsync(message);
            }
            catch (FeatureNotSupportedException ex)
            { }
            catch (Exception ex)
            { }
        }

        public class CircularBuffer<T>
        {
            private int limit;
            public CircularBuffer(int limit)
            {
                this.limit = limit;
            }
            private T trash;
            public Queue<T> q = new Queue<T>();
            private object lockObject = new object();
            public void Enqueue(T obj)
            {
                q.Enqueue(obj);
                if (q.Count > limit)
                    trash = q.Dequeue();
            }
        }
    }
}