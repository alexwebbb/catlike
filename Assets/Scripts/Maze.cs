using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Maze : MonoBehaviour {

    

    [Range(0f, 1f)]
    public float doorProbability;

    public IntVector2 size;
    public float generationStepDelay;

    public MazeCell cellPrefab;
    public MazePassage passagePrefab;
    public MazeWall[] wallPrefabs;
    public MazeDoor doorPrefab;
    public MazeRoomSettings[] roomSettings;

    private MazeCell[,] cells;
    private List<MazeRoom> rooms = new List<MazeRoom>();

    public IntVector2 RandomCoordinates {
        get {
            return new IntVector2(Random.Range(0, size.x), Random.Range(0, size.z));
        }
    }

    public bool ContainsCoordinates(IntVector2 coordinate) {
        return coordinate.x >= 0 && coordinate.x < size.x && coordinate.z >= 0 && coordinate.z < size.z;
    }

    public MazeCell GetCell(IntVector2 coordinates) {
        return cells[coordinates.x, coordinates.z];
    }

    public IEnumerator Generate() {
        WaitForSeconds delay = new WaitForSeconds(generationStepDelay);
        cells = new MazeCell[size.x, size.z];
        List<MazeCell> activeCells = new List<MazeCell>();
        DoFirstGenerationStep(activeCells);
        while (activeCells.Count > 0) {
            yield return delay;
            DoNextGenerationStep(activeCells);
        }
    }

    private MazeRoom CreateRoom(int indexToExclude) {
        MazeRoom newRoom = ScriptableObject.CreateInstance<MazeRoom>();
        newRoom.settingsIndex = Random.Range(0, roomSettings.Length);
        if (newRoom.settingsIndex == indexToExclude) {
            newRoom.settingsIndex = (newRoom.settingsIndex + 1) % roomSettings.Length;
        }
        newRoom.settings = roomSettings[newRoom.settingsIndex];
        rooms.Add(newRoom);
        return newRoom;
    }

    private MazeCell CreateCell(IntVector2 coordinates) {
        MazeCell newCell = Instantiate(cellPrefab) as MazeCell;
        cells[coordinates.x, coordinates.z] = newCell;
        newCell.coordinates = coordinates;
        newCell.name = "Maze Cell " + coordinates.x + ", " + coordinates.z;
        newCell.transform.parent = transform;
        newCell.transform.localPosition =
            new Vector3(coordinates.x - size.x * 0.5f + 0.5f, 0f, coordinates.z - size.z * 0.5f + 0.5f);
        return newCell;
    }

    private void DoFirstGenerationStep (List<MazeCell> activeCells) {
		MazeCell newCell = CreateCell(RandomCoordinates);
		newCell.Initialize(CreateRoom(-1));
		activeCells.Add(newCell);
	}


    private void DoNextGenerationStep(List<MazeCell> activeCells) {
        // active cells is a list of all the cells we can safely backtrack over, or traverse
        // ie not a wall

        // the current index is the last element of the list
        int currentIndex = activeCells.Count - 1;

        // fetch that current cell
        MazeCell currentCell = activeCells[currentIndex];

        // has the cell initialized all sides?
        if (currentCell.IsFullyInitialized) {

            // if so, the cell is no longer traversable
            // that is, it can no longer be used to place cells
            activeCells.RemoveAt(currentIndex);
            return;
        }

        // pick a random direction to test. this is how the maze is randomized.
        // this function picks only from the available edges
        MazeDirection direction = currentCell.RandomUninitializedDirection;

        // get the coordinates of the cell we are about to test, using that random direction
        IntVector2 coordinates = currentCell.coordinates + direction.ToIntVector2();

        // are we within the bounds of the map?
        if (ContainsCoordinates(coordinates)) {

            // Get Cell will attempt to retrieve a MazeCell from that cell
            MazeCell neighbor = GetCell(coordinates);

            // if get cell returns null, ie a mazecell has not been initialized there yet
            if (neighbor == null) {

                // create a mazecell at that position, since there is not one there currently
                neighbor = CreateCell(coordinates);

                // an empty cell means we can safely extend a passage to that cell from the current cell
                CreatePassage(currentCell, neighbor, direction);

                // add the mazecell to the list so that it can be traversed
                activeCells.Add(neighbor);

            // this case will eliminate walls between room sharing cells
            } else if (currentCell.room.settingsIndex == neighbor.room.settingsIndex) {

                CreatePassageInSameRoom(currentCell, neighbor, direction);

            } else {

                // in this case, a cell exists at the position we tested, and a passage
                // cannot be created. therefore we will create a wall
                CreateWall(currentCell, neighbor, direction);

                // SetColorOnBacktrack(activeCells[currentIndex - 1]);
            }

        } else {

            // in this case the position is outside of the bounds of the map, thus a wall
            CreateWall(currentCell, null, direction);

        }
    }


    private void CreatePassage(MazeCell cell, MazeCell otherCell, MazeDirection direction) {

        // remember... maze passage is derived from cell edge
        MazePassage prefab = Random.value < doorProbability ? doorPrefab : passagePrefab;
        MazePassage passage = Instantiate(prefab) as MazePassage;
        passage.Initialize(cell, otherCell, direction);
        passage = Instantiate(prefab) as MazePassage;
        if (passage is MazeDoor) {
            otherCell.Initialize(CreateRoom(cell.room.settingsIndex));
        } else {
            otherCell.Initialize(cell.room);
        }
        passage.Initialize(otherCell, cell, direction.GetOpposite());
    }

    private void CreatePassageInSameRoom (MazeCell cell, MazeCell otherCell, MazeDirection direction) {
		MazePassage passage = Instantiate(passagePrefab) as MazePassage;
		passage.Initialize(cell, otherCell, direction);
		passage = Instantiate(passagePrefab) as MazePassage;
		passage.Initialize(otherCell, cell, direction.GetOpposite());
        if (cell.room != otherCell.room) {
            MazeRoom roomToAssimilate = otherCell.room;
            cell.room.Assimilate(roomToAssimilate);
            rooms.Remove(roomToAssimilate);
            Destroy(roomToAssimilate);
        }
    }
	

    private void CreateWall(MazeCell cell, MazeCell otherCell, MazeDirection direction) {
        MazeWall wall = Instantiate(wallPrefabs[Random.Range(0, wallPrefabs.Length)]) as MazeWall;
        wall.Initialize(cell, otherCell, direction);
        if (otherCell != null) {
            wall = Instantiate(wallPrefabs[Random.Range(0, wallPrefabs.Length)]) as MazeWall;
            wall.Initialize(otherCell, cell, direction.GetOpposite());
        }
    }
}