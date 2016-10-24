﻿using System;
using Wki.EventSourcing.Messages;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Wki.EventSourcing.Serialisation
{
    public static class EventSerializer
    {
        // holds the constructor-generated serializer settings
        private static readonly JsonSerializerSettings JsonSettings;

        static EventSerializer()
        {
            JsonSettings = new JsonSerializerSettings
            {
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                Formatting = Formatting.None,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Converters = new[] { new StringEnumConverter() },
                TypeNameHandling = TypeNameHandling.All
            };
        }

        /// <summary>
        /// convert an event to a properly formatted JSON
        /// </summary>
        /// <returns>The json.</returns>
        /// <param name="event">the event to serialize</param>
        public static string ToJson(Event @event)
        {
            return JsonConvert.SerializeObject(@event, JsonSettings);
        }

        /// <summary>
        /// deserialize a json string into an event
        /// </summary>
        /// <returns>Event</returns>
        /// <param name="json">Json string.</param>
        public static Event FromJson(string json)
        {
            return (Event)JsonConvert.DeserializeObject(json, JsonSettings);
        }
    }
}
