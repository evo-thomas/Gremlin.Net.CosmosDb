﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Gremlin.Net.CosmosDb.Structure
{
    /// <summary>
    /// Represents a schema-less vertex/node within a graph
    /// </summary>
    /// <seealso cref="Gremlin.Net.CosmosDb.Structure.Element"/>
    public class Vertex : Element
    {
        /// <summary>
        /// Gets or sets the properties.
        /// </summary>
        [JsonProperty(PropertyNames.Properties)]
        public virtual IDictionary<string, ICollection<VertexPropertyValue>> Properties
        {
            get { return _properties; }
            set { _properties = value ?? new Dictionary<string, ICollection<VertexPropertyValue>>(); }
        }

        private static readonly Type TYPE_OF_IENUMERABLE = typeof(IEnumerable);
        private static readonly Type TYPE_OF_STRING = typeof(string);
        private IDictionary<string, ICollection<VertexPropertyValue>> _properties = new Dictionary<string, ICollection<VertexPropertyValue>>();

        /// <summary>
        /// Gets a value indicating whether the Properties property should be serialized.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual bool ShouldSerializeProperties() => Properties.Any();

        /// <summary>
        /// Converts this vertex to an object of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of object to convert to</typeparam>
        /// <returns>Returns the converted object</returns>
        public T ToObject<T>()
            where T : class
        {
            return ToObject<T>(new JsonSerializerSettings());
        }

        /// <summary>
        /// Converts this vertex to an object of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of object to convert to</typeparam>
        /// <param name="serializerSettings">The serializer settings.</param>
        /// <returns>Returns the converted object</returns>
        public T ToObject<T>(JsonSerializerSettings serializerSettings)
            where T : class
        {
            //convert the properties to a flattened version, capturing the values under each key
            //the destination object type should determine if we get only the first value or all values (as an array)
            //for each property
            var serializer = JsonSerializer.Create(serializerSettings);
            var contract = (JsonObjectContract)serializer.ContractResolver.ResolveContract(typeof(T));
            var flattenedProperties = new Dictionary<string, object>
            {
                [PropertyNames.Id] = Id,
                [PropertyNames.Label] = Label
            };

            foreach (var kvp in Properties)
            {
                var propertyContract = contract.Properties.GetClosestMatchProperty(kvp.Key);

                flattenedProperties[kvp.Key] = propertyContract == null || (TYPE_OF_IENUMERABLE.IsAssignableFrom(propertyContract.PropertyType) && TYPE_OF_STRING != propertyContract.PropertyType)
                    ? kvp.Value.Select(pv => pv.Value).ToList()
                    : kvp.Value.FirstOrDefault()?.Value;
            }

            return JObject.FromObject(flattenedProperties).ToObject<T>(serializer);
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents this instance.</returns>
        public override string ToString()
        {
            return $"v[{Id}]";
        }
    }
}