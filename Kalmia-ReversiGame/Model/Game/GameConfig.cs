using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kalmia_Game.Model.Game
{
    internal class GameConfig
    {
        public static GameConfig Instance { get; }

        public KalmiaOptions EvaluatorOptions { get; set; } = new();

        static GameConfig()
        {
            try
            {
                var reader = new Utf8JsonReader(File.ReadAllBytes(FilePath.GameConfigFilePath));
                Instance = new GameConfigJsonConverter().Read(ref reader);
            }
            catch (Exception ex) when (ex is FileNotFoundException || ex is JsonException)
            {
                Instance = new GameConfig();
                Save();
            }
        }

        GameConfig() { }

        public static void Save()
        {
            using var fs = File.Create(FilePath.GameConfigFilePath);
            var options = new JsonWriterOptions { Indented = true };
            using var writer = new Utf8JsonWriter(fs, options);
            new GameConfigJsonConverter().Write(writer, Instance, new JsonSerializerOptions());
        }

        class GameConfigJsonConverter : JsonConverter<GameConfig>
        {
            public override GameConfig Read(ref Utf8JsonReader reader, Type? typeToConvert = null, JsonSerializerOptions? options = null)
            {
                var config = new GameConfig();
                var properties = typeof(GameConfig).GetProperties();
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

            public override void Write(Utf8JsonWriter writer, GameConfig value, JsonSerializerOptions options)
            {
                JsonSerializer.Serialize(writer, value, options);
            }
        }
    }
}
