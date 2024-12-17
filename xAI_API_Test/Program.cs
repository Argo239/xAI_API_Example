using System;
using System.Buffers.Text;
using System.ComponentModel.Design.Serialization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace xAI_API_Test {
    internal class Program {
        static async Task Main(string[] args) {
            string baseUrl = "https://api.x.ai/v1";
            string apiKey = "xai-HamXnEyFRBCanbRCIwwd6asW4jlSNnR81xt6snyXejlvi3ZSqaYPtEjpQFnjeUENLcASEgpvuRr6i4MJ";

            while (true) {
                Console.WriteLine(
                    $"1: API Keys \n" +
                    $"2: Chat completions \n" +
                    $"3: Completions \n" +
                    $"4: Create embeddings \n" +
                    $"5: List embedding models \n" +
                    $"6: Get embedding model \n" +
                    $"7: List language models \n" +
                    $"8: Get language model \n" +
                    $"9: List models \n" +
                    $"10: Get model \n" +
                    $"0: Exit \n" +
                    $"C: Clear \n" +
                    $"Enter the number to switch functions or exit.");

                string enterNum = Console.ReadLine();
                if (string.IsNullOrEmpty(enterNum)) continue;

                if (enterNum.Equals("c", StringComparison.OrdinalIgnoreCase)) {
                    Console.Clear();
                    continue;
                }

                if (int.TryParse(enterNum, out int number)) {
                    if (number == 0) {
                        Console.WriteLine("Thanks for using, see you next time. :)))))");
                        break;
                    }
                    await SwitchFunction(number, baseUrl, apiKey);
                } else {
                    Console.WriteLine("Invalid input. Please enter a valid number.");
                }
            }
        }

        /// <summary>
        /// Get information about an API key, including name, status, permissions and users who created or modified this key.
        /// </summary>
        /// <param name="baseUrl"></param>
        /// <param name="apiKey"></param>
        /// <returns></returns>
        public static async Task APIKey(string baseUrl, string apiKey) {
            string url = baseUrl + "/api-key";
            await Get(url, apiKey);
        }

        /// <summary>
        /// Create a language model response for a given chat conversation. This endpoint is compatible with the OpenAI API.
        /// </summary>
        /// <param name="baseUrl"></param>
        /// <param name="apiKey"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static async Task ChatCompletions(string baseUrl, string apiKey, string message) {
            string url = baseUrl + "/chat/completions";
            var requestBody = new {
                messages = new[] {
                    new { role = "system", content = "You are Grok, a chatbot inspired by the Hitchhiker's Guide to the Galaxy." },
                    new { role = "user", content = message }
                },
                model = "grok-beta",
                stream = false,
                temperature = 0
            };

            string jsonContent = JsonSerializer.Serialize(requestBody);

            await Post(url, apiKey, jsonContent);
        }

        public static async Task Completions(string baseUrl, string apiKey, string prompt) {
            string url = baseUrl + "/completions";
            var requestBody = new {
                prompt = prompt,
                model = "grok-beta",
                max_token = "500"
            };

            string jsonContent = JsonSerializer.Serialize(requestBody);

            await Post(url, apiKey, jsonContent);
        }


        public static async Task CreateEmbeddings(string baseUrl, string apiKey, string input) {
            string url = baseUrl + "/embeddings";
            var requestBody = new {
                input = new[] {
                    input,
                },
                model = "grok-beta",
                encoding_format = "base64"
            };

            string jsonContent = JsonSerializer.Serialize(requestBody);

            await Post(url, apiKey, jsonContent);
        }

        /// <summary>
        /// List all embedding models available for an API key.
        /// </summary>
        /// <param name="baseUrl"></param>
        /// <param name="apiKey"></param>
        /// <returns></returns>
        public static async Task ListEmbeddingModels(string baseUrl, string apiKey) {
            string url = baseUrl + "/embedding-models";
            await Get(url, apiKey);
        }

        public static async Task GetEmbeddingModel(string baseUrl, string apiKey, string model_id) {
            string url = baseUrl + $"/embedding-models/{model_id}";
            await Get(url, apiKey);
        }

        public static async Task ListLanguageModels(string baseUrl, string apiKey) {
            string url = baseUrl + "/language-models";
            await Get(url, apiKey);
        }

        public static async Task GetLanguageModel(string baseUrl, string apiKey, string model_id) {
            string url = baseUrl + $"/language-models/{model_id}";
            await Get(url, apiKey);
        }

        public static async Task ListModels(string baseUrl, string apiKey) {
            string url = baseUrl + "/models";
            await Get(url, apiKey);
        }

        public static async Task GetModel(string baseUrl, string apiKey, string model_id) {
            string url = baseUrl + $"/models/{model_id}";
            await Get(url, apiKey);
        }

        public static async Task Get(string url, string apiKey) {
            Console.WriteLine("Getting response now, please wait a few seconds.");

            CancellationTokenSource cts = new CancellationTokenSource();
            Task loadingTask = LoadingAnimation(cts.Token);

            using (HttpClient client = new HttpClient()) {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                try {
                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    string content = await response.Content.ReadAsStringAsync();

                    cts.Cancel();
                    await loadingTask;

                    Console.Clear();
                    Console.WriteLine("Response:\n" + FormatJson(content));
                } catch (HttpRequestException e) {
                    cts.Cancel();
                    await loadingTask;

                    Console.Clear();
                    Console.WriteLine("Request failed:\n" + e.Message);
                }
            }
        }

        public static async Task Post(string url, string apiKey, string jsonContent) {
            Console.WriteLine($"Posting response now, please wait a few seconds.");

            CancellationTokenSource cts = new CancellationTokenSource();
            Task loadingTask = LoadingAnimation(cts.Token);    

            using (HttpClient client = new HttpClient()) {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue($"Bearer", apiKey);
                StringContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                try {
                    HttpResponseMessage response = await client.PostAsync(url, content);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();



                    cts.Cancel();
                    await loadingTask;

                    Console.Clear();
                    Console.WriteLine("Response:\n" + FormatJson(responseBody));
                } catch (HttpRequestException e) {
                    cts.Cancel();
                    await loadingTask;

                    Console.Clear();
                    Console.WriteLine("Request failed: " + e.Message);
                }
            }
        }

        public static async Task SwitchFunction(int number, string baseUrl, string apiKey) {
            Console.Clear();
            switch (number) {
                case 1: await APIKey(baseUrl, apiKey); break;
                case 2:
                    Console.WriteLine("What can I help you with today?");
                    await ChatCompletions(baseUrl, apiKey, Console.ReadLine());
                    break;
                case 3:
                    Console.WriteLine("What can I help you with today?");
                    await Completions(baseUrl, apiKey, Console.ReadLine());
                    break;
                case 4:
                    Console.WriteLine("Input somethihg to create the embeddings");
                    await CreateEmbeddings(baseUrl, apiKey, Console.ReadLine());
                    break;
                case 5: await ListEmbeddingModels(baseUrl, apiKey); break;
                case 6:
                    Console.WriteLine("Please enter the id of the embedding model you want:");
                    await GetEmbeddingModel(baseUrl, apiKey, Console.ReadLine());
                    break;
                case 7: await ListLanguageModels(baseUrl, apiKey); break;
                case 8:
                    Console.WriteLine("Please enter the id of the language model you want:");
                    await GetLanguageModel(baseUrl, apiKey, Console.ReadLine());
                    break;
                case 9: await ListModels(baseUrl, apiKey); break;
                case 10:
                    Console.WriteLine("Please enter the id of the model you want:");
                    await GetModel(baseUrl, apiKey, Console.ReadLine());
                    break;
                default:
                    Console.WriteLine("Invalid function.");
                    break;
            }

            Console.WriteLine("\nPress 'R' to return to the main menu.");
            while (true) {
                ConsoleKeyInfo keyInfo = Console.ReadKey();
                if (keyInfo.Key == ConsoleKey.R) {
                    Console.Clear();
                    break;
                }
            }
        }

        private static async Task LoadingAnimation(CancellationToken token) {
            string[] animationFrames = { "|", "/", "-", "\\" };
            int currentFrame = 0;

            while (!token.IsCancellationRequested) {
                Console.Write("\r" + animationFrames[currentFrame]);
                currentFrame = (currentFrame + 1) % animationFrames.Length;
                await Task.Delay(200);
            }

            Console.Write("\r    \r"); // 清除动画
        }

        /// <summary>
        /// 格式化 JSON 字符串，使其更具可读性
        /// </summary>
        /// <param name="jsonString">原始 JSON 字符串</param>
        /// <returns>格式化后的 JSON 字符串</returns>
        private static string FormatJson(string jsonString) {
            try {
                using (JsonDocument doc = JsonDocument.Parse(jsonString)) {
                    JsonSerializerOptions options = new JsonSerializerOptions {
                        WriteIndented = true // 启用缩进
                    };
                    return JsonSerializer.Serialize(doc.RootElement, options);
                }
            } catch (JsonException e) {
                Console.WriteLine("Invalid JSON format: " + e.Message);
                return jsonString; // 如果格式化失败，直接返回原始内容
            }
        }
    }
}
