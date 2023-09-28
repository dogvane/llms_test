using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace glm_api_call_test.api
{
    /// <summary>
    /// 360公司老总出钱做的
    /// </summary>
    public class baichuan : ILLM
    {
        public class config
        {
            public string url { get; set; } = "http://127.0.0.1:8001/";

            public double temperature { get; set; } = 0.9;
            public double top_p { get; set; } = 0.9;
            public int max_length { get; set; } = 1000 * 4;
        }

        public int MaxLength => Config.max_length;


        public config Config { get; private set; } = new config();

        public string Name => "baichuan2";

        /// <summary>
        /// 将英文翻译成中文
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public string Translation(string source)
        {
            var msg = "翻译成中文：\r\n " + source;

            var response = ChatCompletion(msg, null);

            var ret = response.response;

            return ret;
        }

        public string Summary(string source)
        {
            var msg = "总结下文：\r\n " + source;

            var response = ChatCompletion(msg, null);

            var ret = response.response;

            return ret;
        }

        public ChatCompletionResponse ChatCompletion(string messages, string history = null)
        {
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromMinutes(10);

                var request = new
                {
                    prompt = messages,
                    history,
                    top_p = Config.top_p,
                    max_length = Config.max_length,
                    temperature = Config.temperature,
                    stream = false
                };
                Console.WriteLine($"request.length: {messages.Length}");
                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = client.PostAsync(Config.url + "", content);

                var responseString = response.Result.Content.ReadAsStringAsync().Result;

                //Console.WriteLine(responseString);
                return JsonConvert.DeserializeObject<ChatCompletionResponse>(responseString);
            }
        }

        public LLMTestTP[] GetTestTP()
        {
            return new[] {
                new LLMTestTP(){ top_p = 0.85f, temperature = 0.3f },
                new LLMTestTP(){ top_p = 0.85f, temperature = 0.4f },
                new LLMTestTP(){ top_p = 0.85f, temperature = 0.5f },
                new LLMTestTP(){ top_p = 0.85f, temperature = 0.6f },
                new LLMTestTP(){ top_p = 0.85f, temperature = 0.7f },

                new LLMTestTP(){ top_p = 0.95f, temperature = 0.3f },
                new LLMTestTP(){ top_p = 0.85f, temperature = 0.3f },
                new LLMTestTP(){ top_p = 0.75f, temperature = 0.3f },
                new LLMTestTP(){ top_p = 0.65f, temperature = 0.3f },
                new LLMTestTP(){ top_p = 0.55f, temperature = 0.3f },
            };
        }
        
        public void SetTP(LLMTestTP configItem)
        {
            Config.top_p = configItem.top_p;
            Config.temperature = configItem.temperature;
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
            public string response;

            //public string[][] history;

            public string time;

            public override string ToString()
            {
                return response;
            }
        }


    }
}
