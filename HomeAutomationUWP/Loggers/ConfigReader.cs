using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace HomeAutomationUWP.Loggers
{
    public static class ConfigReader
    {
        public static async Task<T> ReadEntireConfigAsync<T>(ConfigType configType)
        {
            switch (configType)
            {
                case ConfigType.LightConfig:
                    Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                    var fileName = "lights.txt";
                    var file = await storageFolder.TryGetItemAsync(fileName);
                    if (file == null)
                    {
                        return default(T);
                    }
                    var fileForRead = await storageFolder.OpenStreamForReadAsync(fileName);
                    string json = string.Empty;

                    for (int i = 0; i < fileForRead.Length; i++)
                    {
                        json += (char)fileForRead.ReadByte();
                    }

                    return JsonConvert.DeserializeObject<T>(json);
                    break;
                default:
                    return default(T);
            }
        }
    }
}
