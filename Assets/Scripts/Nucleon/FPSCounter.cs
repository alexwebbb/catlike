using UnityEngine;

public class FPSCounter : MonoBehaviour {

    public int frameRange = 60;

    public int HighestFPS { get; private set; }
    public int AverageFPS { get; private set; }
    public int LowestFPS { get; private set; }

    int[] fpsBuffer;
    int fpsBufferIndex;

    void InitializeBuffer() {
        if (frameRange <= 0) {
            frameRange = 1;
        }
        fpsBuffer = new int[frameRange];
        fpsBufferIndex = 0;
    }

    void Update() {
        if (fpsBuffer == null || fpsBuffer.Length != frameRange) {
            InitializeBuffer();
        }
        UpdateBuffer();
        CalculateFPS();
    }

    void UpdateBuffer() {
        // how does the inverse of the time work here?
        // welll....... I guess it is delta time
        // so that means that it represents the percentage of one second
        // that the frame is taking
        // so inverting it will give us the number of frames per second
        // cool
        fpsBuffer[fpsBufferIndex++] = (int)(1f / Time.unscaledDeltaTime);
        Debug.Log(1f / Time.unscaledDeltaTime);
        if (fpsBufferIndex >= frameRange) {
            fpsBufferIndex = 0;
        }
    }

    void CalculateFPS() {
        int sum = 0;
        int highest = 0;
        int lowest = int.MaxValue;
        for (int i = 0; i < frameRange; i++) {
            int fps = fpsBuffer[i];
            sum += fps;
            if (fps > highest) {
                highest = fps;
            }
            if (fps < lowest) {
                lowest = fps;
            }
        }
        AverageFPS = sum / frameRange;
        HighestFPS = highest;
        LowestFPS = lowest;
    }
}