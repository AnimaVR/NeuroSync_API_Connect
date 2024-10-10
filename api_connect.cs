using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

public class AudioToFace
 {
     private static readonly string ApiKey = "YOUR-API-KEY"; // API key for the remote server
     private static readonly string LocalUrl = "http://127.0.0.1:5000/audio_to_blendshapes"; // Local URL
     private static readonly string RemoteUrl = "https://api.neurosync.info/audio_to_blendshapes"; // Remote URL

     public static async Task<List<List<float>>> SendAudioToAudio2Face(byte[] audioBytes, bool useLocal=false)
     {
         if (!ValidateAudioBytes(audioBytes))
         {
             Console.WriteLine("Audio bytes are null or empty, skipping send.");
             return null;
         }

         using (var client = new HttpClient())
         {
             try
             {
                 string url = useLocal ? LocalUrl : RemoteUrl;

                 var response = await PostAudioBytesAsync(client, audioBytes, url, useLocal);
                 response.EnsureSuccessStatusCode();

                 var jsonResponse = await response.Content.ReadAsStringAsync();
                 return ParseBlendshapesFromJson(jsonResponse);
             }
             catch (HttpRequestException e)
             {
                 Console.WriteLine($"Request error: {e.Message}");
                 return null;
             }
             catch (System.Text.Json.JsonException e)
             {
                 Console.WriteLine($"JSON parsing error: {e.Message}");
                 return null;
             }
         }
     }

     private static bool ValidateAudioBytes(byte[] audioBytes)
     {
         return audioBytes != null && audioBytes.Length > 0;
     }

     private static async Task<HttpResponseMessage> PostAudioBytesAsync(HttpClient client, byte[] audioBytes, string url, bool useLocal)
     {
         var byteContent = new ByteArrayContent(audioBytes);
         byteContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

         if (!useLocal)
         {
             client.DefaultRequestHeaders.Add("API-Key", ApiKey);
         }

         return await client.PostAsync(url, byteContent);
     }

     private static List<List<float>> ParseBlendshapesFromJson(string jsonResponse)
     {
         var parsedResponse = System.Text.Json.JsonDocument.Parse(jsonResponse);
         var blendshapesJson = parsedResponse.RootElement.GetProperty("blendshapes");

         var facialData = new List<List<float>>();
         foreach (var frame in blendshapesJson.EnumerateArray())
         {
             var frameData = new List<float>();
             foreach (var value in frame.EnumerateArray())
             {
                 frameData.Add(value.GetSingle());
             }
             facialData.Add(frameData);
         }

         return facialData;
     }
 }
