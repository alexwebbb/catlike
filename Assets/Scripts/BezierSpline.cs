using UnityEngine;
using System;

public class BezierSpline : MonoBehaviour {

    [SerializeField]
    private Vector3[] points;

    [SerializeField]
    private BezierControlPointMode[] modes;

    [SerializeField]
    private bool loop;

    public int ControlPointCount {
        get {
            return points.Length;
        }
    }

    public int CurveCount {
        get {
            return (points.Length - 1) / 3;
        }
    }

    public Vector3 GetControlPoint(int index) {
        return points[index];
    }

    public bool Loop {
        get {
            return loop;
        }
        set {
            loop = value;
            if (value == true) {
                modes[modes.Length - 1] = modes[0];
                SetControlPoint(0, points[0]);
            }
        }
    }

    public void SetControlPoint(int index, Vector3 point) {
        // the role of this function is to update the position of a point when changed by the user

        // this block updates the position of points adjacent to a point on an edge
        // when that "middle" (ie index mod 3 == 0) point is moved
        if (index % 3 == 0) {

            // the distance of the point from its last position
            Vector3 delta = point - points[index];

            // is there a point below?
            if (index > 0) {
                points[index - 1] += delta;
            }

            // is there a point above?
            if (index + 1 < points.Length) {
                points[index + 1] += delta;
            }
        }

        // this next line actually updates the position of the point
        points[index] = point;
        // this function rewrites the position based on the constraint mode
        EnforceMode(index);
    }

    public void SetControlPointMode(int index, BezierControlPointMode mode) {
        int modeIndex = (index + 1) / 3;
        modes[modeIndex] = mode;
        if (loop) {
            if (modeIndex == 0) {
                modes[modes.Length - 1] = mode;
            } else if (modeIndex == modes.Length - 1) {
                modes[0] = mode;
            }
        }
        EnforceMode(index);
    }

    public Vector3 GetPoint(float t) {
        int i;
        if (t >= 1f) {
            t = 1f;
            i = points.Length - 4;
        } else {
            t = Mathf.Clamp01(t) * CurveCount;
            i = (int)t;
            t -= i;
            i *= 3;
        }
        return transform.TransformPoint(Bezier.GetPoint(
            points[i], points[i + 1], points[i + 2], points[i + 3], t));
    }

    public Vector3 GetVelocity(float t) {
        int i;
        if (t >= 1f) {
            t = 1f;
            i = points.Length - 4;
        } else {
            t = Mathf.Clamp01(t) * CurveCount;
            i = (int)t;
            t -= i;
            i *= 3;
        }
        return transform.TransformPoint(Bezier.GetFirstDerivative(
            points[i], points[i + 1], points[i + 2], points[i + 3], t)) - transform.position;
    }

    public Vector3 GetDirection(float t) {
        return GetVelocity(t).normalized;
    }

    public void AddCurve() {
        Vector3 point = points[points.Length - 1];
        Array.Resize(ref points, points.Length + 3);
        point.x += 1f;
        points[points.Length - 3] = point;
        point.x += 1f;
        points[points.Length - 2] = point;
        point.x += 1f;
        points[points.Length - 1] = point;

        Array.Resize(ref modes, modes.Length + 1);
        modes[modes.Length - 1] = modes[modes.Length - 2];
    }

    public BezierControlPointMode GetControlPointMode(int index) {
        return modes[(index + 1) / 3];
    }

    public void Reset() {
        points = new Vector3[] {
            new Vector3(1f, 0f, 0f),
            new Vector3(2f, 0f, 0f),
            new Vector3(3f, 0f, 0f),
            new Vector3(4f, 0f, 0f)
        };

        modes = new BezierControlPointMode[] {
            BezierControlPointMode.Free,
            BezierControlPointMode.Free
        };
    }

    private void EnforceMode(int index) {

        // retrieve the mode from the index
        int modeIndex = (index + 1) / 3;
        BezierControlPointMode mode = modes[modeIndex];

        // if no action is required, return
        if (mode == BezierControlPointMode.Free || modeIndex == 0 || modeIndex == modes.Length - 1) {
            return;
        }


        // slot the involved points into three categories; fixed, middle and enforced Index
        //
        // when index is not middle, fixed is the point we are moving, and enforced is the
        // point that will be changed in relation. 
        //
        // in a scenario where middle is the index, the lower point is fixed,
        // but it doesn't matter because the points get moved along with 
        // the middle point in the parent function.
        int middleIndex = modeIndex * 3;
        int fixedIndex, enforcedIndex;
        if (index <= middleIndex) {
            fixedIndex = middleIndex - 1;
            enforcedIndex = middleIndex + 1;
        } else {
            fixedIndex = middleIndex + 1;
            enforcedIndex = middleIndex - 1;
        }


        // here our enforced point is finally adjusted, either to match tangent and exact distance,
        // or to match tangent and modify distance in a proportion related to its original distance
        Vector3 middle = points[middleIndex];
        Vector3 enforcedTangent = middle - points[fixedIndex];
        if (mode == BezierControlPointMode.Aligned) {
            enforcedTangent = enforcedTangent.normalized * Vector3.Distance(middle, points[enforcedIndex]);
        }
        points[enforcedIndex] = middle + enforcedTangent;
    }
}