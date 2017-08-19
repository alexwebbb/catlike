using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Cube : MonoBehaviour {

    public int xSize, ySize, zSize;

    private Mesh mesh;
    private Vector3[] vertices;

    private void Awake() {
        Generate();
    }

    private void Generate() {
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.name = "Procedural Cube";
        CreateVertices();
        CreateTriangles();
    }


    private void CreateVertices() {


        int cornerVertices = 8;
        int edgeVertices = (xSize + ySize + zSize - 3) * 4;
        int faceVertices = (
            (xSize - 1) * (ySize - 1) +
            (xSize - 1) * (zSize - 1) +
            (ySize - 1) * (zSize - 1)) * 2;
        vertices = new Vector3[cornerVertices + edgeVertices + faceVertices];

        int v = 0;
        for (int y = 0; y <= ySize; y++) {
            for (int x = 0; x <= xSize; x++) {
                vertices[v++] = new Vector3(x, y, 0);

            }
            for (int z = 1; z <= zSize; z++) {
                vertices[v++] = new Vector3(xSize, y, z);

            }
            for (int x = xSize - 1; x >= 0; x--) {
                vertices[v++] = new Vector3(x, y, zSize);

            }
            for (int z = zSize - 1; z > 0; z--) {
                vertices[v++] = new Vector3(0, y, z);

            }
        }

        for (int z = 1; z < zSize; z++) {
            for (int x = 1; x < xSize; x++) {
                vertices[v++] = new Vector3(x, ySize, z);

            }
        }
        for (int z = 1; z < zSize; z++) {
            for (int x = 1; x < xSize; x++) {
                vertices[v++] = new Vector3(x, 0, z);

            }
        }

        mesh.vertices = vertices;
    }

    private void CreateTriangles() {
        int quads = (xSize * ySize + xSize * zSize + ySize * zSize) * 2;
        int[] triangles = new int[quads * 6];
        int ring = (xSize + zSize) * 2;
        int t = 0, v = 0;

        // why use q? dont make a damn lick a sense
        for (int q = 0; q < xSize; q++, v++) {
            t = SetQuad(triangles, t, v, v + 1, v + ring, v + ring + 1);
        }

        mesh.triangles = triangles;
    }


    // naming of these is based on image http://catlikecoding.com/unity/tutorials/rounded-cube/03-quad.png
    private static int SetQuad(int[] triangles, int index, int v00, int v10, int v01, int v11) {
        // lower left
		triangles[index] = v00;
        // upper left
		triangles[index + 1] = triangles[index + 4] = v01;
        // lower right
		triangles[index + 2] = triangles[index + 3] = v10;
        // upper right
		triangles[index + 5] = v11;
        // next index
		return index + 6;
	}


    private void OnDrawGizmos() {
        if (vertices == null) {
            return;
        }
        Gizmos.color = Color.black;
        for (int i = 0; i < vertices.Length; i++) {
            Gizmos.DrawSphere(vertices[i], 0.1f);
        }
    }
}