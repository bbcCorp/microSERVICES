using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Wire;
using Confluent.Kafka.Serialization;

namespace app.common.serialization
{
    // Wire based serializer for application POCO
    public class AppWireSerializer<T> : ISerializer<T> , IDeserializer<T>
    {
        private MemoryStream _stream;
        private Serializer _serializer; 

        public AppWireSerializer()
        {
            _serializer = new Serializer(new SerializerOptions(preserveObjectReferences:true));
            _stream = new MemoryStream();
        }     

        protected void CustomInit(Serializer serializer)
        {
            _serializer = serializer;
        }

        public void Reset()
        {
            _stream.Position = 0;
        }        

        public void Serialize(T data)
        {
            _serializer.Serialize(data, _stream);            
        }

        public byte[] GetSerializedData()
        {
            _stream.Position = 0;            
            return _stream.ToArray();        
        }

        public T Deserialize()
        {
            return _serializer.Deserialize<T>(_stream);
        }

        public T Deserialize(byte[] input)
        {
            _stream = new MemoryStream(input);
          
            _stream.Position = 0;
            return _serializer.Deserialize<T>(_stream);
        }


        /// <summary>
        ///     Serialize an instance of type T to a byte array.
        /// </summary>
        /// <param name="topic">
        ///     The topic associated wih the data.
        /// </param>
        /// <param name="data">
        ///     The object to serialize.
        /// </param>
        /// <returns>
        ///     <paramref name="data" /> serialized as a byte array.
        /// </returns>
        public byte[] Serialize(string topic, T data)
        {
            _serializer.Serialize(data, _stream);
            return GetSerializedData();
        }

        /// <summary>
        ///     Deserialize a byte array to an instance of
        ///     type T.
        /// </summary>
        /// <param name="topic">
        ///     The topic associated wih the data.
        /// </param>
        /// <param name="data">
        ///     The serialized representation of an instance
        ///     of type T to deserialize.
        /// </param>
        /// <returns>
        ///     The deserialized value.
        /// </returns>
        public T Deserialize(string topic, byte[] data)
        {
            return Deserialize(data);
        }

        public IEnumerable<KeyValuePair<string, object>> Configure(IEnumerable<KeyValuePair<string, object>> config, bool isKey)
                => config;

        public void Dispose() {}

    }
}
