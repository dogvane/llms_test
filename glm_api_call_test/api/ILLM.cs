namespace glm_api_call_test.api
{
    public interface ILLM
    {
        /// <summary>
        /// 翻译成中文
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        string Translation(string source);

        /// <summary>
        /// 总结文字
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        string Summary(string source);

        string Name { get; }

        int MaxLength { get; }

        LLMTestTP[] GetTestTP();

        void SetTP(LLMTestTP configItem);
    }

    public class LLMTestTP
    {

        public float top_p { get; set; }

        public float temperature { get; set; }
    }
}