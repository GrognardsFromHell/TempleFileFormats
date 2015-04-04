using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TempleFileFormats.Objects
{
    class ObjectFieldBitmap
    {

        public static int GetLengthForType(ObjectType type)
        {

            int result;
            switch (type)
            {
                case ObjectType.Portal:
                    result = ObjectFieldDefs.Get(ObjectField.PortalPadI64as1).BitmapIndex;
                    break;
                case ObjectType.Container:
                    result = ObjectFieldDefs.Get(ObjectField.ContainerPadObjas1).BitmapIndex;
                    break;
                case ObjectType.Scenery:
                    result = ObjectFieldDefs.Get(ObjectField.SceneryPadI64as1).BitmapIndex;
                    break;
                case ObjectType.Projectile:
                    result = ObjectFieldDefs.Get(ObjectField.ProjectilePadObjas1).BitmapIndex;
                    break;
                case ObjectType.Weapon:
                    result = ObjectFieldDefs.Get(ObjectField.WeaponPadI64as1).BitmapIndex;
                    if (result == -1)
                    {
                        result = ObjectFieldDefs.Get(ObjectField.ItemPadObjas2).BitmapIndex;
                    }
                    break;
                case ObjectType.Ammo:
                    result = ObjectFieldDefs.Get(ObjectField.AmmoPadI64as1).BitmapIndex;
                    if (result == -1)
                    {
                        result = ObjectFieldDefs.Get(ObjectField.ItemPadObjas2).BitmapIndex;
                    }
                    break;
                case ObjectType.Armor:
                    result = ObjectFieldDefs.Get(ObjectField.ArmorPadI64as1).BitmapIndex;
                    if (result == -1)
                    {
                        result = ObjectFieldDefs.Get(ObjectField.ItemPadObjas2).BitmapIndex;
                    }
                    break;
                case ObjectType.Money:
                    result = ObjectFieldDefs.Get(ObjectField.MoneyPadI64as1).BitmapIndex;
                    if (result == -1)
                    {
                        result = ObjectFieldDefs.Get(ObjectField.ItemPadObjas2).BitmapIndex;
                    }
                    break;
                case ObjectType.Food:
                    result = ObjectFieldDefs.Get(ObjectField.FoodPadI64as1).BitmapIndex;
                    if (result == -1)
                    {
                        result = ObjectFieldDefs.Get(ObjectField.ItemPadObjas2).BitmapIndex;
                    }
                    break;
                case ObjectType.Scroll:
                    result = ObjectFieldDefs.Get(ObjectField.ScrollPadI64as1).BitmapIndex;
                    if (result == -1)
                    {
                        result = ObjectFieldDefs.Get(ObjectField.ItemPadObjas2).BitmapIndex;
                    }
                    break;
                case ObjectType.Key:
                    result = ObjectFieldDefs.Get(ObjectField.KeyPadI64as1).BitmapIndex;
                    if (result == -1)
                    {
                        result = ObjectFieldDefs.Get(ObjectField.ItemPadObjas2).BitmapIndex;
                    }
                    break;
                case ObjectType.Written:
                    result = ObjectFieldDefs.Get(ObjectField.WrittenPadI64as1).BitmapIndex;
                    if (result == -1)
                    {
                        result = ObjectFieldDefs.Get(ObjectField.ItemPadObjas2).BitmapIndex;
                    }
                    break;
                case ObjectType.Bag:
                    result = ObjectFieldDefs.Get(ObjectField.BagSize).BitmapIndex;
                    if (result == -1)
                    {
                        result = ObjectFieldDefs.Get(ObjectField.ItemPadObjas2).BitmapIndex;
                    }
                    break;
                case ObjectType.Generic:
                    result = ObjectFieldDefs.Get(ObjectField.GenericPadI64as1).BitmapIndex;
                    if (result == -1)
                    {
                        result = ObjectFieldDefs.Get(ObjectField.ItemPadObjas2).BitmapIndex;
                    }
                    break;
                case ObjectType.PC:
                    result = ObjectFieldDefs.Get(ObjectField.PcPadI64as1).BitmapIndex;
                    if (result == -1)
                    {
                        result = ObjectFieldDefs.Get(ObjectField.CritterPadI64as5).BitmapIndex;
                    }
                    break;
                case ObjectType.NPC:
                    result = ObjectFieldDefs.Get(ObjectField.NpcPadI64as5).BitmapIndex;
                    if (result == -1)
                    {
                        result = ObjectFieldDefs.Get(ObjectField.CritterPadI64as5).BitmapIndex;
                    }
                    break;
                case ObjectType.Trap:
                    result = ObjectFieldDefs.Get(ObjectField.TrapPadI64as1).BitmapIndex;
                    break;
                default:
                    result = -1;
                    break;
            }

            if (result == -1)
            {
                result = ObjectFieldDefs.Get(ObjectField.PadObjas2).BitmapIndex;
            }

            result = result + 1; // account for 0-based index

            return result;
        }

    }
}
