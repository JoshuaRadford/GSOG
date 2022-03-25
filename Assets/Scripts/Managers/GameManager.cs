using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class GameManager : Singleton<GameManager> {
    public Tile TILE_TERRAINBASE;
    public Tile TILE_TERRAINWALL;
    public Tile TILE_MOVEMENTBASE;
    public Tile TILE_MOVEMENTPATH;

    // Start is called before the first frame update
    void Start() {
        Manager_Camera MANAGER_CAMERA = new Manager_Camera();
        Manager_Scenes MANAGER_SCENES = new Manager_Scenes();
        Manager_GridCombat MANAGER_GRIDCOMBAT = new Manager_GridCombat();
        Manager_Grid MANAGER_GRID = new Manager_Grid();
        Manager_Input MANAGER_INPUT = new Manager_Input();

        Manager_Grid.InitMaps();
        Manager_Grid.SyncAllTileMapsWithCellmaps();
    }

    // Update is called once per frame
    void Update() {

    }
}
