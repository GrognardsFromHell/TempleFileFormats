using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TempleFileFormats.Utils;

namespace TempleFileFormats.Objects
{
    public class GameObjectReader
    {

        private readonly BinaryReader reader;

        public GameObjectReader(BinaryReader reader)
        {
            this.reader = reader;
        }

        public GameObject Read()
        {
            var version = reader.ReadInt32();
            if (version != 0x77) {
                throw new InvalidDataException("Unknown object file version: " + version);
            }

            var obj = new GameObject();

            obj.ProtoId = reader.ReadObjectGuid();

            if (obj.ProtoId.IsProto)
            {
                throw new InvalidDataException("Proto not supported at this point.");
            }

            obj.Id = reader.ReadObjectGuid();

            obj.Type = (ObjectType) reader.ReadUInt32();
            
            var propCollectionItems = reader.ReadInt16(); // Actually not really used anymore

            var bitmapLength = ObjectFieldBitmap.GetLengthForType(obj.Type);

            var bitmap = new uint[bitmapLength];
            for (var i = 0; i < bitmap.Length; ++i)
            {
                bitmap[i] = reader.ReadUInt32();
            }

            foreach (var field in ObjectTypeFields.Get(obj.Type))
            {
                var fieldDef = ObjectFieldDefs.Get(field);
                // Check if it's set in the bitmap
                if ((bitmap[fieldDef.BitmapIndex] & fieldDef.BitmapMask) != 0)
                {
                    var value = ReadFieldValue(fieldDef, reader);
                    if (value != null)
                    {
                        obj.Properties[field] = value;
                    }
                }
            }

            return obj;
        }

        private object ReadFieldValue(ObjectFieldDef fieldDef, BinaryReader reader)
        {
            switch (fieldDef.FieldType)
            {
                case ObjectFieldType.Int32:
                    return reader.ReadInt32();
                case ObjectFieldType.Int64:
                    if (reader.ReadByte() == 0)
                    {
                        return null;
                    }
                    else
                    {
                        return reader.ReadInt64();
                    }
                case ObjectFieldType.Float32:
                    return reader.ReadSingle();
                case ObjectFieldType.AbilityArray:
                case ObjectFieldType.UnkArray:
                case ObjectFieldType.Int32Array:
                case ObjectFieldType.Int64Array:
                case ObjectFieldType.ScriptArray:
                case ObjectFieldType.Unk2Array:
                case ObjectFieldType.ObjArray:
                case ObjectFieldType.SpellArray:
                    break;
                case ObjectFieldType.String:
                    break;
                case ObjectFieldType.Obj:
                    break;
            }

            throw new InvalidOperationException("Could not handle field type " + fieldDef.FieldType);
        }
    }
}
