using BlockEntities;
using Pipliz;
using System.Collections.Generic;

namespace Chisel
{
    [BlockEntityAutoLoader]
    public class PlaceShape : IMultiBlockEntityMapping, IChangedWithType
    {
        public IEnumerable<ItemTypes.ItemType> TypesToRegister { get { return types; } }

        readonly ItemTypes.ItemType[] types = ShapeManager.typeWithShapes.ConvertAll(y => ItemTypes.GetType(y)).ToArray();

        public void OnChangedWithType(Chunk chunk, BlockChangeRequestOrigin requestOrigin, Vector3Int blockPosition, ItemTypes.ItemType typeOld, ItemTypes.ItemType typeNew)
        {
            if (requestOrigin.Type != BlockChangeRequestOrigin.EType.Player)
                return;

            Players.Player player = requestOrigin.AsPlayer;

            //OnAdd
            if (typeOld == BlockTypes.BuiltinBlocks.Types.air)
            {
                if (ChiselType.selectedShape[player.ID] == -1)
                    return;

                ServerManager.TryChangeBlock(blockPosition,
                        ItemTypes.GetType(typeNew.Name + ShapeManager.shapeName[ChiselType.selectedShape[player.ID]] + ChiselType.getBestRotation(blockPosition)),
                        requestOrigin);
            }
        }
    }
}
