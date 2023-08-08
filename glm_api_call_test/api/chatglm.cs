using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace glm_api_call_test.api
{
    public class chatglm : ILLM
    {
        public class config
        {
            public string url { get; set; } = "http://127.0.0.1:8000/v1/";

            public string model { get; set; } = "gpt-3.5-turbo";

            public double temperature { get; set; } = 0.9;
            public double top_p { get; set; } = 0.9;
            public int max_length { get; set; } = 1024 * 32;
        }

        public config Config { get; private set; } = new config();

        public string Name => "chatglm";

        /// <summary>
        /// 将英文翻译成中文
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public string Translation(string source)
        {
            var msg = "翻译成中文，不要带任何提示信息：\r\n " + source;

            var response = ChatCompletion(msg).Result;

            var ret = response.choices.Last().message.content;

            return ret;
        }

        public string Summary(string source)
        {
            var msg = "总结下面的文字：\r\n " + source;

            var response = ChatCompletion(msg).Result;

            var ret = response.choices.Last().message.content;

            return ret;
        }

        public async Task<ChatCompletionResponse> ChatCompletion(string message)
        {
            Console.Write($"message.length:{message.Length} \r\n");

            var msg = new ChatMessage()
            {
                content = message,
                role = "user",
            };

            return await ChatCompletion(new[] { msg });
        }

        public async Task<ChatCompletionResponse> ChatCompletion(ChatMessage[] messages)
        {
            using (var client = new HttpClient())
            {
                var len = messages.Sum(o => o.content.Length) * 2;

                var request = new ChatCompletionRequest()
                {
                    model = Config.model,
                    messages = messages,
                    temperature = Config.temperature,
                    top_p = Config.top_p,
                    max_length = Math.Min(len, Config.max_length),
                    stream = false
                };

                Console.Write($"ChatCompletion:{request.max_length}\r\n");

                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(Config.url + "chat/completions", content);

                var responseString = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ChatCompletionResponse>(responseString);
            }
        }

        public async Task<ModelList> ListModels()
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync("https://example.com/v1/models");
                var responseString = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ModelList>(responseString);
            }
        }


        public class ChatCompletionRequest
        {
            public string model { get; set; }
            public ChatMessage[] messages { get; set; }
            public double temperature { get; set; }
            public double top_p { get; set; }
            public int max_length { get; set; }
            public bool stream { get; set; }
        }

        public class ChatMessage
        {
            public string role { get; set; }
            public string content { get; set; }
        }

        public class ChatCompletionResponse
        {
            public string model { get; set; }
            public ChatCompletionResponseChoice[] choices { get; set; }
            public int created { get; set; }
        }

        public class ChatCompletionResponseChoice
        {
            public int index { get; set; }
            public ChatMessage message { get; set; }
            public string finish_reason { get; set; }
        }


        public class ModelList
        {
            public string @object { get; set; }
            public ModelCard[] data { get; set; }
        }

        public class ModelCard
        {
            public string id { get; set; }
            public string @object { get; set; }
            public int created { get; set; }
            public string owned_by { get; set; }
            public string root { get; set; }
            public string parent { get; set; }
            public object[] permission { get; set; }
        }

        public class HTTPValidationError
        {
            public ValidationError[] detail { get; set; }
        }

        public class ValidationError
        {
            public object[] loc { get; set; }
            public string msg { get; set; }
            public string type { get; set; }
        }

        public class DeltaMessage
        {
            public string role { get; set; }
            public string content { get; set; }
        }
    }
}
