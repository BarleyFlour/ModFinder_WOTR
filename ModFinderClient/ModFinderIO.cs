﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.IO;

namespace ModFinder_WOTR
{
    /// <summary>
    /// IO utilities for the app
    /// </summary>
    public static class ModFinderIO
    {
        private static JsonSerializer Json
        {
            get
            {
                var json = new JsonSerializer
                {
                    Formatting = Formatting.Indented
                };
                json.Converters.Add(new StringEnumConverter());
                return json;
            }
        }

        /// <summary>
        /// Parse a string as json
        /// </summary>
        /// <typeparam name="T">Type to read as</typeparam>
        /// <param name="value">raw json string</param>
        /// <returns>A T parsed from the json value</returns>
        public static T FromString<T>(string value)
        {
            var jsonReader = new JsonTextReader(new StringReader(value));
            return Json.Deserialize<T>(jsonReader);
        }

        /// <summary>
        /// Parse a file as json
        /// </summary>
        /// <typeparam name="T">Type to read as</typeparam>
        /// <param name="path">full path to a file</param>
        /// <returns>A T parsed from the json contents of the file</returns>
        public static T Read<T>(string path)
        {
            using var reader = File.OpenText(path);
            using var jsonReader = new JsonTextReader(reader);
            return Json.Deserialize<T>(jsonReader);
        }
        /// <summary>
        /// Parse a stream as json
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream">A stream containing json data</param>
        /// <returns>A T parsed from the json contents of the stream</returns>
        public static T Read<T>(Stream stream)
        {
            using var reader = new StreamReader(stream);
            using var jsonReader = new JsonTextReader(reader);
            return Json.Deserialize<T>(jsonReader);
        }

        /// <summary>
        /// Write an object to a file as json
        /// </summary>
        /// <param name="obj">object to convert to json</param>
        /// <param name="path">path to the file to write</param>
        public static void Write(object obj, string path)
        {
            using var writer = File.CreateText(path);
            Json.Serialize(writer, obj);
        }

        /// <summary>
        /// Convert an object to a json string
        /// </summary>
        /// <param name="obj">object to convert to json</param>
        /// <returns>json respresentation of obj</returns>
        public static string Write(object obj)
        {
            var writer = new StringWriter();
            Json.Serialize(writer, obj);
            return writer.ToString();
        }
    }

}
