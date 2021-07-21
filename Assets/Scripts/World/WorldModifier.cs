using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldModifier : MonoBehaviour
{
    [SerializeField] PickupManager pickupManager;
    [SerializeField] AudioSource audioS;
    [SerializeField] WorldGenerator wg;
    [SerializeField] WorldLoader wl;
    [SerializeField] Pickup pickupPrefab;
    [SerializeField] BlockBreakingVisual bbv;
    [SerializeField] ChestInventory cim;
    [SerializeField] ChestManager cm;

    // This needs to be a vector3 so that i can specify what layer it is on
    Dictionary<Vector3Int, int> blockDurability;

    public Vector2Int blockModifiedAt = -Vector2Int.one;

    private void Update()
    {
        GD.wd.elapsedTime += Time.fixedDeltaTime;
    }

    void Awake()
    {
        blockDurability = new Dictionary<Vector3Int, int>();
    }

    // Pretty much a Queue
    List<Vector2Int> treeBlocksToChop = new List<Vector2Int>();

    bool chopping;

    IEnumerator ChopTree()
    {
        while (treeBlocksToChop.Count > 0)
        {
            Vector2Int pos = treeBlocksToChop[0];
            BreakTreeBlock(pos.x, pos.y);
            //DamageBlock(pos.x, pos.y, 1, 255, true);
            treeBlocksToChop.RemoveAt(0);
            yield return new WaitForSeconds(.015f);
        }

        chopping = false;
    }

    bool CanAttach(int x, int y, ItemDataContainer item)
    {
        if (GD.wd.blockMap[x, y, item.tileData.placeOnLayer] != 0) { return false; }
        if (!item.tileData.placeBehindForeground && GD.wd.blockMap[x, y, 0] != 0) { return false; }

        if (item.tileData.canFloat) { return true; }

        if (!item.tileData.noAttachToBackground)
        {
            // It can attach to walls

            if (GD.wd.blockMap[x, y, 2] != 0)
            {
                return true;
            }
        }

        var alayer = item.tileData.attachToLayer;

        if (GD.wd.blockMap[x + 1, y, alayer] != 0) { return true; }
        else if (GD.wd.blockMap[x - 1, y, alayer] != 0) { return true; }
        else if (GD.wd.blockMap[x, y + 1, alayer] != 0) { return true; }
        else if (GD.wd.blockMap[x, y - 1, alayer] != 0) { return true; }

        for (int i = 0; i < item.tileData.otherAttachTo.Count; i++)
        {
            var target = item.tileData.otherAttachTo[i];

            if (GD.wd.blockMap[x + 1, y, target.tileData.placeOnLayer] == target.itemData.id) { return true; }
            else if (GD.wd.blockMap[x - 1, y, target.tileData.placeOnLayer] == target.itemData.id) { return true; }
            else if (GD.wd.blockMap[x, y + 1, target.tileData.placeOnLayer] == target.itemData.id) { return true; }
            else if (GD.wd.blockMap[x, y - 1, target.tileData.placeOnLayer] == target.itemData.id) { return true; }
        }

        return false;
    }

    public bool CanPlace(int x, int y, ItemDataContainer item)
    {
        if (x >= GD.wd.worldWidth || x < 0 || y >= GD.wd.worldHeight || y < 0) { return false; }

        var layer = item.tileData.placeOnLayer;

        if (layer == 2 && CanAttach(x, y, item))
        {
            return true;
        }

        if (GD.wd.blockMap[x, y, 0] == 0 && GD.wd.blockMap[x, y, 1] == 0 && CanAttach(x, y, item))
        {
            return true;
        }

        return false;
    }

    public short CanBreak(int x, int y, Tool tool)
    {
        if (x >= GD.wd.worldWidth || x < 0 || y >= GD.wd.worldHeight || y < 0) { return -1; }

        if (wg.ItemAtPosition(x, y, 0).tileData.requiredTool == tool && GD.wd.blockMap[x, y, 0] != 0 && !wg.ItemAtPosition(x, y + 1, 1).tileData.cementBlockBelow)
        {
            return 0;
        }
        else if (wg.ItemAtPosition(x, y, 1).tileData.requiredTool == tool && GD.wd.blockMap[x, y, 1] != 0)
        {
            return 1;
        }
        else if (wg.ItemAtPosition(x, y, 2).tileData.requiredTool == tool && GD.wd.blockMap[x, y, 2] != 0)
        {
            return 2;
        }

        return -1;
    }

    void TryPlayClip(ItemDataContainer item, bool breakS = false)
    {
        if (breakS)
        {
            if (item.tileData.breakSound)
            {
                audioS.PlayOneShot(item.tileData.breakSound.sound.RandomSound());
            }
        }
        else if (item.tileData.hitSound)
        {
            audioS.PlayOneShot(item.tileData.hitSound.sound.RandomSound());
        }
    }

    public void BreakBlock(int x, int y, byte layer, bool tryDropPickup = true, bool playBreakSound = true)
    {
        ItemDataContainer targetBlock = wg.ItemAtPosition(x, y, layer);
        wl.SetBlockInTilemap(x, y, layer);

        GD.wd.blockMap[x, y, layer] = 0;

        if (layer == 0) { blockModifiedAt = new Vector2Int(x, y); }

        if (tryDropPickup)
        {
            TrySpawnFromBreak(x, y, targetBlock);
        }

        if (playBreakSound)
        {
            TryPlayClip(targetBlock, true);
        }
    }

    bool DamageBlock(int x, int y, byte layer, int strength, ItemDataContainer targetBlock)
    {
        Vector3Int blockPos = new Vector3Int(x, y, layer);

        if (blockDurability.TryGetValue(blockPos, out int val))
        {
            // There is already damage done to this block

            blockDurability[blockPos] -= strength;

            if (blockDurability[blockPos] > 0)
            {
                if (!targetBlock.tileData.hideBreakingGraphic)
                {
                    bbv.DisplayDamage(x, y, targetBlock.tileData.hardness, blockDurability[blockPos]);
                }

                TryPlayClip(targetBlock);
                return false;
            }
            else
            {
                blockDurability.Remove(blockPos);
                bbv.EmitParticles(x, y);
                bbv.Finish();

                return true;
            }
        }
        else
        {
            // There is NOT already damage done to this block

            if (targetBlock.tileData.hardness - strength > 0)
            {
                blockDurability.Add(blockPos, targetBlock.tileData.hardness - strength);

                if (!targetBlock.tileData.hideBreakingGraphic)
                {
                    bbv.DisplayDamage(x, y, targetBlock.tileData.hardness, targetBlock.tileData.hardness - strength);
                    TryPlayClip(targetBlock);
                }

                return false;
            }
            else
            {
                blockDurability.Remove(blockPos);
                bbv.EmitParticles(x, y);
                bbv.Finish();

                return true;
            }
        }
    }

    public void HitBlock(int x, int y, byte layer, int strength)
    {
        ItemDataContainer targetBlock = wg.ItemAtPosition(x, y, layer);

        if (!DamageBlock(x, y, layer, strength, targetBlock)) { return; }

        if (targetBlock.tileData.isPartOfMultiTile)
        {
            BreakMultiTileBlock(x, y, layer);
            return;
        }

        switch (layer)
        {
            case 0: BreakBlock(x, y, layer, true); break;
            case 1:

                if (targetBlock.tileData.treeTile)
                {
                    BreakTreeBlock(x, y);
                    TryPlayClip(targetBlock, true);
                }
                else
                {
                    BreakBlock(x, y, layer, true, true);
                }

                break;
            case 2:
                BreakBlock(x, y, layer);
                break;
        }
    }

    void BreakMultiTileBlock(int x, int y, byte layer)
    {
        TileData targetBlock = wg.ItemAtPosition(x, y, layer).tileData;
        Vector2Int rootPos = GetRootPos(x, y, targetBlock);

        var structureData = GD.wd.multiTileItems[rootPos];

        for (int sx = 0; sx < structureData.tileData.multiBlockStructure.width; sx++)
        {
            for (int sy = 0; sy < structureData.tileData.multiBlockStructure.height; sy++)
            {
                var posx = rootPos.x + sx;
                var posy = rootPos.y + sy;

                //if (posx == x && posy == y) { continue; }
                BreakBlock(posx, posy, layer, false);
            }
        }

        if (targetBlock.isChest)
        {
            var rootp = GetRootPos(x, y, targetBlock);

            if (cim.open)
            {
                cim.CloseChest();
            }

            List<ItemPackage> chestItems = cm.GetChestContentsAt(rootp.x, rootp.y);

            for (int i = 0; i < chestItems.Count; i++)
            {
                SpawnPickup(x, y, chestItems[i]);
            }
            //chests.Remove(rootp);
        }

        TrySpawnFromBreak(x, y, structureData);
        GD.wd.multiTileItems.Remove(rootPos);
    }

    Vector2Int GetRootPos(int x, int y, TileData multiTilePart)
    {
        return new Vector2Int(x - multiTilePart.partOfMultiTileData.relativePosition.x, y - multiTilePart.partOfMultiTileData.relativePosition.y);
    }

    void BreakTreeBlock(int x, int y)
    {
        BreakBlock(x, y, 1, true, false);

        if (wg.ItemAtPosition(x, y + 1, 1).tileData.treeTile)
        {

            treeBlocksToChop.Add(new Vector2Int(x, y + 1));

            // Check for stumps

            if (wg.ItemAtPosition(x + 1, y, 1).tileData.treeTile)
            {
                treeBlocksToChop.Add(new Vector2Int(x + 1, y));
            }

            if (wg.ItemAtPosition(x - 1, y, 1).tileData.treeTile)
            {
                treeBlocksToChop.Add(new Vector2Int(x - 1, y));
            }

            if (!chopping)
            {
                chopping = true;
                StartCoroutine(ChopTree());
            }
        }
    }

    public void TrySpawnFromBreak(float x, float y, ItemDataContainer itemBroken)
    {
        if (itemBroken.tileData.itemDroppedOnBreak.item == null) { return; }
        SpawnPickup(x, y, itemBroken.tileData.itemDroppedOnBreak);
    }

    public void SpawnPickup(float x, float y, ItemPackage item)
    {
        var rotation = Quaternion.Euler(0, 0, Random.Range(0, 360));
        var pickup = Instantiate(pickupPrefab, new Vector3(x + Random.Range(-.25f, .25f), y + Random.Range(-.25f, .25f), 0) + Vector3.one / 2, rotation);
        pickup.SetItem(item);
        pickupManager.AddPickup(pickup);
    }

    public bool CanPlaceStructure(int rootX, int rootY, Structure s)
    {
        for (int rx = 0; rx < s.width; rx++)
            for (int ry = 0; ry < s.height; ry++)
            {
                var index = rx + (ry * s.width);
                var tile = s.tiles[s.structureData[index]];

                Vector2Int worldPosition = new Vector2Int((rootX + rx), rootY + ry);

                if (!CanAttach(worldPosition.x, worldPosition.y, tile))
                {
                    return false;
                }
            }

        return true;
    }

    public void TryInteractAt(int x, int y, ref Vector2Int pos)
    {
        ItemDataContainer targetItem = wg.ItemAtPosition(x, y, 1);

        if (GD.wd.blockMap[x, y, 1] != 0 && targetItem.tileData.interactable)
        {
            if (targetItem.tileData.isChest)
            {
                var root = GetRootPos(x, y, targetItem.tileData);

                if (GD.wd.chests.TryGetValue(root, out ChestData val))
                {
                    cim.OpenWithData(val);
                }
                else
                {
                    ChestData c = cm.CreateNewChestDataAt(root.x, root.y, false);
                    cim.OpenWithData(c);
                }

                pos = new Vector2Int(x, y);
            }
        }
        else
        {
            pos = -Vector2Int.one;
        }
    }

    public void PlaceBlock(int x, int y, ItemDataContainer item)
    {
        if (item.tileData.placeOnLayer == 0)
        {
            blockModifiedAt = new Vector2Int(x, y);
        }

        if (item.tileData.multiBlockStructure)
        {
            // It takes up more than one tile
            GD.wd.multiTileItems.Add(new Vector2Int(x, y), item);
            wg.GenerateStructure(item.tileData.multiBlockStructure, x, y, true);

            if (item.tileData.isChest)
            {
                cm.CreateNewChestDataAt(x, y, false);
            }

            return;
        }
        else
        {
            GD.wd.blockMap[x, y, item.tileData.placeOnLayer] = item.itemData.id;
        }

        wl.SetBlockInTilemap(x, y, item.tileData.placeOnLayer, item.tileData.tile);
    }

}
