using Newtonsoft.Json;

namespace RBXNo60.ClientPresets
{
    public class ClientAppSettingsBase
    {
        [JsonIgnore]
        public bool ENABLED { get; set; }
    }
}
