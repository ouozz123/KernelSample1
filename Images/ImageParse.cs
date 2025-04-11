using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace KernelSample.Images;
internal class ImageParse
{
    internal async Task RunAsync(string apiKey, string imagePath)
    {
        // 將圖片轉成 base64
        string base64Image = Convert.ToBase64String(File.ReadAllBytes(imagePath));
        string imageUrl = $"data:image/jpeg;base64,{base64Image}";

        // 建立 API 請求內容
        var requestBody = new
        {
            model = "gpt-4o-mini",
            input = new[]
            {
                new {
                    role = "user",
                    content = new object[]
                    {
                        new { type = "input_text", text = "請幫我解析這個圖片要傳達的資訊，允許使用 markdown 中的流程圖或 Table" },
                        new {
                            type = "input_image",
                            image_url = imageUrl
                        }
                    }
                }
            }
        };

        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        var requestJson = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync("https://api.openai.com/v1/responses", content);
        var responseModel = JsonSerializer.Deserialize<Rootobject>((await response.Content.ReadAsStringAsync()));

        Console.WriteLine("Response:\n");
        var lines = responseModel.output[0].content[0].text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        foreach (var line in lines)
            Console.WriteLine(line);
    }


    public class Rootobject
    {
        public string id { get; set; }
        public string _object { get; set; }
        public int created_at { get; set; }
        public string status { get; set; }
        public object error { get; set; }
        public object incomplete_details { get; set; }
        public object instructions { get; set; }
        public object max_output_tokens { get; set; }
        public string model { get; set; }
        public Output[] output { get; set; }
        public bool parallel_tool_calls { get; set; }
        public object previous_response_id { get; set; }
        public Reasoning reasoning { get; set; }
        public bool store { get; set; }
        public float temperature { get; set; }
        public Text text { get; set; }
        public string tool_choice { get; set; }
        public object[] tools { get; set; }
        public float top_p { get; set; }
        public string truncation { get; set; }
        public Usage usage { get; set; }
        public object user { get; set; }
        public Metadata metadata { get; set; }
    }

    public class Reasoning
    {
        public object effort { get; set; }
        public object generate_summary { get; set; }
    }

    public class Text
    {
        public Format format { get; set; }
    }

    public class Format
    {
        public string type { get; set; }
    }

    public class Usage
    {
        public int input_tokens { get; set; }
        public Input_Tokens_Details input_tokens_details { get; set; }
        public int output_tokens { get; set; }
        public Output_Tokens_Details output_tokens_details { get; set; }
        public int total_tokens { get; set; }
    }

    public class Input_Tokens_Details
    {
        public int cached_tokens { get; set; }
    }

    public class Output_Tokens_Details
    {
        public int reasoning_tokens { get; set; }
    }

    public class Metadata
    {
    }

    public class Output
    {
        public string id { get; set; }
        public string type { get; set; }
        public string status { get; set; }
        public Content[] content { get; set; }
        public string role { get; set; }
    }

    public class Content
    {
        public string type { get; set; }
        public object[] annotations { get; set; }
        public string text { get; set; }
    }
}
