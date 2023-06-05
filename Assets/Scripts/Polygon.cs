using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a polygon or polyline constructed from control points on the X-Z plane.
/// </summary>
public class Polygon
{
    private struct Curve
    {
        public Vector3[] controls;

        public void SetControls(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            controls = new Vector3[4] { p0, p1, p2, p3 };
        }

        public Vector3 SampleCurve(float t)
        {
            t = Mathf.Clamp01(t);

            Vector3 p0p1 = Vector3.Lerp(controls[0], controls[1], t);
            Vector3 p1p2 = Vector3.Lerp(controls[1], controls[2], t);
            Vector3 p2p3 = Vector3.Lerp(controls[2], controls[3], t);

            Vector3 p0p1p1p2 = Vector3.Lerp(p0p1, p1p2, t);
            Vector3 p1p2p2p3 = Vector3.Lerp(p1p2, p2p3, t);


            return Vector3.Lerp(p0p1p1p2, p1p2p2p3, t);
        }

        public Vector3[] SampleCurve(int numPoints)
        {
            float step = 1f / (numPoints - 1);
            Vector3[] points = new Vector3[numPoints];

            int i = 0;
            for (float t = 0f; t <= 1f - Mathf.Epsilon; t += step)
            {
                points[i] = SampleCurve(t);
                i++;
            }

            return points;
        }

        public void DrawDebug(int res, float y, bool drawControls, bool drawCurve)
        {
            float step = 1f / res;
            Vector3 offsetVector = new Vector3(0f, y, 0f);

            if (drawControls)
            {
                for (int i = 0; i < controls.Length - 1; i++)
                    Debug.DrawLine(controls[i] + offsetVector, controls[i + 1] + offsetVector, Color.yellow);
            }

            if (drawCurve)
            {
                for (float t = 0f; t < 1f; t += step)
                    Debug.DrawLine(SampleCurve(t) + offsetVector, SampleCurve(t + step) + offsetVector, Color.magenta);
            }
        }
    }

    public enum Axis
    {
        X, Y, Z
    }

    public enum UnitType
    {
        DISTANCE,
        INDEX,
        NORMALISED
    }

    public Vector3[] ControlPositions { get; private set; }

    public Vector3[] ControlNormals { get; private set; }
    public Vector3[] ControlTangents { get; private set; }

    public Vector3[] CurvePositions { get; private set; }
    public Vector3[] CurveNormals { get; private set; }
    public Vector3[] CurveTangents { get; private set; }

    private bool looped;

    private float[] controlDistancesCache;

    public Polygon(Vector3[] controlPositions, bool looped, float bezierHandleLength = 0.25f, int bezierResolution = 6)
    {
        this.looped = looped;

        //for (int i = 0; i < controlPositions.Length; i++)
        //    controlPositions[i].y = 0;

        //Compute the control data
        ControlPositions = controlPositions;
        ControlNormals = ComputeNormals(controlPositions, looped);
        ControlTangents = ComputeTangents(controlPositions, looped);

        if (bezierResolution > 0)
            RecalculateBezierSpline(bezierHandleLength, bezierResolution);

        //Cache the individual point distances
        float distanceSum = 0;
        controlDistancesCache = new float[ControlPositions.Length];
        for (int i = 0; i < ControlPositions.Length - 1; i++)
        {
            controlDistancesCache[i] = distanceSum;
            distanceSum += Vector3.Distance(controlPositions[i], controlPositions[i + 1]);
        }

        controlDistancesCache[controlPositions.Length - 1] = distanceSum;

    }

    #region Public Methods

