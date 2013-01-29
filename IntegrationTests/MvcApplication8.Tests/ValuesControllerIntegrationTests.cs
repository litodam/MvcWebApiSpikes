namespace MvcApplication8.Tests
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using MvcApplication8.Controllers;
    using Newtonsoft.Json.Linq;

    [TestClass]
    public class ValuesControllerIntegrationTests : WebApiClassBase
    {
        public ValuesControllerIntegrationTests() : base("localhost", 8080, typeof(ValuesController)) 
        {
        }

        [TestMethod]
        public void ShouldGetValues()
        {
            try
            {
                this.Start();

                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri("http://localhost:8080/");
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));            

                    HttpResponseMessage resp = client.GetAsync("api/values").Result;
                    resp.EnsureSuccessStatusCode();

                    var jsonResult = resp.Content.ReadAsStringAsync().Result;
                    
                    var result = JArray.Parse(jsonResult);
                    Assert.IsTrue(result.Count == 2);
                    Assert.AreEqual("value1", result[0]);
                    Assert.AreEqual("value2", result[1]);

                    Assert.IsFalse(string.IsNullOrEmpty(jsonResult));
                }
            }
            finally
            {
                this.Close();   
            }
        }
    }
}
