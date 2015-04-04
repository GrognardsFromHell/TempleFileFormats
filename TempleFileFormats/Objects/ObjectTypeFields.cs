using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TempleFileFormats.Objects
{
    /// <summary>
    /// Defines which fields apply to which types and in which order.
    /// </summary>
    public static class ObjectTypeFields
    {

        private static IEnumerable<ObjectField> EnumerateFields(ObjectField start, ObjectField end)
        {
            for (var i = start + 1; i < end; ++i)
            {
                yield return i;
            }
            yield break;
        }
        
        public static IEnumerable<ObjectField> Get(ObjectType type) {

            // All object types support obj fields
            var result = EnumerateFields(ObjectField.ObjBegin, ObjectField.ObjEnd);

            switch (type)
            {
                case ObjectType.Portal:
                    result = result.Concat(EnumerateFields(ObjectField.PortalBegin, ObjectField.PortalEnd));
                    break;
                case ObjectType.Container:
                    result = result.Concat(EnumerateFields(ObjectField.ContainerBegin, ObjectField.ContainerEnd));
                    break;
                case ObjectType.Scenery:
                    result = result.Concat(EnumerateFields(ObjectField.SceneryBegin, ObjectField.SceneryEnd));
                    break;
                case ObjectType.Projectile:
                    result = result.Concat(EnumerateFields(ObjectField.ProjectileBegin, ObjectField.ProjectileEnd));
                    break;
                case ObjectType.Weapon:
                    result = result.Concat(EnumerateFields(ObjectField.ItemBegin, ObjectField.ItemEnd));
                    result = result.Concat(EnumerateFields(ObjectField.WeaponBegin, ObjectField.WeaponEnd));
                    break;
                case ObjectType.Ammo:
                    result = result.Concat(EnumerateFields(ObjectField.ItemBegin, ObjectField.ItemEnd));
                    result = result.Concat(EnumerateFields(ObjectField.AmmoBegin, ObjectField.AmmoEnd));
                    break;
                case ObjectType.Armor:
                    result = result.Concat(EnumerateFields(ObjectField.ItemBegin, ObjectField.ItemEnd));
                    result = result.Concat(EnumerateFields(ObjectField.ArmorBegin, ObjectField.ArmorEnd));
                    break;
                case ObjectType.Money:
                    result = result.Concat(EnumerateFields(ObjectField.ItemBegin, ObjectField.ItemEnd));
                    result = result.Concat(EnumerateFields(ObjectField.MoneyBegin, ObjectField.MoneyEnd));
                    break;
                case ObjectType.Food:
                    result = result.Concat(EnumerateFields(ObjectField.ItemBegin, ObjectField.ItemEnd));
                    result = result.Concat(EnumerateFields(ObjectField.FoodBegin, ObjectField.FoodEnd));
                    break;
                case ObjectType.Scroll:
                    result = result.Concat(EnumerateFields(ObjectField.ItemBegin, ObjectField.ItemEnd));
                    result = result.Concat(EnumerateFields(ObjectField.ScrollBegin, ObjectField.ScrollEnd));
                    break;
                case ObjectType.Key:
                    result = result.Concat(EnumerateFields(ObjectField.ItemBegin, ObjectField.ItemEnd));
                    result = result.Concat(EnumerateFields(ObjectField.KeyBegin, ObjectField.KeyEnd));
                    break;
                case ObjectType.Written:
                    result = result.Concat(EnumerateFields(ObjectField.ItemBegin, ObjectField.ItemEnd));
                    result = result.Concat(EnumerateFields(ObjectField.WrittenBegin, ObjectField.WrittenEnd));
                    break;
                case ObjectType.Bag:
                    result = result.Concat(EnumerateFields(ObjectField.ItemBegin, ObjectField.ItemEnd));
                    result = result.Concat(EnumerateFields(ObjectField.BagBegin, ObjectField.BagEnd));
                    break;
                case ObjectType.Generic:
                    result = result.Concat(EnumerateFields(ObjectField.ItemBegin, ObjectField.ItemEnd));
                    result = result.Concat(EnumerateFields(ObjectField.GenericBegin, ObjectField.GenericEnd));
                    break;
                case ObjectType.PC:
                    result = result.Concat(EnumerateFields(ObjectField.CritterBegin, ObjectField.CritterEnd));
                    result = result.Concat(EnumerateFields(ObjectField.PcBegin, ObjectField.PcEnd));
                    break;
                case ObjectType.NPC:
                    result = result.Concat(EnumerateFields(ObjectField.CritterBegin, ObjectField.CritterEnd));
                    result = result.Concat(EnumerateFields(ObjectField.NpcBegin, ObjectField.NpcEnd));
                    break;
                case ObjectType.Trap:
                    result = result.Concat(EnumerateFields(ObjectField.TrapBegin, ObjectField.TrapEnd));
                    break;
            }

            return result;

        }

    }
}
