using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CRUD
{
    public static class CRUD<T>
    {
        public static string EndPoint { get; set; } = string.Empty;

        public static List<T> GetAll()
        {
            using (var client = new HttpClient())
            {
                var response = client.GetAsync(EndPoint).Result;

                if (response.IsSuccessStatusCode)
                {
                    var json = response.Content.ReadAsStringAsync().Result;
                    return JsonConvert.DeserializeObject<List<T>>(json) ?? new List<T>();
                }
                else
                {
                    throw new Exception($"Error: {response.StatusCode}");
                }
            }
        }

        public static T? GetById(Guid id)
        {
            using (var client = new HttpClient())
            {
                var response = client.GetAsync($"{EndPoint}/{id}").Result;

                if (response.IsSuccessStatusCode)
                {
                    var json = response.Content.ReadAsStringAsync().Result;
                    return JsonConvert.DeserializeObject<T>(json);
                }
                else
                {
                    throw new Exception($"Error: {response.StatusCode}");
                }
            }
        }

        public static List<T> GetBy(string campo, Guid id)
        {
            using (var client = new HttpClient())
            {
                var response = client.GetAsync($"{EndPoint}/{campo}/{id}").Result;

                if (response.IsSuccessStatusCode)
                {
                    var json = response.Content.ReadAsStringAsync().Result;
                    return JsonConvert.DeserializeObject<List<T>>(json) ?? new List<T>();
                }
                else
                {
                    throw new Exception($"Error: {response.StatusCode}");
                }
            }
        }

        public static T? Create(T item)
        {
            using (var client = new HttpClient())
            {
                var content = new StringContent(
                    JsonConvert.SerializeObject(item),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = client.PostAsync(EndPoint, content).Result;

                if (response.IsSuccessStatusCode)
                {
                    var json = response.Content.ReadAsStringAsync().Result;
                    return JsonConvert.DeserializeObject<T>(json);
                }
                else
                {
                    var error = response.Content.ReadAsStringAsync().Result;
                    throw new Exception($"Error: {response.StatusCode} - {error}");
                }
            }
        }

        public static bool Update(Guid id, T item)
        {
            using (var client = new HttpClient())
            {
                var content = new StringContent(
                    JsonConvert.SerializeObject(item),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = client.PutAsync($"{EndPoint}/{id}", content).Result;

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    var error = response.Content.ReadAsStringAsync().Result;
                    throw new Exception($"Error: {response.StatusCode} - {error}");
                }
            }
        }

        public static bool Delete(Guid id)
        {
            using (var client = new HttpClient())
            {
                var response = client.DeleteAsync($"{EndPoint}/{id}").Result;

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    var error = response.Content.ReadAsStringAsync().Result;
                    throw new Exception($"Error: {response.StatusCode} - {error}");
                }
            }
        }
    }
}