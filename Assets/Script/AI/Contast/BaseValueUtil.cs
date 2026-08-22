using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BaseValueUtil
{
    public static Vector2Int RelPositionTransform(Vector2 transform, Vector2 origin)
    {
        return new Vector2Int((int)transform.x - (int)origin.x, (int)transform.y - (int)origin.y);
    }







    public static Vector2Int PointRotateWithOrign(float angle, Vector2Int point)
    {
        Vector3 target = Quaternion.AngleAxis(angle, Vector3.forward).normalized * (Vector3Int)point;
        return Vector2Int.RoundToInt(target);
    }







    public static Vector2 PointRotateWithOrignF(float angle, Vector2 point)
    {
        return Quaternion.AngleAxis(angle, Vector3.forward).normalized * point;
    }







    public static List<Vector2Int> GetCircleRange(Vector2Int origin, int radius)
    {
        var positions = new List<Vector2Int>();
        for (int i = -radius; i <= radius; i++)
        {
            for (int j = -radius; j <= radius; j++)
            {
                if (i * i + j * j <= radius * radius)
                {
                    positions.Add(new Vector2Int(origin.x + i, origin.y + j));
                }
            }
        }
        return positions;
    }






    public static Quaternion GetQuaternion(Vector3 direction)
    {
        return Quaternion.Euler(new Vector3(0, 0, Vector3.Angle(Vector3.right, direction) * Mathf.Sign(direction.y)));
    }





    public static Quaternion GetQuaternion(Vector3 direction, Vector3 origin)
    {
        return Quaternion.Euler(new Vector3(0, 0, Vector3.Angle(origin, direction) * Mathf.Sign(direction.y)));
    }






    public static float VectorAngle(Vector2 from, Vector2 to)
    {
        float angle;

        Vector3 cross = Vector3.Cross(from, to);
        angle = Vector2.Angle(from, to);
        return cross.z > 0 ? -angle : angle;
    }



    public static float GetCollisionStrength(this Rigidbody2D rigidbody, Collision2D collision)
    {
        return Vector2.Dot(collision.GetContact(0).normal, collision.relativeVelocity) * rigidbody.mass;
    }






    public static TimeSpan DebugTimer(Action action)
    {
        var startTime = DateTime.Now;
        action?.Invoke();
        return DateTime.Now - startTime;
    }





    public static void CaptureScreenshot(string path)
    {




        int resWidth = Screen.width;
        int resHeight = Screen.height;


        Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);


        var timeScale = Time.timeScale;
        Time.timeScale = 0;
        RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
        Camera.main.targetTexture = rt;
        RenderTexture.active = rt;
        Camera.main.Render();
        screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
        Camera.main.targetTexture = null;
        RenderTexture.active = null;
        UnityEngine.Object.Destroy(rt);
        Time.timeScale = timeScale;


        byte[] bytes = screenShot.EncodeToPNG();
        System.IO.File.WriteAllBytes(path, bytes);
    }
    private const float precision = 0.1f;





    public static float Round(float x)
    {
        return Mathf.Round(x / precision) * precision;
    }





    public static Vector2 Round(Vector2 pos)
    {
        return new Vector2(Round(pos.x), Round(pos.y));
    }

}
