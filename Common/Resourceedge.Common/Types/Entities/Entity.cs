using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using Resourceedge.Common.Util;
using Resourceedge.Common.Types.Entities.Interfaces;

namespace Resourceedge.Common.Types.Entities
{
    public class Entity : IEntityBase<string>
    {
        public Entity()
        {
            //InitializeInternalState();
        }
        public Entity(string id, string userId)
        {
            Id = id;
            CreatedBy = userId;
            UpdatedBy = userId;
            InitializeInternalState();
        }

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; protected set; }
        public double CreatedAt { get; protected set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string CreatedBy { get; protected set; }
        public double UpdatedAt { get; protected set; }
        public string UpdatedBy { get; protected set; }

        private void InitializeInternalState()
        {
            //Id = ObjectId.GenerateNewId(DateTime.UtcNow).ToString();
            CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            UpdatedAt = CreatedAt;
        }
    }
}
