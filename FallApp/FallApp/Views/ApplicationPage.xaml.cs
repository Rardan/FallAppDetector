using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;

namespace FallApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ApplicationPage : ContentPage
    {
        public const int OKRES_PROBKOWANIA = 10; // W milisekundach
        public const double DOLNA_GRANICA = 0.836;
        public const double GORNA_GRANICA = 1.652;
        public const double MAKS_AMPLITUDA_W_SPOCZYNKU = 0.05;

        public float x, y, z;
        private bool stopPressed;
        public ApplicationPage()
        {
            stopPressed = false;
            InitializeComponent();

            MessagingCenter.Subscribe<SettingsPage, string>(this, "telefon", (sender, arg) =>
            {
                telephone.Text = arg;
            });

            FallTest();
        }

        private void Accelerometer_ReadingChanged(object sender, AccelerometerChangedEventArgs e)
        {
            x = e.Reading.Acceleration.X;
            y = e.Reading.Acceleration.Y;
            z = e.Reading.Acceleration.Z;
        }

        private void FallTest()
        {
            if (Accelerometer.IsMonitoring)
                return;

            Accelerometer.ReadingChanged += Accelerometer_ReadingChanged;
            Accelerometer.Start(SensorSpeed.UI);

            CircularBuffer<double> AccelAmps = new CircularBuffer<double>(2000 / OKRES_PROBKOWANIA);   //pamietamy wartosci z akcelometra przez ostatnie dwie sekundy
            while (true)
            {
                System.Threading.Thread.Sleep(OKRES_PROBKOWANIA);
                AccelAmps.Enqueue(Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2) + Math.Pow(z, 2)));    // amplituda przyspieszen x,y,z
                if (AccelAmps.q.Min() < DOLNA_GRANICA && AccelAmps.q.Max() > GORNA_GRANICA) // Sprawdzenie czy zostaly przekroczone dolne i gorne progi przyspieszenia
                {
                    System.Threading.Thread.Sleep(3000);                                              // po mozliwym upadku czekamy trzy sekundy
                    if (Math.Abs(AccelAmps.q.Min() - AccelAmps.q.Max()) < MAKS_AMPLITUDA_W_SPOCZYNKU) // i sprawdzamy czy telefon byl nieruchomy przez ostatnie dwie aby potwierdzic
                    {
                        /************************************ UPADEK ******************************/
                        FallDetected();
                    }
                }
            }
        }
        /*
         https://docs.microsoft.com/pl-pl/xamarin/essentials/gyroscope?context=xamarin/xamarin-forms – tutaj masz obsługę żyroskopu

            Gyroscope.ReadingChanged += Gyroscope_ReadingChanged;
            Gyroscope.Start(SensorSpeed.UI);

            void Gyroscope_ReadingChanged(object sender, GyroscopeChangedEventArgs e)
            {
                var data = e.Reading;
                //Console.WriteLine($"Reading: X: {data.AngularVelocity.X}, Y: {data.AngularVelocity.Y}, Z: {data.AngularVelocity.Z}");
            }
             */
        private void StopButton_Clicked(object sender, EventArgs e)
        {
            stopPressed = true;
        }

        private void FallDetected()
        {
            stopPressed = false;
            stopButton.IsEnabled = true;
            fallDetectedLabel.IsVisible = true;
            System.Threading.Thread.Sleep(10000);
            if(stopPressed == false)
            {
                if(telephone.Text != null)
                {
                    try
                    {
                        var message = new SmsMessage("Pomocy, upadlem!", new[] { telephone.Text });
                        Sms.ComposeAsync(message);
                    }
                    catch (FeatureNotSupportedException ex)
                    { }
                    catch (Exception ex)
                    { }
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