    /// <summary>
    /// Take in a set of points making up a 2D polygon on the X and Z axis and from them creates a grid bounds that matches the inputs shape
    /// </summary>
    /// <param name="polygon">An array of vectors forming a polygon on the X-Z axis</param>
    /// <param name="pixelSize">Size of the square to use when splitting</param>
    /// <returns>An array of bounds matching the input shape</returns>
    public Bounds[] Rasterise(float pixelSize)
    {
        if (ControlPositions == null || pixelSize == 0)
            return null;

        Bounds polygonBound = GetBounds();

        //Calculate the size of each pixel and the number of pixels on each axis
        Vector3 realPixelSize = new Vector3(
            polygonBound.size.x / Mathf.Ceil(polygonBound.size.x / pixelSize),
            0,
            polygonBound.size.z / Mathf.Ceil(polygonBound.size.z / pixelSize));

        Vector3Int pixelCounts = new Vector3Int(
            Mathf.CeilToInt(polygonBound.size.x / realPixelSize.x),
            0,
            Mathf.CeilToInt(polygonBound.size.z / realPixelSize.z));

        List<Bounds> outputBounds = new List<Bounds>();

        //Check if each pixel is inside the polygon and add it to the output if so
        for (int z = 0; z < pixelCounts.z; z++)
        {
            for (int x = 0; x < pixelCounts.x; x++)
            {
                Vector3 pixelPoint = polygonBound.min + new Vector3(realPixelSize.x * x + (realPixelSize.x / 2), 0, realPixelSize.z * z + (realPixelSize.z / 2));

                if (IsPointInsidePolygon(pixelPoint))
                    outputBounds.Add(new Bounds(pixelPoint, realPixelSize));
            }
        }

        return outputBounds.ToArray();
    }

    /// <summary>
    /// Calculates the axis aligned bounds of a 2D polygon on the X-Z axis
    /// </summary>
    /// <param name="polygon">An array of vectors forming a polygon on the X-Z axis</param>
    /// <returns>The bounds of the polygon</returns>
    public Bounds GetBounds()
    {
        if (ControlPositions == null || ControlPositions.Length == 0)
            return default;

        Bounds polygonBounds = new Bounds(ControlPositions[0], Vector3.zero);
        for (int p = 0; p < ControlPositions.Length; p++)
            polygonBounds.Encapsulate(ControlPositions[p]);

        return polygonBounds;
    }

    /// <summary>
    /// Checks if a point is inside a 2D polygon on the X-Z axis
    /// </summary>
    /// <param name="polygon">An array of vectors forming a polygon on the X-Z axis</param>
    /// <param name="point">The point to check</param>
    /// <returns></returns>
    private bool IsPointInsidePolygon(Vector3 point)
    {
        if (ControlPositions == null || ControlPositions.Length == 0 || looped == false)
            return false;

        Bounds polygonBounds = GetBounds();
        Vector3 pointEnd = point + (Vector3.forward * (polygonBounds.size.z + 1f));
        int intersectionCount = 0;

        //Count the number of intersection with the polygon
        for (int i = 0; i < ControlPositions.Length; i++)
        {
            if (DoLinesIntersect(point, pointEnd, ControlPositions[i], ControlPositions[(i + 1) % ControlPositions.Length]) == true)
            {
                intersectionCount++;
            }
        }

        return intersectionCount % 2 == 1;
    }

    /// <summary>
    /// Get the centre point of a polygon
    /// </summary>
    /// <param name="polygon">An array of vectors forming a polygon on the X-Z axis</param>
    /// <returns>The centre point of the polygon</returns>
    public Vector3 GetCentre()
    {
        if (ControlPositions == null || ControlPositions.Length == 0)
            return Vector3.zero;

        Vector3 centerPoint = Vector3.zero;
        for (int p = 0; p < ControlPositions.Length; p++)
        {
            centerPoint += ControlPositions[p] / ControlPositions.Length;
        }

        return centerPoint;
    }

    /// <summary>
    /// Get a point on the perimeter of the polygon
    /// </summary>
    /// <param name="polygon">An array of vectors forming a polygon on the X-Z axis</param>
    /// <param name="t">The amount of units along the poly line</param>
    /// <param name="units">The type of unit passed</param>
    /// <returns>Point on the perimeter of the polygon</returns>
    public Vector3 GetPositionOnEdge(float t, UnitType units, bool useCurve = false)
    {
        if (useCurve && CurvePositions == null)
            RecalculateBezierSpline(0.25f, 6);

        Vector3[] positions = useCurve ? CurvePositions : ControlPositions;

        //TODO - Pre compute each control positions distance along the edge

        if (positions == null)
            return Vector3.zero;
        else if (positions.Length == 1)
            return positions[0];

        float edgeLength = GetEdgeLength();

        switch (units)
        {
            case UnitType.DISTANCE:
                if (t <= 0)
                    return positions[0];
                else if (t >= edgeLength)
                    return positions[positions.Length - 1];

                int vertexIndex = 0;
                while (t > Vector3.Distance(positions[vertexIndex], positions[vertexIndex + 1]))
                {
                    t -= Vector3.Distance(positions[vertexIndex], positions[vertexIndex + 1]);
                    vertexIndex++;
                }

                return Vector3.Lerp(positions[vertexIndex], positions[vertexIndex + 1], t / Vector3.Distance(positions[vertexIndex], positions[vertexIndex + 1]));
            case UnitType.INDEX:
                if (t <= 0)
                    return positions[0];
                else if (t >= edgeLength)
                    return positions[positions.Length - 1];

                int basePoint = Mathf.FloorToInt(t);
                float interpolation = t - basePoint;

                if (interpolation == 0)
                    return positions[basePoint];
                else
                    return Vector3.Lerp(positions[basePoint], positions[basePoint + 1], interpolation);
            case UnitType.NORMALISED:
                return Vector3.zero;
            default:
                return Vector3.zero;
        }
    }

