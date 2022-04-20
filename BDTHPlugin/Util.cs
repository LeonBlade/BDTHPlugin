using System;
using System.Numerics;

namespace BDTHPlugin
{
  class Util
  {
    private const float Deg2Rad = (float)(Math.PI * 2) / 360;
    private const float Rad2Deg = (float)(360 / (Math.PI * 2));

    public static Quaternion ToQ(Vector3 euler)
    {
      var xOver2 = euler.X * Deg2Rad * 0.5f;
      var yOver2 = euler.Y * Deg2Rad * 0.5f;
      var zOver2 = euler.Z * Deg2Rad * 0.5f;

      var sinXOver2 = (float)Math.Sin(xOver2);
      var cosXOver2 = (float)Math.Cos(xOver2);
      var sinYOver2 = (float)Math.Sin(yOver2);
      var cosYOver2 = (float)Math.Cos(yOver2);
      var sinZOver2 = (float)Math.Sin(zOver2);
      var cosZOver2 = (float)Math.Cos(zOver2);

      Quaternion result;
      result.X = cosYOver2 * sinXOver2 * cosZOver2 + sinYOver2 * cosXOver2 * sinZOver2;
      result.Y = sinYOver2 * cosXOver2 * cosZOver2 - cosYOver2 * sinXOver2 * sinZOver2;
      result.Z = cosYOver2 * cosXOver2 * sinZOver2 - sinYOver2 * sinXOver2 * cosZOver2;
      result.W = cosYOver2 * cosXOver2 * cosZOver2 + sinYOver2 * sinXOver2 * sinZOver2;
      return result;
    }

    public static Vector3 FromQ(Quaternion q2)
    {
      Quaternion q = new Quaternion(q2.W, q2.Z, q2.X, q2.Y);
      Vector3 pitchYawRoll = new Vector3
      {
        Y = (float)Math.Atan2(2f * q.X * q.W + 2f * q.Y * q.Z, 1 - 2f * (q.Z * q.Z + q.W * q.W)),
        X = (float)Math.Asin(2f * (q.X * q.Z - q.W * q.Y)),
        Z = (float)Math.Atan2(2f * q.X * q.Y + 2f * q.Z * q.W, 1 - 2f * (q.Y * q.Y + q.Z * q.Z))
      };

      pitchYawRoll.X *= Rad2Deg;
      pitchYawRoll.Y *= Rad2Deg;
      pitchYawRoll.Z *= Rad2Deg;

      return pitchYawRoll;
    }

    public static float DistanceFromPlayer(HousingGameObject obj, Vector3 playerPos)
      => Vector3.Distance(playerPos, new(obj.X, obj.Y, obj.Z));
  }

  public static class QuaternionExtensions
  {
    public static float ComputeXAngle(this Quaternion q)
    {
      float sinr_cosp = 2 * (q.W * q.X + q.Y * q.Z);
      float cosr_cosp = 1 - 2 * (q.X * q.X + q.Y * q.Y);
      return (float)Math.Atan2(sinr_cosp, cosr_cosp);
    }

    public static float ComputeYAngle(this Quaternion q)
    {
      float sinp = 2 * (q.W * q.Y - q.Z * q.X);
      if (Math.Abs(sinp) >= 1)
        return (float)Math.PI / 2 * Math.Sign(sinp); // use 90 degrees if out of range
      else
        return (float)Math.Asin(sinp);
    }

    public static float ComputeZAngle(this Quaternion q)
    {
      float siny_cosp = 2 * (q.W * q.Z + q.X * q.Y);
      float cosy_cosp = 1 - 2 * (q.Y * q.Y + q.Z * q.Z);
      return (float)Math.Atan2(siny_cosp, cosy_cosp);
    }

    public static Vector3 ComputeAngles(this Quaternion q)
    {
      return new Vector3(ComputeXAngle(q), ComputeYAngle(q), ComputeZAngle(q));
    }

    public static Quaternion FromAngles(Vector3 angles)
    {

      float cy = (float)Math.Cos(angles.Z * 0.5f);
      float sy = (float)Math.Sin(angles.Z * 0.5f);
      float cp = (float)Math.Cos(angles.Y * 0.5f);
      float sp = (float)Math.Sin(angles.Y * 0.5f);
      float cr = (float)Math.Cos(angles.X * 0.5f);
      float sr = (float)Math.Sin(angles.X * 0.5f);

      Quaternion q = new Quaternion
      {
        W = cr * cp * cy + sr * sp * sy,
        X = sr * cp * cy - cr * sp * sy,
        Y = cr * sp * cy + sr * cp * sy,
        Z = cr * cp * sy - sr * sp * cy
      };

      return q;

    }
  }
}
