using System.Reflection;
using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.Common;

[assembly: ModInfo(name: "更好的实体交互", modID: "betterentityinteraction", Authors = ["神麤詭末"], Description = "允许和实体的交互（包括左键攻击）忽略植物方块（比如草）。")]

namespace BetterEntityInteraction;

public class BetterEntityInteractionModSystem : ModSystem {
	static private readonly MethodInfo GameMainRayTraceForSelection = typeof(GameMain).GetMethod(
		nameof(GameMain.RayTraceForSelection),
		[
			typeof(IWorldIntersectionSupplier),
			typeof(Ray),
			typeof(BlockSelection).MakeByRefType(),
			typeof(EntitySelection).MakeByRefType(),
			typeof(BlockFilter),
			typeof(EntityFilter)
		]);

	static private readonly MethodInfo GameMainRayTraceForSelectionPrefix =
		typeof(GameMainRayTraceForSelectionPrefix).GetMethod("Prefix");

	public string HarmonyId => Mod.Info.ModID;

	public Harmony HarmonyInstance => new(HarmonyId);

	public override bool ShouldLoad(EnumAppSide forSide) { return forSide == EnumAppSide.Client; }

	public override void StartClientSide(ICoreClientAPI api) {
		HarmonyInstance.Patch(original: GameMainRayTraceForSelection,
			prefix: GameMainRayTraceForSelectionPrefix);
	}

	public override void Dispose() {
		base.Dispose();
		HarmonyInstance.Unpatch(original: GameMainRayTraceForSelection,
			patch: GameMainRayTraceForSelectionPrefix);
	}
}