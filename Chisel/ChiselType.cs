using System.Collections.Generic;

using Pipliz;
using ModLoaderInterfaces;

using Shared;
using NetworkUI;
using NetworkUI.Items;

using static Shared.PlayerClickedData;
using System.Linq;

namespace Chisel
{
    [ModLoader.ModManager]
    public class ChiselType : IOnPlayerClicked, IOnPlayerPushedNetworkUIButton, IAfterItemTypesDefined, IOnTryChangeBlock
    {
        public static Dictionary<Players.PlayerID, int> selectedShape = new Dictionary<Players.PlayerID, int>();

        public void OnPlayerClicked(Players.Player player, PlayerClickedData click)
        {
            if (!ItemTypes.IndexLookup.TryGetName(click.TypeSelected, out string name) || !name.Equals("Khanx.Chisel"))
                return;

            if (click.ClickType == EClickType.Left)
            {
                //Check if hits a type
                if (click.HitType != EHitType.Block || click.GetVoxelHit().TypeHit == BlockTypes.BuiltinBlocks.Indices.air)
                    return;

                //Check if the player has selected a shape
                if (!selectedShape.TryGetValue(player.ID, out int shape))
                {
                    Chatting.Chat.Send(player, "Use right click to select the shape.");

                    return;
                }

                string typeHitName = ItemTypes.IndexLookup.GetName(click.GetVoxelHit().TypeHit);

                //Check if Type has shape
                if (ShapeManager.typeWithShapes.Contains(click.GetVoxelHit().TypeHit))
                {
                    if (selectedShape[player.ID] == -1)
                        return;

                    //Replace for selected shape
                    ServerManager.TryChangeBlock(click.GetVoxelHit().BlockHit,
                        ItemTypes.GetType(typeHitName + ShapeManager.shapeName[selectedShape[player.ID]] + getBestRotation(click.GetVoxelHit().BlockHit)),
                        player);

                    return;
                }

                //substring can throw exception if do not contain . AND .
                if (!typeHitName.Contains("."))
                {
                    Chatting.Chat.Send(player, "The chisel is not strong enough to carve this block.");

                    return;
                }

                string typeHitBase = typeHitName.Substring(0, typeHitName.LastIndexOf("."));

                string typeHitShape = typeHitName.Substring(typeHitName.LastIndexOf("."));
                typeHitShape = typeHitShape.Substring(0, typeHitShape.Length - 2);

                string typeHitRotation = typeHitName.Substring(typeHitName.Length - 2);


                //Not hitting shape
                if (!ShapeManager.shapeName.Contains(typeHitShape))
                {
                    //Message Repited
                    Chatting.Chat.Send(player, "The chisel is not strong enough to carve this block.");

                    return;
                }

                if (selectedShape[player.ID] == -1) // Cube shape
                {
                    ServerManager.TryChangeBlock(click.GetVoxelHit().BlockHit,
                                        ItemTypes.GetType(typeHitBase),
                                        player);

                    return;
                }

                //Rotate if is the same shape
                if (ShapeManager.shapeName[selectedShape[player.ID]] == typeHitShape)
                {
                    string newRotation = "x+";

                    switch (typeHitRotation)
                    {
                        case "x+": newRotation = "z+"; break;
                        case "z+": newRotation = "x-"; break;
                        case "x-": newRotation = "z-"; break;
                        case "z-": newRotation = "x+"; break;
                    }

                    ServerManager.TryChangeBlock(click.GetVoxelHit().BlockHit,
                        ItemTypes.GetType(typeHitBase + typeHitShape + newRotation),
                        player);
                }
                else    //Change shape
                {
                    ServerManager.TryChangeBlock(click.GetVoxelHit().BlockHit,
                        ItemTypes.GetType(typeHitBase + ShapeManager.shapeName[selectedShape[player.ID]] + typeHitRotation),
                        player);
                }
            }
            else if (click.ClickType == EClickType.Right)
            {
                NetworkMenu chiselMenu = new NetworkMenu();

                int shapesPerRow = 4;

                int width = 175;


                chiselMenu.Identifier = "Chisel";
                chiselMenu.LocalStorage.SetAs("header", "Chisel");
                chiselMenu.Width = width * shapesPerRow + 50;
                chiselMenu.Height = Math.RoundToInt((float)ShapeManager.shapeName.Count / shapesPerRow) * (64 + 30 + 30 + 5);

                chiselMenu.Items.Add(new HorizontalRow(new List<(IItem, int)>
                    {
                        (new EmptySpace(), (int) (width * shapesPerRow * 0.45)),
                        (new ItemIcon(ShapeManager.baseName + ".Cube"){ ShowTooltip = false}, (int) ( width * shapesPerRow * 0.7)),
                        (new EmptySpace(), (int) (width * shapesPerRow * 0.45))
                    }));

                chiselMenu.Items.Add(new HorizontalRow(new List<(IItem, int)>
                    {
                        (new EmptySpace(), (int) (width * shapesPerRow * 0.45) - 30),
                        (new ButtonCallback("Khanx.Chisel.-1", new LabelData("Cube"), -1, 30, ButtonCallback.EOnClickActions.ClosePopup), (int) (width * 0.7)),
                        (new EmptySpace(), (int) (width * shapesPerRow * 0.45))
                    }));

                chiselMenu.Items.Add(new EmptySpace(30));

                for (int i = 0; i < ShapeManager.shapeName.Count / shapesPerRow; i++)
                {
                    chiselMenu.Items.Add(new HorizontalRow(new List<(IItem, int)>
                    {
                        (new EmptySpace(), (int) (width * 0.3)),
                        (new ItemIcon(ShapeManager.baseName + ShapeManager.shapeName[i * shapesPerRow]){ ShowTooltip = false}, (int) (width * 0.7)),
                        (new EmptySpace(), (int) (width * 0.3)),
                        (new ItemIcon(ShapeManager.baseName + ShapeManager.shapeName[i * shapesPerRow + 1]){ ShowTooltip = false}, (int) (width * 0.7)),
                        (new EmptySpace(), (int) (width * 0.3)),
                        (new ItemIcon(ShapeManager.baseName + ShapeManager.shapeName[i * shapesPerRow + 2]){ ShowTooltip = false}, (int) (width * 0.7)),
                        (new EmptySpace(), (int) (width * 0.3)),
                        (new ItemIcon(ShapeManager.baseName + ShapeManager.shapeName[i * shapesPerRow + 3]){ ShowTooltip = false}, (int) (width * 0.7))
                    }));

                    chiselMenu.Items.Add(new HorizontalRow(new List<(IItem, int)>
                    {
                        (new ButtonCallback("Khanx.Chisel." + (i * shapesPerRow), new LabelData(ShapeManager.shapeName[i * shapesPerRow].Substring(1)), -1, 30, ButtonCallback.EOnClickActions.ClosePopup), width),
                        (new ButtonCallback("Khanx.Chisel." + (i * shapesPerRow + 1), new LabelData(ShapeManager.shapeName[i * shapesPerRow + 1].Substring(1)), -1, 30, ButtonCallback.EOnClickActions.ClosePopup) , width),
                        (new ButtonCallback("Khanx.Chisel." + (i * shapesPerRow + 2), new LabelData(ShapeManager.shapeName[i * shapesPerRow + 2].Substring(1)), -1, 30, ButtonCallback.EOnClickActions.ClosePopup), width),
                        (new ButtonCallback("Khanx.Chisel." + (i * shapesPerRow + 3), new LabelData(ShapeManager.shapeName[i * shapesPerRow + 3].Substring(1)), -1, 30, ButtonCallback.EOnClickActions.ClosePopup), width)
                    }));

                    chiselMenu.Items.Add(new EmptySpace(30));
                }

                NetworkMenuManager.SendServerPopup(player, chiselMenu);
            }
        }

