using UnityEngine;

public class MazeDoor : MazePassage {

    public Transform hinge;

    private MazeDoor OtherSideOfDoor {
        get {
            // this will return null when constructing the "front" of the door
            return otherCell.GetEdge(direction.GetOpposite()) as MazeDoor;
        }
    }

    public override void Initialize(MazeCell primary, MazeCell other, MazeDirection direction) {
        base.Initialize(primary, other, direction);
        if (OtherSideOfDoor != null) {
            // flipping the door 
            // because it is being flipped from the hinge, it is inverted with scale
            hinge.localScale = new Vector3(-1f, 1f, 1f);
            Vector3 p = hinge.localPosition;
            // then the position is adjusted so that it is back at its original position
            p.x = -p.x;
            hinge.localPosition = p;
        }
    }
}