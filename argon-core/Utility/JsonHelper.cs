using System;
using System.IO;
using System.Text;
using System.Text.Json;
using JCS.Argon.Services.Soap.Opentext;
using Serilog;
using static JCS.Neon.Glow.Helpers.General.LogHelpers;

namespace JCS.Argon.Utility
{
    /// <summary>
    ///     Useful Json related utilities go in here, along with any custom
    ///     converters/serialisers
    /// </summary>
    public static class JsonHelper
    {
        /// <summary>
        ///     Static logger for this class
        /// </summary>
        private static readonly ILogger _log = Log.ForContext(typeof(JsonHelper));

        /// <summary>
        ///     Converts a <see cref="DataValue" /> into a string representation for serialisation to JSON
        /// </summary>
        /// <param name="v">The actual <see cref="DataValue" /> instance.  May be one of several different types</param>
        /// <returns></returns>
        private static string OpenTextDataValueToString(DataValue v)
        {
            LogMethodCall(_log);
            switch (v)
            {
                case IntegerValue iv:
                    return $"{iv.Values[0]}";
                case StringValue sv:
                    return $"{sv.Values[0]}";
                case DateValue dv:
                    if (dv.Values[0] != null)
                    {
                        return $"{dv.Values[0]}";
                    }
                    else
                    {
                        return "";
                    }
                default:
                    return v.ToString() ?? string.Empty;
            }
        }

        /// <summary>
        ///     The default function for the serialisation of <see cref="Node" /> instances to Json
        /// </summary>
        /// <param name="source">The source <see cref="Node" /></param>
        /// <param name="writer">The <see cref="Utf8JsonWriter" /> being used to serialise the Json</param>
        public static void OpenTextDefaultNodeSerialisationFunction(Node source, Utf8JsonWriter writer)
        {
            LogMethodCall(_log);
            writer.WriteNumber("id", source.ID);
            writer.WriteNumber("parentId", source.ParentID);
            writer.WriteNumber("volumeId", source.VolumeID);
            writer.WriteString("name", source.Name);
            writer.WriteString("comments", source.Comment);
            writer.WriteString("nickname", source.Nickname);
            writer.WriteString("type", source.Type);
            writer.WriteString("createdDate", source.CreateDate.Value);
            writer.WriteString("modifiedDate", source.ModifyDate.Value);
            if (source.Metadata.AttributeGroups != null)
            {
                writer.WritePropertyName("meta");
                writer.WriteStartObject();
                foreach (var group in source.Metadata.AttributeGroups)
                {
                    writer.WritePropertyName(group.DisplayName);
                    writer.WriteStartObject();
                    foreach (var t in group.Values)
                    {
                        writer.WriteString(t.Key, OpenTextDataValueToString(t));
                    }

                    writer.WriteEndObject();
                }

                writer.WriteEndObject();
            }
        }

        /// <summary>
        ///     Emits a JSON serialisation of a given OTCS <see cref="Node" />
        /// </summary>
        /// <param name="source">The source <see cref="Node" /></param>
        /// <param name="children">An optional array of children for the node</param>
        /// <param name="nodeSerialisationFunction">A function which knows how to serialise a node to JSON, without the outer object braces</param>
        /// <returns>A string representation of the node and potentially its children</returns>
        public static string OpenTextNodeToJson(Node source, Node[]? children, Action<Node, Utf8JsonWriter>? nodeSerialisationFunction)
        {
            LogMethodCall(_log);
            var options = new JsonWriterOptions
            {
                Indented = false
            };

            nodeSerialisationFunction ??= OpenTextDefaultNodeSerialisationFunction;

            using var stream = new MemoryStream();
            using (var writer = new Utf8JsonWriter(stream, options))
            {
                writer.WriteStartObject();
                nodeSerialisationFunction(source, writer);
                if (children != null)
                {
                    writer.WritePropertyName("children");
                    writer.WriteStartArray();
                    foreach (var child in children)
                    {
                        writer.WriteStartObject();
                        nodeSerialisationFunction(child, writer);
                        writer.WriteEndObject();
                    }

                    writer.WriteEndArray();
                }

                writer.WriteEndObject();
            }

            return Encoding.UTF8.GetString(stream.ToArray());
        }
    }
}