        public void OnPlayerPushedNetworkUIButton(ButtonPressCallbackData data)
        {
            if (data.ButtonIdentifier.Contains("Khanx.Chisel"))
            {
                if (selectedShape.ContainsKey(data.Player.ID))
                    selectedShape.Remove(data.Player.ID);

                int shape = int.Parse(data.ButtonIdentifier.Substring(data.ButtonIdentifier.LastIndexOf(".") + 1));

                selectedShape.Add(data.Player.ID, shape);

                if(shape == -1)
                {
                    colonyserver.Assets.UIGeneration.UIManager.RemoveUIImage("Khanx.Shape", data.Player);
                }
                else
                {
                    colonyserver.Assets.UIGeneration.UIManager.AddorUpdateUIImage("Khanx.Shape", colonyshared.NetworkUI.UIGeneration.UIElementDisplayType.Colony,
                    ShapeManager.baseName + ShapeManager.shapeName[shape], new Vector3Int(100, 100, 0), colonyshared.NetworkUI.AnchorPresets.BottomLeft, data.Player);
                }
            }
        }

        public static string getBestRotation(Vector3Int voxelHit)
        {
            bool forward = World.TryGetTypeAt(voxelHit + Vector3Int.forward, out ushort type) && type != BlockTypes.BuiltinBlocks.Indices.air;
            bool back = World.TryGetTypeAt(voxelHit + Vector3Int.back, out type) && type != BlockTypes.BuiltinBlocks.Indices.air;
            bool left = World.TryGetTypeAt(voxelHit + Vector3Int.left, out type) && type != BlockTypes.BuiltinBlocks.Indices.air;
            bool right = World.TryGetTypeAt(voxelHit + Vector3Int.right, out type) && type != BlockTypes.BuiltinBlocks.Indices.air;


            //Three adjacent blocks
            if (forward && left && right)
                return "z+";

            if (back && left && right)
                return "z-";

            if (forward && left && back)
                return "x-";

            if (forward && right && back)
                return "x+";

            //Two adjacent blocks
            if (forward && left)
                return "z+";

            if (forward && right)
                return "x+";

            if (back && left)
                return "x-";

            if (back && right)
                return "z-";

            //One adjacent block
            if (forward)
                return "z+";

            if (back)
                return "z-";

            if (left)
                return "x-";

            if (right)
                return "x+";

            return "x+";
        }

        HashSet<ItemTypes.ItemType> types;

        public void AfterItemTypesDefined()
        {
            types = ShapeManager.typeWithShapes.ConvertAll(y => ItemTypes.GetType(y)).ToHashSet();
        }

        public void OnTryChangeBlock(ModLoader.OnTryChangeBlockData data)
        {
            if (data.CallbackConsumedResult != EServerChangeBlockResult.None) { return; }
            if (data.CallbackOrigin != ModLoader.OnTryChangeBlockData.ECallbackOrigin.ClientPlayerManual) { return; }
            if (data.RequestOrigin.Type != BlockChangeRequestOrigin.EType.Player) { return; }
            if (data.TypeOld != BlockTypes.BuiltinBlocks.Types.air) { return; }
            if (!types.Contains(data.TypeNew)) { return; }

            Players.Player player = data.RequestOrigin.AsPlayer;
            
            if (!selectedShape.ContainsKey(player.ID) || selectedShape[player.ID] == -1) { return; }
            
            ItemTypes.ItemType nextNewType = ItemTypes.GetType(data.TypeNew.Name + ShapeManager.shapeName[ChiselType.selectedShape[player.ID]] + ChiselType.getBestRotation(data.Position));
            data.TypeNew = nextNewType;
        }
    }
}
