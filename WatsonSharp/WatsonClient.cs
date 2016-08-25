using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;

namespace WatsonSharp
{
	public class WatsonClient
    {
		private const string _watsonServiceUrl = "https://gateway.watsonplatform.net/personality-insights/api/v2/profile";

		public string GetTraitPercentageForCharacteristic(string profileName, string characteristicName)
		{
			var bluemixUsername = ConfigurationManager.AppSettings["Bluemix.Username"];
			var bluemixPassword = ConfigurationManager.AppSettings["Bluemix.Password"];
			string jsonResponse = null;

			var serviceUrl = _watsonServiceUrl;
			if (!String.IsNullOrEmpty(profileName))
				serviceUrl = String.Format("{0}{1}{2}", serviceUrl, "?profileName=", profileName);

			var request = (HttpWebRequest)WebRequest.Create(serviceUrl);

			// Have to set a proxy if using Fiddler
			request.Proxy = new WebProxy("127.0.0.1", 8888);
			try
			{
				var auth = string.Format("{0}:{1}", bluemixUsername, bluemixPassword);
				var auth64 = Convert.ToBase64String(Encoding.ASCII.GetBytes(auth));
				var credentials = string.Format("{0} {1}", "Basic", auth64);

				request.Headers[HttpRequestHeader.Authorization] = credentials;
				request.Method = "POST";
				request.Accept = "application/json";
				request.ContentType = "text/plain";
				request.Headers["X-SyncTimeOut"] = "30";

				var encoding = new UTF8Encoding();
				var payload = Encoding.GetEncoding("iso-8859-1").GetBytes(GetCorpus());
				request.ContentLength = payload.Length;
				using (var callStream = request.GetRequestStream())
				{
					callStream.Write(payload, 0, payload.Length);
				}
			}
			catch (Exception e)
			{
				Console.Out.WriteLine("error:" + e.Message);
				Console.ReadKey();
			}

			try
			{
				var qaResponse = request.GetResponse();
				var requestStream = qaResponse.GetResponseStream();
				var responseReader = new StreamReader(requestStream);
				jsonResponse = responseReader.ReadToEnd();
				responseReader.Close();
			}
			catch (System.Net.WebException e)
			{
				Console.Out.WriteLine("errors:" + e.Message);
				Console.ReadKey();
			}
			catch (Exception e)
			{
				Console.Out.WriteLine("error:" + e.Message);
				Console.ReadKey();
			}

			dynamic jsonResult = JsonConvert.DeserializeObject(jsonResponse);

			var jObject = JsonConvert.DeserializeObject<JObject>(jsonResult);
			var jToken = jObject.SelectToken(String.Format("$..*[?(@.name=='{0}')]", characteristicName));
			var percentage = Convert.ToDouble(jToken["percentage"]);
			return percentage;
		}

		public static string GetCorpus()
		{
			// Replace the contents of this method with your code for fetching the body of text you'd like to be analysed
			return
				@"Well, thank you very much, Jim, for this opportunity. I want to thank Governor Romney and the University
            of Denver for your hospitality.";
		}
	}
}
