using Microsoft.Office.Interop.Excel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace price_bot.Networking;
public class BaseClient
{
    string baseUrl = "";
    static 
        #if !DEBUG
    readonly
        #endif
        HttpClient client = new();

    public BaseClient() 
    {
#if DEBUG
        HttpClientHandler clientHandler = new HttpClientHandler();
        clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
        client = new HttpClient(clientHandler);
#endif
    }

    internal async Task<string> Get(string url)
    {
        HttpResponseMessage response = await client.GetAsync(url);
        return await response.Content.ReadAsStringAsync();
    }
    internal async Task<string> Post<T>(string url, T content)
    {
        var stringContent = new StringContent(JsonConvert.SerializeObject(content).ToLower(), Encoding.UTF8, "application/json");

        HttpResponseMessage response = await client.PostAsync(url, stringContent);
        return await response.Content.ReadAsStringAsync();
    }

    internal async Task<string> Patch<T>(string url, T content)
    {
        var stringContent = new StringContent(JsonConvert.SerializeObject(content).ToLower(), Encoding.UTF8, "application/json");

        HttpResponseMessage response = await client.PatchAsync(url, stringContent);
        return await response.Content.ReadAsStringAsync();
    }
}