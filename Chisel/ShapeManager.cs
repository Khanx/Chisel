using System.Collections.Generic;
using System.Linq;
using System.IO;

using Pipliz;
using ModLoaderInterfaces;
using static ItemTypesServer;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Chisel
{
    [ModLoader.ModManager]
    public class ShapeManager : IAddItemTypes, IOnAssemblyLoaded
    {
        public static readonly string baseName = "Khanx.Kings.Shape";
        public static readonly List<string> shapeName = new List<string>()
        {
            ".CurveHorizontal",
            ".CurveVertical",
            ".CurveVerticalInverted",
            ".CurveCorner",
            ".CurveCornerInverted",
            //".SlabVertical",
            //".SlabHorizontalUp",
            //".SlabHorizontalDown",
            ".SlopeVertical",
            ".SlopeVerticalInverted",
            ".SlopeHorizontal",
            ".SlopeCorner",
            ".SlopeCornerInverted",
            ".SlopeEdge",
            ".SlopeEdgeInverted",
            ".SlopeConnector",
            ".SlopeConnectorInverted",
            ".SlopePeak",
            ".SlopeDA",
            ".SlopeDAInverted",
            ".2Stairs",
            ".2StairsCorner",
            ".2StairsSpinner",
            //".4Stairs",
            //".4StairsCorner",
            //".4StairsSpinner"
        };
        public static List<string> typeWithoutShape = new List<string>();
        public static List<ushort> typeWithShapes = new List<ushort>();

        public void OnAssemblyLoaded(string path)
        {
            string shapeWithoutShapeFile = Path.GetDirectoryName(path).Replace("\\", "/") + "/typeWithoutShape.json";

            if (!File.Exists(shapeWithoutShapeFile))
                return;

            typeWithoutShape = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(shapeWithoutShapeFile));
        }

        [ModLoader.ModCallbackDependsOn("pipliz.server.applymoditempatches")]
        public void AddItemTypes(Dictionary<string, ItemTypeRaw> types)
        {
            //The list is also in ShapePlacers
            var blocksToAddShapes = types.Values.ToList().FindAll(type => type.IsPlaceable && !type.NeedsBase && type.Mesh == null && type.Categories != null && !type.Categories.Contains("job") && !type.Categories.Contains("lantern") && !typeWithoutShape.Contains(type.name)).ToList();

            typeWithShapes = blocksToAddShapes.ConvertAll(type => type.ItemIndex);

            //TEST WITH PLANKS
            //blocksToAddShapes = types.Values.ToList().FindAll(type => type.name.Equals("planks")).ToList();

            foreach (var baseType in blocksToAddShapes)
            {

                foreach (var name in shapeName)
                {

                    ItemTypeRaw shape = new ItemTypeRaw(baseType.name + name, baseType);

                    ItemTypeRaw shapeType = types[baseName + name];

                    shape.description.SetAs("mesh", shapeType.description["mesh"]);
                    shape.description.SetAs("meshRotationEuler", shapeType.description["meshRotationEuler"]);
                    shape.description.SetAs("colliders", shapeType.description["colliders"]);
                    shape.description.SetAs("canBuildUpon", shapeType.description["canBuildUpon"]);
                    shape.description.SetAs("customData", shapeType.description["customData"]);


                    //if (UnityEngine.Color.white != baseType.Color)
                    if (baseType.description.ContainsKey("color"))
                    {
                        JObject customData = (JObject)shape.description["customData"];
                        customData.SetAs("colors", new JArray { "#ffffff->" + baseType.description["color"] });
                    }

                    BlockRotator.CreateAndRegisterRotatedBlocks(types, shape, null, null, null, null, null, null, null);
                }
            }
        }
    }
}
