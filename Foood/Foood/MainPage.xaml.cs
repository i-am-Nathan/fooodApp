using Plugin.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Foood
{
	public partial class MainPage : ContentPage
	{
 
		public MainPage()
		{
			InitializeComponent();
		}

        private async void takeButtonClicked(object sender, EventArgs e)
        {
            await CrossMedia.Current.Initialize();

            //Make sure that there is a camera and can take photos available
            if(CrossMedia.Current.IsCameraAvailable && CrossMedia.Current.IsTakePhotoSupported)
            {
                var MediaOptions = new Plugin.Media.Abstractions.StoreCameraMediaOptions
                {
                    Directory = "Receipts",
                    Name = $"{DateTime.UtcNow}.jpg"
                };

                //Takes photo of the business receipt
                var file = await CrossMedia.Current.TakePhotoAsync(MediaOptions);
            }
        }
	}
}