    public Vector3 GetTangentOnEdge(float t, UnitType units, bool useCurve = false)
    {
        if (useCurve && CurvePositions == null)
            RecalculateBezierSpline(0.25f, 6);

        Vector3[] positions = useCurve ? CurvePositions : ControlPositions;
        Vector3[] tangents = useCurve ? CurveTangents : ControlTangents;

        float edgeLength = GetEdgeLength();

        if (positions == null)
            return Vector3.zero;

        switch (units)
        {
            case UnitType.DISTANCE:
                if (t <= 0)
                    return tangents[0];
                else if (t >= edgeLength)
                    return tangents[positions.Length - 1];

                int posIndex = 0;
                while (t > Vector3.Distance(positions[posIndex], positions[posIndex + 1]))
                {
                    t -= Vector3.Distance(positions[posIndex], positions[posIndex + 1]);
                    posIndex++;
                }

                return Vector3.Lerp(tangents[posIndex], tangents[posIndex + 1], t / Vector3.Distance(positions[posIndex], positions[posIndex + 1]));
            case UnitType.INDEX:
                if (t <= 0)
                    return tangents[0];
                else if (t >= edgeLength)
                    return tangents[positions.Length - 1];

                int basePoint = Mathf.FloorToInt(t);
                float interpolation = t - basePoint;

                if (interpolation == 0)
                    return tangents[basePoint];
                else
                    return Vector3.Lerp(tangents[basePoint], tangents[basePoint + 1], interpolation);
            case UnitType.NORMALISED:
                return Vector3.zero;
            default:
                return Vector3.zero;
        }
    }

    /// <summary>
    /// Get a point on the perimeter of the polygon
    /// </summary>
    /// <param name="queryPos">The point to query the polygon from</param>
    /// <returns>Point on the perimeter of the polygon</returns>
    public Vector3 GetNearestPointOnEdge(Vector3 queryPos, bool useCurve = false)
    {
        return GetPositionOnEdge(GetDistanceAlongEdge(queryPos), UnitType.DISTANCE);
    }

