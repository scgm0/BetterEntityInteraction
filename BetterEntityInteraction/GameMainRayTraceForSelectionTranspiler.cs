using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace BetterEntityInteraction;

public static class GameMainRayTraceForSelectionTranspiler {
	public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator) {
		var codes = instructions.ToList();

		var destinationIndex = -1;
		for (var i = 0; i < codes.Count - 2; i++) {
			if (codes[i].opcode != OpCodes.Ldarg_3 &&
				(codes[i].opcode != OpCodes.Ldarg_S || codes[i].operand.ToString() != "3") ||
				codes[i + 1].opcode != OpCodes.Ldnull ||
				codes[i + 2].opcode != OpCodes.Stind_Ref) {
				continue;
			}

			destinationIndex = i;
			break;
		}

		if (destinationIndex == -1) {
			BetterEntityInteractionModSystem.Api.Logger.Error("Transpiler 找不到跳转出口 (blockSelection = null)");
			return codes;
		}

		var bypassLabel = generator.DefineLabel();
		codes[destinationIndex].labels.Add(bypassLabel);

		var sqDistMethod = AccessTools.Method(typeof(Vec3d), nameof(Vec3d.SquareDistanceTo), [typeof(Vec3d)]);
		var injectionIndex = -1;

		for (var i = 0; i < codes.Count; i++) {
			if (!codes[i].Calls(sqDistMethod)) {
				continue;
			}

			for (var j = i; j >= 0; j--) {
				if (codes[j].opcode != OpCodes.Ldarg_2 &&
					(codes[j].opcode != OpCodes.Ldarga_S || codes[j].operand.ToString() != "2")) {
					continue;
				}

				injectionIndex = j;
				break;
			}

			break;
		}

		if (injectionIndex == -1) {
			BetterEntityInteractionModSystem.Api.Logger.Error("Transpiler 找不到插入点 (SquareDistanceTo 之前)");
			return codes;
		}

		var payload = new List<CodeInstruction> {
			new(OpCodes.Ldarg_3),
			new(OpCodes.Ldind_Ref),
			new(OpCodes.Call, AccessTools.Method(typeof(GameMainRayTraceForSelectionTranspiler), nameof(CanPenetrateWithBlock))),
			new(OpCodes.Brtrue, bypassLabel)
		};

		codes.InsertRange(injectionIndex, payload);

		return codes;
	}

	public static bool CanPenetrateWithBlock(BlockSelection blockSelection) {
		var mat = blockSelection?.Block?.BlockMaterial;
		return mat is EnumBlockMaterial.Plant or EnumBlockMaterial.Snow;
	}
}