using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kalmia_Game
{
    internal sealed class GlobalConfig
    {
        public static GlobalConfig Instance { get; }

        public string? WorkDirPath { get; set; } = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public string DefaultFontFamily { get; set; } = "Arial";
        public bool FullScreen { get; set; } = false;
        public int ScreenWidth => (int)(this.ScreenHeight * this.AspectRatio);
        public int ScreenHeight { get; set; } = 720;
        public float AspectRatio { get; set; } = 16.0f / 9.0f;  // AspectRatio = Width / Height
        public float AnimationFPS { get; set; } = 60.0f;
        public float AnimationFrameIntervalMs => 1000.0f / this.AnimationFPS;

        static GlobalConfig()
        {
            try
            {
                var reader = new Utf8JsonReader(File.ReadAllBytes(FilePath.GlobalConfigFilePath));
                Instance = new GlobalConfigJsonConverter().Read(ref reader);
            }
            catch (Exception ex) when (ex is FileNotFoundException || ex is JsonException)
            {
                Instance = new GlobalConfig();
                Save();
            }
        }

        GlobalConfig() { }

        public static void Save()
        {
            using var fs = File.Create(FilePath.GlobalConfigFilePath);
            var options = new JsonWriterOptions { Indented = true };
            using var writer = new Utf8JsonWriter(fs, options);
            new GlobalConfigJsonConverter().Write(writer, Instance, new JsonSerializerOptions());
        }

        class GlobalConfigJsonConverter : JsonConverter<GlobalConfig>
        {
            public override GlobalConfig Read(ref Utf8JsonReader reader, Type? typeToConvert = null, JsonSerializerOptions? options = null)
            {
                var config = new GlobalConfig();
                var properties = typeof(GlobalConfig).GetProperties();
                while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
                {
                    if (reader.TokenType == JsonTokenType.PropertyName)
                    {
                        var propertyName = reader.GetString();
                        var property = properties.Where(x => x.Name == propertyName).FirstOrDefault();
                        reader.Read();
                        if (property is not null && property.CanWrite)
                            property.SetValue(config, JsonSerializer.Deserialize(ref reader, property.PropertyType));
                    }
                }
                return config;
            }

            public override void Write(Utf8JsonWriter writer, GlobalConfig value, JsonSerializerOptions options)
            {
                JsonSerializer.Serialize(writer, value, options);
            }
        }
    }
}
