using Plugin.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Plugin.Media.Abstractions;
using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Net.Http.Headers;
using Foood.Model;
using Newtonsoft.Json;

namespace Foood
{
	public partial class MainPage : ContentPage
	{
 
		public MainPage()
		{
			InitializeComponent();
		}

        private async void uploadButtonClicked(object sender, EventArgs e)
        { 
            await CrossMedia.Current.Initialize();

            if (!CrossMedia.Current.IsPickPhotoSupported)
            {
                await DisplayAlert("Pick Photo Unsupported", ":(Cannot pick photos.", "OK");
            }
            var photo = await CrossMedia.Current.PickPhotoAsync();

            foodLabel.Text = "";
            await MakePredictionRequest(photo);
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

                image.Source = ImageSource.FromStream(() =>
                {
                    return file.GetStream();
                });

                foodLabel.Text = "";

                await MakePredictionRequest(file);
            }
        }

        private async Task MakePredictionRequest(MediaFile file)
        {
            Contract.Ensures(Contract.Result<Task>() != null);

            var client = new HttpClient();

            client.DefaultRequestHeaders.Add("Prediction-Key", "9b2cd115e674489c892a12aa9b4747f2");

            String url = "https://southcentralus.api.cognitive.microsoft.com/customvision/v1.0/Prediction/f7b0efdf-bdfe-43bd-956a-1fc151887c04/url?iterationId=475ef7ec-5997-401d-95ad-8d6b535614aa";

            HttpResponseMessage response;

            byte[] byteData = GetImageAsByteArray(file);

            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                response = await client.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    PredictionModel responseModel = JsonConvert.DeserializeObject<PredictionModel>(responseString);

                    List<Prediction> predictions = responseModel.Predictions;

                    foreach (Prediction prediction in predictions)
                    {
                        if (prediction.Probability >= 0.8)
                        {
                            foodLabel.Text = prediction.Tag;
                            file.Dispose();
                            return;
                        }
                    }

                    foodLabel.Text = "What is this? Is this even food?";
                }
            }
        
            file.Dispose();
        }

        private byte[] GetImageAsByteArray(MediaFile file)
        {
            var stream = file.GetStream();
            BinaryReader br = new BinaryReader(stream);
            return br.ReadBytes((int)stream.Length);
        }
    }
}