    public float GetDistanceToEdge(Vector3 queryPos, bool useCurve = false)
    {
        Vector3 edgeDirection = GetPositionOnEdge(GetDistanceAlongEdge(queryPos), UnitType.DISTANCE) - queryPos;
        //edgeDirection.y = 0;
        return edgeDirection.magnitude;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="queryPos"></param>
    /// <returns></returns>
    public float GetDistanceAlongEdge(Vector3 queryPos, bool useCurve = false)
    {
        queryPos.z = 0;

        if (ControlPositions == null || ControlPositions.Length <= 1)
            return 0;

        //Find the nearest control position
        int nearestPointA = 0;
        float distToASqrd = float.MaxValue;

        for (int i = 0; i < ControlPositions.Length; i++)
        {
            float distToISqrd = (ControlPositions[i] - queryPos).sqrMagnitude;

            if (distToISqrd < distToASqrd)
            {
                nearestPointA = i;
                distToASqrd = distToISqrd;
            }
        }

        //Find the second nearest control position
        int nearestPointB = 1;
        float distToBSqrd = float.MaxValue;

        for (int i = 0; i < ControlPositions.Length; i++)
        {
            float distToISqrd = (ControlPositions[i] - queryPos).sqrMagnitude;
            if (distToISqrd < distToBSqrd && i != nearestPointA)
            {
                nearestPointB = i;
                distToBSqrd = distToISqrd;
            }
        }

        //Make sure position A is before position B
        if (nearestPointA > nearestPointB)
        {
            int temp = nearestPointA;
            nearestPointA = nearestPointB;
            nearestPointB = temp;
        }

        return controlDistancesCache[nearestPointA] + ComputeDistanceAlongLine(ControlPositions[nearestPointA], ControlPositions[nearestPointB], queryPos, false);
    }

    public int GetNextIndex(float t, UnitType units, bool useCurve = false)
    {
        if (useCurve && CurvePositions == null)
            RecalculateBezierSpline(0.25f, 6);

        Vector3[] positions = useCurve ? CurvePositions : ControlPositions;

        //TODO - Pre compute each control positions distance along the edge

        if (positions == null || positions.Length == 1)
            return 0;

        float edgeLength = GetEdgeLength();

        switch (units)
        {
            case UnitType.DISTANCE:

                if (t <= 0)
                    return 0;
                else if (t >= edgeLength)
                    return positions.Length - 1;

                int vertexIndex = 0;
                while (t > Vector3.Distance(positions[vertexIndex], positions[vertexIndex + 1]))
                {
                    t -= Vector3.Distance(positions[vertexIndex], positions[vertexIndex + 1]);
                    vertexIndex++;
                }

                return vertexIndex + 1;
            case UnitType.INDEX:
                return Mathf.Clamp(0, positions.Length - 1, Mathf.CeilToInt(t));
            case UnitType.NORMALISED:
                return 0;
            default:
                return 0;
        }
    }

    /// <summary>
    /// Get the total length of the polygons perimeter
    /// </summary>
    /// <returns>Length of the polygons perimeter</returns>
    public float GetEdgeLength()
    {
        return GetPerimeterLength(0, ControlPositions.Length - 1);
    }

    /// <summary>
    /// Get the length of the polygons perimeter between two indices
    /// </summary>
    /// <param name="start">Start index</param>
    /// <param name="end">End index</param>
    /// <returns>Length of the polygons perimeter between the start and end indices</returns>
    public float GetPerimeterLength(int start, int end)
    {
        //Clamp the indices to the paths points
        start = Mathf.Clamp(start, 0, ControlPositions.Length - 1);
        end = Mathf.Clamp(end, 0, ControlPositions.Length - 1);

        //Swap the indices if necessary
        if (start > end)
        {
            int temp = start;
            start = end;
            end = temp;
        }

        //Sum up the length
        float length = 0;
        for (int i = start; i < end; i++)
            length += Vector3.Distance(ControlPositions[i], ControlPositions[i + 1]);

        if (looped == true && start == 0 && end == ControlPositions.Length - 1)
            length += Vector3.Distance(ControlPositions[ControlPositions.Length - 1], ControlPositions[0]);

        return length;
    }

    private void RecalculateNormalsAndTangents()
    {
        ControlNormals = ComputeNormals(ControlPositions, looped);
        ControlTangents = ComputeTangents(ControlPositions, looped);
    }

    private void RecalculateBezierSpline(float handleLength, int resolution)
    {
        RecalculateNormalsAndTangents();

        List<Curve> curveSegments = new List<Curve>();

        for (int p = 0; p < ControlPositions.Length - 1; p++)
        {
            float pointDist = Vector3.Distance(ControlPositions[p], ControlPositions[p + 1]);

            Curve curve = new Curve();
            curve.SetControls(
                ControlPositions[p],
                ControlPositions[p] + (ControlTangents[p] * (pointDist * handleLength)),
                ControlPositions[p + 1] - (ControlTangents[p + 1] * (pointDist * handleLength)),
                ControlPositions[p + 1]
            );

            curveSegments.Add(curve);
        }

        for (int i = 0; i < curveSegments.Count; i++)
        {
            curveSegments[i].DrawDebug(resolution, 0, false, true);
        }

        List<Vector3> samplePoints = new List<Vector3>();
        for (int i = 0; i < curveSegments.Count; i++)
        {
            samplePoints.AddRange(curveSegments[i].SampleCurve(resolution));
        }

        for (int i = 0; i < samplePoints.Count - 1; i++)
        {
            if (samplePoints[i].Equals(samplePoints[i + 1]))
            {
                samplePoints.RemoveAt(i + 1);
            }
        }

        CurvePositions = samplePoints.ToArray();
        CurveNormals = ComputeNormals(CurvePositions, looped);
        CurveTangents = ComputeNormals(CurvePositions, looped);
    }

    /// <summary>
    /// Draw the polygon to the scene view, including normals and tangents
    /// </summary>
    /// <param name="rayLength">The length of the normal and tangent lines</param>
    /// <param name="verticalOffset">The offset to draw the line at on the Y axis</param>
    public void DrawDebugPolygon(float rayLength, float verticalOffset, float duration)
    {
        Vector3 offset = -Vector3.forward * verticalOffset;

        for (int p = 0; p < ControlPositions.Length - 1; p++)
            Debug.DrawLine(ControlPositions[p] + offset, ControlPositions[p + 1] + offset, Color.red, duration);

        if (looped && ControlPositions.Length >= 3)
            Debug.DrawLine(ControlPositions[ControlPositions.Length - 1] + offset, ControlPositions[0] + offset, Color.red, duration);

        RecalculateNormalsAndTangents();

        for (int n = 0; n < ControlNormals.Length; n++)
            Debug.DrawRay(ControlPositions[n] + offset, ControlNormals[n] * rayLength, Color.green, duration);

        for (int t = 0; t < ControlTangents.Length; t++)
            Debug.DrawRay(ControlPositions[t] + offset, ControlTangents[t] * rayLength, Color.blue, duration);
    }

    #endregion

    #region Utility Functions

    /// <summary>
    /// Checks if two lines intersect on the X-Z plane
    /// </summary>
    /// <param name="s1">Line 1 start</param>
    /// <param name="e1">Line 1 end</param>
    /// <param name="s2">Line 2 start</param>
    /// <param name="e2">Line 2 end</param>
    /// <returns>True if the lines intersect</returns>
    public static bool DoLinesIntersect(Vector3 s1, Vector3 e1, Vector3 s2, Vector3 e2)
    {
        int ori1 = ComputeOrientationOfTriangle(s1, e1, s2);
        int ori2 = ComputeOrientationOfTriangle(s1, e1, e2);
        int ori3 = ComputeOrientationOfTriangle(s2, e2, s1);
        int ori4 = ComputeOrientationOfTriangle(s2, e2, e1);

        if (ori1 != ori2 && ori3 != ori4)
            return true;
        else
            return false;
    }

    /// <summary>
    /// Get the orientation of the triangle on the X-Z plane looking down
    /// </summary>
    /// <param name="p1">Triangle point 1</param>
    /// <param name="p2">Triangle point 2</param>
    /// <param name="p3">Triangle point 3</param>
    /// <returns>0 for co-linear, 1 for clockwise, -1 for counter-clockwise</returns>
    public static int ComputeOrientationOfTriangle(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float orienation = (p2.z - p1.z) * (p3.x - p2.x) - (p2.x - p1.x) * (p3.z - p2.z);

        if (orienation == 0)
            return 0;
        else
            return orienation > 0 ? 1 : -1;
    }

    /// <summary>
    /// Calculated the distance a given point is along a line
    /// </summary>
    /// <param name="start">Start vector of the line</param>
    /// <param name="end">End vector of the line</param>
    /// <param name="query">The vector to query the line from</param>
    /// <param name="percentage">Should the distance along the line be a percentage</param>
    /// <returns>The distance along the line</returns>
    public static float ComputeDistanceAlongLine(Vector3 start, Vector3 end, Vector3 query, bool percentage)
    {
        start.z = end.z = query.z = 0;

        float startAngle = Vector3.Angle(end - start, query - start);
        float endAngle = Vector3.Angle(start - end, query - end);
        float lineLength = (end - start).magnitude;

        if (startAngle >= 90)
        {
            return 0;
        }
        else if (endAngle >= 90)
        {
            return percentage ? 1 : lineLength;
        }
        else
        {
            float angleRadians = startAngle * Mathf.Deg2Rad;

            float distanceToStart = (start - query).magnitude;
            float distanceToLine = Mathf.Sin(angleRadians) * distanceToStart;
            float distanceAlongLine = Mathf.Sqrt((distanceToStart * distanceToStart) - (distanceToLine * distanceToLine));

            if (percentage)
                return distanceAlongLine / lineLength;
            else
                return distanceAlongLine;
        }
    }

    public static Vector3 ComputePositionOnLine(Vector3 start, Vector3 end, Vector3 queryPosition)
    {
        return Vector3.Lerp(start, end, ComputeDistanceAlongLine(start, end, queryPosition, true));
    }

    public static float ComputeDistanceToLine(Vector3 start, Vector3 end, Vector3 query, bool sqrd)
    {
        start.y = end.y = query.y = 0;

        Vector3 startToEnd = end - start;
        Vector3 startToQuery = query - start;
        Vector3 endToQuery = query - end;

        if (Vector3.Dot(startToEnd, startToQuery) <= 0)
        {
            return sqrd ? startToQuery.sqrMagnitude : startToQuery.magnitude;
        }
        else if (Vector3.Dot(startToEnd, endToQuery) >= 0)
        {
            return sqrd ? endToQuery.sqrMagnitude : endToQuery.magnitude;
        }
        else
        {
            float a = Mathf.Abs(((end.x - start.x) * (start.z - query.z)) - ((start.x - query.x) * (end.z - start.z)));
            float b = Mathf.Sqrt(Mathf.Pow(end.x - start.x, 2) + Mathf.Pow(end.z - start.z, 2));

            return a / b;
        }
    }

    public static Vector3[] ComputeNormals(Vector3[] points, bool looped)
    {
        if (points == null || points.Length < 2)
            return new Vector3[0];

        if (looped)
        {
            Vector3[] pointNormals = new Vector3[points.Length];
            Vector3[] lineNormals = new Vector3[points.Length];

            for (int l = 0; l < lineNormals.Length; l++)
            {
                int nextPointIndex = (l + 1) % lineNormals.Length;
                Vector3 lineDirection = (points[nextPointIndex] - points[l]).normalized;
                lineNormals[l] = Vector3.Cross(-Vector3.forward, lineDirection);
            }

            for (int p = 0; p < pointNormals.Length; p++)
            {
                int prevLineIndex = p - 1 < 0 ? pointNormals.Length - 1 : p - 1;
                pointNormals[p] = (lineNormals[prevLineIndex] + lineNormals[p]).normalized;
            }

            return pointNormals;
        }
        else
        {
            Vector3[] pointNormals = new Vector3[points.Length];
            Vector3[] lineNormals = new Vector3[points.Length - 1];

            for (int l = 0; l < lineNormals.Length; l++)
                lineNormals[l] = Vector3.Cross(-Vector3.forward, (points[l + 1] - points[l]).normalized);

            for (int v = 1; v < pointNormals.Length - 1; v++)
                pointNormals[v] = (lineNormals[v - 1] + lineNormals[v]).normalized;

            pointNormals[0] = lineNormals[0];
            pointNormals[pointNormals.Length - 1] = lineNormals[lineNormals.Length - 1];

            return pointNormals;
        }
    }

    public static Vector3[] ComputeTangents(Vector3[] points, bool looped)
    {
        Vector3[] normals = ComputeNormals(points, looped);
        Vector3[] tangents = new Vector3[normals.Length];

        for (int n = 0; n < normals.Length; n++)
            tangents[n] = Vector3.Cross(normals[n], -Vector3.forward).normalized;

        return tangents;
    }

    public static Vector3[] OffsetPolyline(Vector3[] points, bool looped, float offset)
    {
        Vector3[] offsetPoints = new Vector3[points.Length];
        Vector3[] normals = ComputeNormals(points, looped);

        for (int i = 0; i < points.Length; i++)
        {
            offsetPoints[i] = points[i] + (normals[i] * offset);
        }

        return offsetPoints;
    }


    /// <summary>
    /// Get the distance between two vectors constrained to a single plane
    /// </summary>
    /// <param name="a">Start vector</param>
    /// <param name="b">End vector</param>
    /// <param name="ignoreAxis">The axis to ignore in the distance calculation</param>
    /// <param name="sqrd">Should the result be sqrd</param>
    /// <returns>Distance between the two projected vectors, squared if sqrd is set to true</returns>
    public static float FlatDistance(Vector3 a, Vector3 b, Axis ignoreAxis, bool sqrd)
    {
        Vector3 direction = b - a;

        switch (ignoreAxis)
        {
            case Axis.X:
                direction.x = 0;
                break;
            case Axis.Y:
                direction.y = 0;
                break;
            case Axis.Z:
                direction.z = 0;
                break;
            default:
                throw new InvalidOperationException("Projecting Onto Invalid Axis");
        }

        return sqrd ? direction.sqrMagnitude : direction.magnitude;
    }

    #endregion

}
