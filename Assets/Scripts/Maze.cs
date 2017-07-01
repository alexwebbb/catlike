using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Maze : MonoBehaviour {


    public IntVector2 size;
    public float generationStepDelay;

    public MazeCell cellPrefab;
    public MazePassage passagePrefab;
    public MazeWall wallPrefab;

    private MazeCell[,] cells;

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

    private void DoFirstGenerationStep(List<MazeCell> activeCells) {
        activeCells.Add(CreateCell(RandomCoordinates));
        activeCells[0].backtrackCount = activeCells[0].backtrackCount + 3;

    }


    private void DoNextGenerationStep(List<MazeCell> activeCells) {
        // active cells is a list of all the cells we can safely backtrack over, or traverse
        // ie not a wall

        // the current index is the last element of the list
        int currentIndex = activeCells.Count - 1;

        // fetch that current cell
        MazeCell currentCell = activeCells[currentIndex];

        // pick a random direction to test. this is how the maze is randomized
        MazeDirection direction = MazeDirections.RandomValue;

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

            } else {

                // in this case, a cell exists at the position we tested, and a passage
                // cannot be created. therefore we will create a wall
                CreateWall(currentCell, neighbor, direction);

                // the edge we are testing cannot be traversed here
                // this may mean that those coordinates already exist in the list as a passage
                activeCells.RemoveAt(currentIndex);

                SetColorOnBacktrack(activeCells[currentIndex - 1]);
            }

        } else {

            // in this case the position is outside of the bounds of the map, thus a wall
            CreateWall(currentCell, null, direction);

            // if (currentIndex == 0 || activeCells[currentIndex - 1] == null) return false;

            // a position outside of the bounds of the map cannot be traversed
            activeCells.RemoveAt(currentIndex);

        }
    }

    private void SetColorOnBacktrack(MazeCell cell) {

        Material mat = cell.GetComponentInChildren<MeshRenderer>().material;

        switch (cell.backtrackCount) {
            case 0:
                mat.color = Color.red;
                break;
            case 1:
                mat.color = Color.yellow;
                break;
            case 2:
                mat.color = Color.green;
                break;
            case 3:
                mat.color = Color.cyan;
                break;
            case 4:
                mat.color = Color.blue;
                break;
        }

        cell.backtrackCount++;

    }

    private void CreatePassage(MazeCell cell, MazeCell otherCell, MazeDirection direction) {
        MazePassage passage = Instantiate(passagePrefab) as MazePassage;
        passage.Initialize(cell, otherCell, direction);
        passage = Instantiate(passagePrefab) as MazePassage;
        passage.Initialize(otherCell, cell, direction.GetOpposite());
    }

    private void CreateWall(MazeCell cell, MazeCell otherCell, MazeDirection direction) {
        MazeWall wall = Instantiate(wallPrefab) as MazeWall;
        wall.Initialize(cell, otherCell, direction);
        if (otherCell != null) {
            wall = Instantiate(wallPrefab) as MazeWall;
            wall.Initialize(otherCell, cell, direction.GetOpposite());
        }
    }
}