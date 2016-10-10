using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using IssueApp.Properties;
using ModernHttpClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IssueApp
{
    public class RedmineService
    {
        public RedmineService(string key)
        {
            _httpClient = new HttpClient(new NativeMessageHandler());
            _httpClient.DefaultRequestHeaders.Add("X-Redmine-API-Key", key);
        }

        public async Task<bool> CheckAsync()
        {
            var uri = new Uri(Resources.ProjectUrl);
            var response = await _httpClient.GetAsync(uri);
            return response.IsSuccessStatusCode;
        }

        public async Task<IssuesResponse> GetIssues()
        {
            try
            {
                var uri = new Uri(Resources.IssueUrl);
                var response = await _httpClient.GetAsync(uri);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var data = JsonConvert.DeserializeObject<IssuesResponse>(json);
                    Formatting(data.issues);
                    return data;
                }
            }
            catch
            {
                // ignored
            }
            return null;
        }

        private void Formatting(Issue[] issues)
        {
            foreach (var issue in issues)
            {
                foreach (var custom in issue.custom_fields)
                {
                    if (custom.id == 3)
                    {
                        var value = (string)custom.value;
                        var latlng = value.Split(',');
                        double lat, lng;
                        if (latlng.Length < 2 ||
                            !double.TryParse(latlng[0], out lat) ||
                            !double.TryParse(latlng[1], out lng))
                            continue;

                        issue.lat = lat;
                        issue.lng = lng;
                    }
                }
            }
        }

        private readonly HttpClient _httpClient;
    }
}
