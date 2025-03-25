using System;

namespace KenshiMultiplayerLoader.MODELS
{
    public class Position
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; } // Added Z coordinate for proper 3D positioning
        public float RotationX { get; set; } // Rotation around X axis
        public float RotationY { get; set; } // Rotation around Y axis
        public float RotationZ { get; set; } // Rotation around Z axis (yaw)
        public long Timestamp { get; set; } // For synchronization and interpolation

        // Default constructor
        public Position()
        {
            X = 0;
            Y = 0;
            Z = 0;
            RotationX = 0;
            RotationY = 0;
            RotationZ = 0;
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        // Constructor with position parameters
        public Position(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
            RotationX = 0;
            RotationY = 0;
            RotationZ = 0;
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        // Constructor with position and rotation parameters
        public Position(float x, float y, float z, float rotX, float rotY, float rotZ)
        {
            X = x;
            Y = y;
            Z = z;
            RotationX = rotX;
            RotationY = rotY;
            RotationZ = rotZ;
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        // Full constructor with timestamp
        public Position(float x, float y, float z, float rotX, float rotY, float rotZ, long timestamp)
        {
            X = x;
            Y = y;
            Z = z;
            RotationX = rotX;
            RotationY = rotY;
            RotationZ = rotZ;
            Timestamp = timestamp;
        }

        // Calculate distance between positions (ignoring rotation)
        public float DistanceTo(Position other)
        {
            return (float)Math.Sqrt(
                Math.Pow(X - other.X, 2) +
                Math.Pow(Y - other.Y, 2) +
                Math.Pow(Z - other.Z, 2)
            );
        }

        // Linear interpolation between two positions for smooth movement
        public static Position Lerp(Position start, Position end, float factor)
        {
            factor = Math.Clamp(factor, 0.0f, 1.0f);

            return new Position(
                start.X + (end.X - start.X) * factor,
                start.Y + (end.Y - start.Y) * factor,
                start.Z + (end.Z - start.Z) * factor,
                start.RotationX + (end.RotationX - start.RotationX) * factor,
                start.RotationY + (end.RotationY - start.RotationY) * factor,
                start.RotationZ + (end.RotationZ - start.RotationZ) * factor,
                end.Timestamp
            );
        }

        // Override ToString for easier debugging
        public override string ToString()
        {
            return $"Position(X:{X:F2}, Y:{Y:F2}, Z:{Z:F2}, Rot:{RotationZ:F1}°)";
        }
    }
}