using AMS.Models;
using System.Text;
using System.Text.Json;

namespace AMS.Services
{
    public class DbOperations : IDbOperations
    {

        public static string firebaseDatabaseUrl = "https://attendance-tracking-system-ft-default-rtdb.firebaseio.com/";


        /// <summary>
        /// Save's the Data 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="dbDocument"></param>
        /// <returns></returns>
        public async Task<T> SaveData<T>(T data, string dbDocument) where T : BaseModel
        {
            try
            {
                data.Id = Guid.NewGuid().ToString("N");
                string courseJsonString = JsonSerializer.Serialize(data);

                var payload = new StringContent(courseJsonString, Encoding.UTF8, "application/json");

                string url = $"{firebaseDatabaseUrl}" +
                            $"{dbDocument}/" + //student,faculty,course,section
                            $"{data.Id}.json";
                HttpClient client = new();
                var httpResponseMessage = await client.PutAsync(url, payload);

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    var contentStream = await httpResponseMessage.Content.ReadAsStringAsync();

                    var result = JsonSerializer.Deserialize<T>(contentStream);
                    return result;
                }
                return null;

            }
            catch (Exception ex)
            {

                throw;
            }
        }

        /// <summary>
        /// Delete the Data based on Id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dbDocument"></param>
        /// <returns></returns>
        public async Task<bool> DeleteData(string id, string dbDocument)
        {
            try
            {
                string url = $"{firebaseDatabaseUrl}" +
                      $"{dbDocument}/" +
                      $"{id}.json";
                HttpClient client = new();
                var httpResponseMessage = await client.DeleteAsync(url);

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    var contentStream = await httpResponseMessage.Content.ReadAsStringAsync();
                    if (contentStream == "null")
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Update the Data based on Id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <param name="data"></param>
        /// <param name="dbDocument"></param>
        /// <returns></returns>
        public async Task<T> UpdateData<T>(string id, T data, string dbDocument) where T : BaseModel
        {
            try
            {
                data.Id = id;
                string bookJsonString = JsonSerializer.Serialize(data);

                var payload = new StringContent(bookJsonString, Encoding.UTF8, "application/json");

                string url = $"{firebaseDatabaseUrl}" +
                            $"{dbDocument}/" +
                            $"{id}.json";

                HttpClient client = new();
                var httpResponseMessage = await client.PatchAsync(url, payload);

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    var contentStream = await httpResponseMessage.Content.ReadAsStringAsync();

                    if (contentStream != null && contentStream != "null")
                    {
                        var result = JsonSerializer.Deserialize<T>(contentStream);

                        return result;
                    }
                }

                return null;

            }
            catch (Exception ex)
            {

                throw;
            }
        }

        /// <summary>
        /// Gets all the data
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dbDocument"></param>
        /// <returns></returns>
        public async Task<List<T>> GetAllData<T>(string dbDocument) where T : BaseModel
        {
            try
            {
                string url = $"{firebaseDatabaseUrl}" +
                       $"{dbDocument}.json";//studnet,course,section
                HttpClient client = new();
                var httpResponseMessage = await client.GetAsync(url);
                List<T> entries = new();

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    var contentStream = await httpResponseMessage.Content.ReadAsStringAsync();
                    if (contentStream != null && contentStream != "null")
                    {
                        var result = JsonSerializer.Deserialize<Dictionary<string, T>>(contentStream);

                        entries = result.Select(x => x.Value).ToList();
                    }
                }

                return entries;

            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}
