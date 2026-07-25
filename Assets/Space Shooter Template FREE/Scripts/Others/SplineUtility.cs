using System;
using UnityEngine;

public static class SplineUtility
{
    public static Vector3[] PadForCatmullRom(Vector3[] path)
    {
        if (path == null || path.Length < 2)
            throw new ArgumentException("Path needs at least 2 points.", nameof(path));

        const int extraPoints = 2;
        Vector3[] newPathPos = new Vector3[path.Length + extraPoints];
        Array.Copy(path, 0, newPathPos, 1, path.Length);
        newPathPos[0] = newPathPos[1] + (newPathPos[1] - newPathPos[2]);
        newPathPos[newPathPos.Length - 1] = newPathPos[newPathPos.Length - 2] + (newPathPos[newPathPos.Length - 2] - newPathPos[newPathPos.Length - 3]);

        if (newPathPos[1] == newPathPos[newPathPos.Length - 2])
        {
            Vector3[] loopSpline = new Vector3[newPathPos.Length];
            Array.Copy(newPathPos, loopSpline, newPathPos.Length);
            loopSpline[0] = loopSpline[loopSpline.Length - 3];
            loopSpline[loopSpline.Length - 1] = loopSpline[2];
            newPathPos = loopSpline;
        }
        return newPathPos;
    }

    public static Vector3 Interpolate(Vector3[] paddedPath, float t)
    {
        int numSections = paddedPath.Length - 3;
        int currPt = Mathf.Min(Mathf.FloorToInt(t * numSections), numSections - 1);
        float u = t * numSections - currPt;
        Vector3 a = paddedPath[currPt];
        Vector3 b = paddedPath[currPt + 1];
        Vector3 c = paddedPath[currPt + 2];
        Vector3 d = paddedPath[currPt + 3];
        return 0.5f * ((-a + 3f * b - 3f * c + d) * (u * u * u) + (2f * a - 5f * b + 4f * c - d) * (u * u) + (-a + c) * u + 2f * b);
    }

    public static Vector3 GetPointOnPath(Vector3[] rawPath, float t) => Interpolate(PadForCatmullRom(rawPath), t);
}
