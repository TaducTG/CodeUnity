using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Tiles/Custom Rule Tile")]
public class MyCustomRuleTile : RuleTile<MyCustomRuleTile.Neighbor>
{
    public enum Neighbor
    {
        Grass = 3,
        Dirt = 4,
        Stone = 5,
    }

    public override bool RuleMatch(int neighbor, TileBase tile)
    {
        if (neighbor == (int)Neighbor.Grass)
        {
            return tile != null && tile.name.StartsWith("grass_");
        }
        if (neighbor == (int)Neighbor.Dirt)
        {
            return tile != null && tile.name.StartsWith("dirt_");
        }
        if (neighbor == (int)Neighbor.Stone)
        {
            return tile != null && tile.name.StartsWith("stone_");
        }
        return base.RuleMatch(neighbor, tile);
    }
}
