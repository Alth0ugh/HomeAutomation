using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeAutomationUWP.Loggers
{
    public static class ConfigLogger
    {
        public async static Task Log(ConfigType configType, object data)
        {
            switch(configType)
            {
                case ConfigType.LightConfig:
                        Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                        Windows.Storage.StorageFile file = await storageFolder.CreateFileAsync("lights.txt", Windows.Storage.CreationCollisionOption.ReplaceExisting);

                        var jsonString = JsonConvert.SerializeObject(data);
                        var convertedData = Encoding.ASCII.GetBytes(jsonString);
                        var openedFile = await file.OpenStreamForWriteAsync();
                        openedFile.Flush();
                        openedFile.Write(convertedData, 0, convertedData.Length);
                        openedFile.Close();
                        openedFile.Dispose();
                    break;
                default:
                    break;
            }
        }
    }

    public enum ConfigType
    {
        LightConfig = 1,
        PoolEntriesConfig = 2
    }
}
