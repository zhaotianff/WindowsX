using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace WindowsX.Web.Util
{
    public class JsonHelper
    {
        public static T Deserialize<T>(string json)
        {
            T t = default(T);
            try
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Converters.Add(new JavaScriptDateTimeConverter());
                serializer.NullValueHandling = NullValueHandling.Ignore;
                t = JsonConvert.DeserializeObject<T>(json);
            }
            catch
            {
                //TODO 日志
            }
            return t;
        }
    }
}
