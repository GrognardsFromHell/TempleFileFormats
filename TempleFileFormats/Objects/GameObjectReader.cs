using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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

        private static object ReadFieldValue(ObjectFieldDef fieldDef, BinaryReader reader)
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
                    return ReadFieldValueArray(fieldDef, reader);
                case ObjectFieldType.String:
                    break;
                case ObjectFieldType.Obj:
                    if (reader.ReadByte() == 0)
                    {
                        return null;
                    }
                    else
                    {
                        return reader.ReadObjectGuid();
                    }
            }

            throw new InvalidOperationException("Could not handle field type " + fieldDef.FieldType);
        }

        private static IList ReadFieldValueArray(ObjectFieldDef fieldDef, BinaryReader reader)
        {
            if (reader.ReadByte() == 0)
            {
                return null;
            }

            var elementSize = reader.ReadInt32();
            var elementCount = reader.ReadInt32();
            var headerUnk = reader.ReadInt32();

            var payloadSize = elementCount * elementSize;

            IList result = null;

            if (payloadSize > 0)
            {
                switch (fieldDef.FieldType)
                {
                    case ObjectFieldType.AbilityArray:
                    case ObjectFieldType.Int32Array:
                        Debug.Assert(elementSize == 4);
                        result = ReadArrayValues(reader, elementCount, r => r.ReadInt32());
                        break;
                    case ObjectFieldType.Int64Array:
                        Debug.Assert(elementSize == 8);
                        result = ReadArrayValues(reader, elementCount, r => r.ReadInt64());
                        break;
                    case ObjectFieldType.ScriptArray:
                        Debug.Assert(elementSize == 12);
                        result = ReadArrayValues(reader, elementCount, r => {
                            return new ObjectScript()
                            {
                                F1 = r.ReadInt32(),
                                F2 = r.ReadInt32(),
                                ScriptId = r.ReadInt32()
                            };
                        });
                        break;
                    case ObjectFieldType.ObjArray:
                        Debug.Assert(elementSize == 0x18);
                        result = ReadArrayValues(reader, elementCount, r => r.ReadObjectGuid());
                        break;
                    default:                        
                        throw new InvalidDataException("Unsupported array type: " + fieldDef.FieldType);
                }

                var count = reader.ReadInt32();
                var actualIdx = 0;
                var elementIdx = 0;
                for (var i = 0; i < count; ++i) 
                {
                    uint bitmask = reader.ReadUInt32();
                    for (var j = 0; j < 32; ++j)
                    {
                        uint mask = (uint) 1 << j;
                        if ((bitmask & mask) == mask)
                        {
                            while (elementIdx < actualIdx)
                            {
                                result.Insert(elementIdx, null);
                                elementIdx++;
                            }
                            elementIdx++;
                        }
                        actualIdx++;
                    }
                }

                // Check for sparse arrays
                
            }

            return result;
        }

        private static List<T> ReadArrayValues<T>(BinaryReader reader, int elementCount, Func<BinaryReader, T> readFunc)
        {
            List<T> array = new List<T>();
            for (int i = 0; i < elementCount; ++i)
            {
                array.Add(readFunc.Invoke(reader));
            }
            return array;
        }

    }
}
