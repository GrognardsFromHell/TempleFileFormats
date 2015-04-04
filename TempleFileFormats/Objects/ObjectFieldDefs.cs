using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TempleFileFormats.Objects
{
    public enum ObjectFieldType
    {
        None = 0,
        SectionBegin = 1,
        SectionEnd = 2,
        Int32 = 3,
        Int64 = 4,
        AbilityArray = 5,
        UnkArray = 6,
        Int32Array = 7,
        Int64Array = 8,
        ScriptArray = 9,
        Unk2Array = 10,
        String = 11,
        Obj = 12,
        ObjArray = 13,
        SpellArray = 14,
        Float32 = 15
    }

    /// <summary>
    /// Specifies properties of a field.
    /// </summary>
    public class ObjectFieldDef
    {
        public int ProtoPropIdx { get; private set; }
        public int Field4 { get; private set; }
        public int BitmapIndex { get; private set; }
        public uint BitmapMask { get; private set; }
        public int BitmapBit { get; private set; }
        public bool InCollection { get; private set; }
        public ObjectFieldType FieldType { get; private set; }

        public ObjectFieldDef(int protoPropIdx, int field4, int bitmapIdx, uint bitmapMask, int bitmapBit, bool inCollection, ObjectFieldType fieldType)
        {
            this.ProtoPropIdx = protoPropIdx;
            this.Field4 = field4;
            this.BitmapIndex = bitmapIdx;
            this.BitmapMask = bitmapMask;
            this.BitmapBit = bitmapBit;
            this.InCollection = inCollection;
            this.FieldType = fieldType;
        }
    }

    /// <summary>
    /// Specifies properties for all object fields.
    /// </summary>
    public static partial class ObjectFieldDefs
    {

        private static readonly ObjectFieldDef[] fields;

        static ObjectFieldDefs()
        {
            fields = new ObjectFieldDef[430];

            InitializeTable();

            for (int i = 0; i < fields.Length; ++i)
            {
                if (fields[i] == null)
                {
                    throw new InvalidOperationException("Field " + i + " has not been initialized.");
                }
            }
        }

        public static ObjectFieldDef Get(ObjectField field)
        {
            return fields[(int)field];
        }

    }

}
