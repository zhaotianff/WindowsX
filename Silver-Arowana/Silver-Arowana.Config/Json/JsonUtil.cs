using Newtonsoft.Json;

namespace Silver_Arowana.Config.Json
{
    public class JsonUtil
    {
        public static T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static T DeserializeFile<T>(string file)
        {
            var json = System.IO.File.ReadAllText(file);
            return Deserialize<T>(json);
        }

        public static void Serialize(object o,string file)
        {
            var json = JsonConvert.SerializeObject(o);
            System.IO.File.WriteAllText(file, json,System.Text.Encoding.UTF8);
        }
    }
}
