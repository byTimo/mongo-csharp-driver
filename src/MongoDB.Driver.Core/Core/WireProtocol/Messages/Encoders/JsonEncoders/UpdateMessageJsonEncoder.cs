﻿/* Copyright 2013-2014 MongoDB Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Core.WireProtocol.Messages.Encoders.JsonEncoders
{
    public class UpdateMessageJsonEncoder : MessageJsonEncoderBase, IMessageEncoder<UpdateMessage>
    {

        // constructors
        public UpdateMessageJsonEncoder(TextReader textReader, TextWriter textWriter, MessageEncoderSettings encoderSettings)
            : base(textReader, textWriter, encoderSettings)
        {
        }

        // methods
        public UpdateMessage ReadMessage()
        {
            var jsonReader = CreateJsonReader();
            var messageContext = BsonDeserializationContext.CreateRoot<BsonDocument>(jsonReader);
            var messageDocument = BsonDocumentSerializer.Instance.Deserialize(messageContext);

            var opcode = messageDocument["opcode"].AsString;
            if (opcode != "update")
            {
                throw new FormatException("Opcode is not update.");
            }

            var requestId = messageDocument["requestId"].ToInt32();
            var databaseName = messageDocument["database"].AsString;
            var collectionName = messageDocument["collection"].AsString;
            var query = messageDocument["query"].AsBsonDocument;
            var update = messageDocument["update"].AsBsonDocument;
            var isMulti = messageDocument.GetValue("isMulti", false).ToBoolean();
            var isUpsert = messageDocument.GetValue("isUpsert", false).ToBoolean();

            return new UpdateMessage(
                requestId,
                databaseName,
                collectionName,
                query,
                update,
                isMulti,
                isUpsert);
        }

        public void WriteMessage(UpdateMessage message)
        {
            Ensure.IsNotNull(message, "message");

            var messageDocument = new BsonDocument
            {
                { "opcode", "update" },
                { "requestId", message.RequestId },
                { "database", message.DatabaseName },
                { "collection", message.CollectionName },
                { "isMulti", true, message.IsMulti },
                { "isUpsert", true, message.IsUpsert },
                { "query", message.Query },
                { "update", message.Update }
            };

            var jsonWriter = CreateJsonWriter();
            var messageContext = BsonSerializationContext.CreateRoot<BsonDocument>(jsonWriter);
            BsonDocumentSerializer.Instance.Serialize(messageContext, messageDocument);
        }

        // explicit interface implementations
        MongoDBMessage IMessageEncoder.ReadMessage()
        {
            return ReadMessage();
        }

        void IMessageEncoder.WriteMessage(MongoDBMessage message)
        {
            WriteMessage((UpdateMessage)message);
        }
    }
}
