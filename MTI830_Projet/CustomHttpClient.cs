using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace MTI830_Projet
{
    public class CustomHttpClient
    {
        private static HttpClient client;
        private const String APP_ID = "211de7a0";
        private const String APP_KEY = "f77bb2b1b536fe5450c12866f8eafba7";

        public static HttpClient Client()
        {
            if (client != null)
                return client;
            else
            {
                client = new HttpClient
                {
                    BaseAddress = new Uri("https://od-api.oxforddictionaries.com/api/v1/")
                };
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("app_id", APP_ID);
                client.DefaultRequestHeaders.Add("app_key", APP_KEY);

                return client;
            }
        }
    }
}
