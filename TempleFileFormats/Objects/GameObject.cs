using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TempleFileFormats.Objects
{
    public class GameObject
    {

        public ObjectType Type { get; set; }

        public ObjectGuid Id { get; set; }

        public ObjectGuid ProtoId { get; set; }

        public IDictionary<ObjectField, object> Properties { get; private set; }

        public GameObject()
        {
            Properties = new Dictionary<ObjectField, object>();
        }

    }

    public class ObjectScript
    {
        public int F1 { get; set; }
        public int F2 { get; set; }
        public int ScriptId { get; set; }

        public override string ToString()
        {
            return String.Format("({0}, {1}, {2})", F1, F2, ScriptId);
        }
    }

}
