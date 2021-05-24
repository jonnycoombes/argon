﻿using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using JCS.Argon.Model.Configuration;
using JCS.Argon.Services.Soap.Opentext;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Serilog;
using static JCS.Neon.Glow.Helpers.General.LogHelpers;

namespace JCS.Argon.Services.Core
{
    /// <summary>
    ///     The default implementation of the <see cref="IArchiveManager" /> interface. The intention
    /// </summary>
    public class ArchiveManager : BaseCoreService, IArchiveManager
    {
        /// <summary>
        ///     The default path separator which is used to split paths
        /// </summary>
        private const string PathSeperator = "/";

        /// <summary>
        ///     The default root node id
        /// </summary>
        private const long RootId = 2000;

        /// <summary>
        ///     The static logger
        /// </summary>
        private static readonly ILogger _log = Log.ForContext<ArchiveManager>();

        /// <summary>
        ///     The current set of options
        /// </summary>
        private readonly ArchiveStorageOptions _options;

        /// <summary>
        ///     The default constructor
        /// </summary>
        /// <param name="options">The current set of <see cref="ApiOptions" /> injected by  the DI container</param>
        /// <param name="serviceProvider">The current DI <see cref="IServiceProvider" /> instance, injected by the DI container</param>
        public ArchiveManager(IOptionsMonitor<ApiOptions> options, IServiceProvider serviceProvider) : base(options, serviceProvider)
        {
            LogMethodCall(_log);
            _options = options.CurrentValue.ArchiveStorageOptions;
        }

        /// <inheritdoc cref="IArchiveManager.DownloadArchivedDocument" />
        public async Task<IArchiveManager.DownloadContentResult> DownloadArchivedDocument(string tag, string path,
            IArchiveManager.ArchiveDownloadType archiveType = IArchiveManager.ArchiveDownloadType.ZipArchive)
        {
            LogMethodCall(_log);
            var client = BindWebServiceClient(tag);
            try
            {
                var elements = path.Split(PathSeperator);
                var node = await client.GetNodeByPath(RootId, elements);
                if (node == null)
                {
                    throw new ArchiveManagerException(StatusCodes.Status404NotFound, "The specified item cannot be found in the archive");
                }

                if (node.IsContainer)
                {
                    throw new ArchiveManagerException(StatusCodes.Status400BadRequest, "The specified item is a container");
                }

                var version = await client.GetItemVersion(node.ID, node.VersionInfo.VersionNum);
                var attachment = await client.GetVersionContents(node.ID, node.VersionInfo.VersionNum);
                return new IArchiveManager.DownloadContentResult
                {
                    Stream = new MemoryStream(attachment.Contents),
                    Filename = version.Filename,
                    MimeType = version.MimeType
                };
            }
            catch (WebServiceClientException ex)
            {
                throw new ArchiveManagerException(StatusCodes.Status500InternalServerError,
                    "An error occurred whilst attempting to retrieve the specified item");
            }
        }

        /// <inheritdoc cref="IArchiveManager.DownloadArchivedMetadata" />
        public async Task<string> DownloadArchivedMetadata(string tag, string path)
        {
            LogMethodCall(_log);
            var client = BindWebServiceClient(tag);

            try
            {
                var elements = path.Split(PathSeperator);
                var node = await client.GetNodeByPath(2000, elements);
                if (node == null)
                {
                    throw new ArchiveManagerException(StatusCodes.Status404NotFound, "The specified item cannot be found in the archive");
                }

                Node[]? children = null;
                if (node.IsContainer)
                {
                    children = await client.GetChildren(node.ID);
                }

                return NodeToJson(node, children, DefaultNodeSerialisationFunction);
            }
            catch (WebServiceClientException ex)
            {
                throw new ArchiveManagerException(StatusCodes.Status500InternalServerError,
                    "An error occurred whilst attempting to retrieve the specified item");
            }
        }

        /// <summary>
        ///     Converts a <see cref="DataValue" /> into a string representation for serialisation to JSON
        /// </summary>
        /// <param name="v">The actual <see cref="DataValue" /> instance.  May be one of several different types</param>
        /// <returns></returns>
        private static string DataValueToString(DataValue v)
        {
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
        private static void DefaultNodeSerialisationFunction(Node source, Utf8JsonWriter writer)
        {
            writer.WriteNumber("id", source.ID);
            writer.WriteNumber("parentId", source.ParentID);
            writer.WriteNumber("volumeId", source.VolumeID);
            writer.WriteString("name", source.Name);
            writer.WriteString("comments", source.Comment);
            writer.WriteString("nickname", source.Nickname);
            writer.WriteString("type", source.Type);
            writer.WriteString("createdDate", source.CreateDate.Value);
            writer.WriteString("modifiedDate", source.ModifyDate.Value);
            writer.WritePropertyName("meta");
            writer.WriteStartObject();
            foreach (var group in source.Metadata.AttributeGroups)
            {
                writer.WritePropertyName(group.DisplayName);
                writer.WriteStartObject();
                foreach (var t in group.Values)
                {
                    writer.WriteString(t.Key, DataValueToString(t));
                }

                writer.WriteEndObject();
            }

            writer.WriteEndObject();
        }

        /// <summary>
        ///     Emits a JSON serialisation of a given node
        /// </summary>
        /// <param name="source">The source <see cref="Node" /></param>
        /// <param name="children">An optional array of children for the node</param>
        /// <param name="nodeSerialisationFunction">A function which knows how to serialise a node to JSON, without the outer object braces</param>
        /// <returns>A string representation of the node and potentially its children</returns>
        private static string NodeToJson(Node source, Node[]? children, Action<Node, Utf8JsonWriter>? nodeSerialisationFunction)
        {
            var options = new JsonWriterOptions
            {
                Indented = false
            };

            nodeSerialisationFunction ??= DefaultNodeSerialisationFunction;

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

        /// <summary>
        ///     Resolves and instantiates a new instance of <see cref="WebServiceClient" /> based on the binding tag supplied
        /// </summary>
        /// <param name="tag">The binding tag used to look up the client settings</param>
        /// <returns>A new instance of <see cref="WebServiceClient" /></returns>
        /// <exception cref="ArchiveManagerException">Thrown in the event of a fault occurring</exception>
        private WebServiceClient BindWebServiceClient(string tag)
        {
            LogMethodCall(_log);
            if (_options == null)
            {
                throw new WebServiceClientException("The system is not currently configured for archiving - please check configuration");
            }

            if (_options.Bindings.Any(b => b.Tag == tag))
            {
                var binding = _options.Bindings.First(b => b.Tag == tag);
                if (!string.IsNullOrEmpty(binding.User) && !string.IsNullOrEmpty(binding.Password))
                {
                    return new WebServiceClient(binding.Endpoint, binding.User, binding.Password);
                }

                return new WebServiceClient(binding.Endpoint);
            }

            throw new ArchiveManagerException(StatusCodes.Status400BadRequest, $"No archive binding  found for tag \"{tag}\"");
        }
    }
}