using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;

namespace BetterEntityInteraction;

public static class GameMainRayTraceForSelectionPrefix {
	public static bool Prefix(
		ref AABBIntersectionTest ___interesectionTester,
		ref EntitySelection ___entitySelTmp,
		IWorldIntersectionSupplier supplier,
		Ray ray,
		ref BlockSelection blockSelection,
		ref EntitySelection entitySelection,
		BlockFilter bfilter = null,
		EntityFilter efilter = null) {
		___interesectionTester.LoadRayAndPos(ray);
		___interesectionTester.blockSelectionTester = supplier;
		var length = (float)ray.Length;
		blockSelection = ___interesectionTester.GetSelectedBlock(length, bfilter);
		var entitiesAround = supplier.GetEntitiesAround(ray.origin,
			length,
			length,
			entity => efilter == null || efilter(entity));
		Entity entity1 = null;
		var num1 = 1048576.0;
		foreach (var entity2 in entitiesAround) {
			if (!___interesectionTester.RayIntersectsWithCuboid(entity2.SelectionBox,
				entity2.SidedPos.X,
				entity2.SidedPos.Y,
				entity2.SidedPos.Z)) continue;
			double num2 = entity2.Pos.SquareDistanceTo(ray.origin.X, ray.origin.Y, ray.origin.Z);
			if (!(num2 < num1)) continue;
			entity1 = entity2;
			num1 = num2;
			___entitySelTmp.Entity = entity2;
			___entitySelTmp.Face = ___interesectionTester.hitOnBlockFace;
			___entitySelTmp.HitPosition =
				___interesectionTester.hitPosition.SubCopy(entity2.SidedPos.X, entity2.SidedPos.Y, entity2.SidedPos.Z);
			___entitySelTmp.Position = entity2.SidedPos.XYZ;
		}

		entitySelection = null;
		if (entity1 == null)
			return false;
		if (blockSelection != null) {
			var position = blockSelection.Position;
			var pos1 = new Vec3d(position.X, position.Y, position.Z).Add(blockSelection.HitPosition);
			var pos2 = entity1.SidedPos.XYZ.Add(___entitySelTmp.HitPosition);
			if (ray.origin.SquareDistanceTo(pos2) >= (double)ray.origin.SquareDistanceTo(pos1) &&
				blockSelection.Block.BlockMaterial != EnumBlockMaterial.Plant)
				return false;
			blockSelection = null;
		}

		entitySelection = ___entitySelTmp.Clone();

		return false;
	}
}