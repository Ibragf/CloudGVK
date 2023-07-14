using BackendGVK.Models;
using BackendGVK.Services.Configs;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;

namespace BackendGVK.Services
{
    public class GoogleCaptcha
    {
        private readonly IOptions<GoogleCaptchaSettings> _config;
        public GoogleCaptcha(IOptions<GoogleCaptchaSettings> config)
        {
            _config = config;
        }
        public async Task<bool> VerifyTokenAsync(string token, bool v2 = true)
        {
            try
            {
                string url;
                if (v2)
                {
                    url = $"https://www.google.com/recaptcha/api/siteverify?secret={_config.Value.v2.SecretKey}&response={token}";
                }
                else url = $"https://www.google.com/recaptcha/api/siteverify?secret={_config.Value.v3.SecretKey}&response={token}";

                using (var client = new HttpClient())
                {
                    var httpResult = await client.GetAsync(url);
                    if (httpResult.StatusCode != HttpStatusCode.OK)
                    {
                        return false;
                    }

                    var responseString = await httpResult.Content.ReadAsStringAsync();
                    var googleResult = JsonConvert.DeserializeObject<GoogleCaptchaModel>(responseString);

                    if (googleResult == null) return false;

                    if (v2) { return googleResult.Success; }
                    else return googleResult.Success && googleResult.Score > 0.5;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
