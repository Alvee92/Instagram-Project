using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InstaSharp.Model {
    public class Location {
        [JsonMapping("id", JsonMapping.MappingType.Primitive)]
        public int Id { get; set; }
        [JsonMapping("latitude", JsonMapping.MappingType.Primitive)]
        public string Latitude { get; set; }
        [JsonMapping("longitude", JsonMapping.MappingType.Primitive)]
        public string Longitude { get; set; }
        [JsonMapping("name", JsonMapping.MappingType.Primitive)]
        public string Name { get; set; }
    }
}
