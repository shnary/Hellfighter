
using OpenTK.Mathematics;

namespace Hellfighter;
public class Camera {

    public Vector3 Position { get; set; }
    public float AspectRatio { get; set; }

    private Vector3 _front;
    private Vector3 _up;
    private Vector3 _right;

    private float _pitch;
    private float _yaw;

    private float _fov;

    // We convert from degrees to radians as soon as the property is set to improve performance.
    public float Pitch {
        get => MathHelper.RadiansToDegrees(_pitch);
        set {
            // We clamp the pitch value between -89 and 89 to prevent the camera from going upside down, and a bunch
            // of weird "bugs" when you are using euler angles for rotation.
            // If you want to read more about this you can try researching a topic called gimbal lock
            var angle = MathHelper.Clamp(value, -89f, 89f);
            _pitch = MathHelper.DegreesToRadians(angle);
            UpdateVectors();
        }
    }

    // We convert from degrees to radians as soon as the property is set to improve performance.
    public float Yaw {
        get => MathHelper.RadiansToDegrees(_yaw);
        set {
            _yaw = MathHelper.DegreesToRadians(value);
            UpdateVectors();
        }
    }

    // The field of view (FOV) is the vertical angle of the camera view.
    // This has been discussed more in depth in a previous tutorial,
    // but in this tutorial, you have also learned how we can use this to simulate a zoom feature.
    // We convert from degrees to radians as soon as the property is set to improve performance.
    public float Fov {
        get => MathHelper.RadiansToDegrees(_fov);
        set {
            var angle = MathHelper.Clamp(value, 1f, 90f);
            _fov = MathHelper.DegreesToRadians(angle);
        }
    }

    public Vector3 Front => _front;

    public Vector3 Up => _up;

    public Vector3 Right => _right;

    public Camera(Vector3 position, float aspectRatio) {
        Position = position;
        AspectRatio = aspectRatio;

        _front = new Vector3(0, 0, -1);
        _up = new Vector3(0, 1, 0);
        _right = new Vector3(1, 0, 0);
        Fov = 45;

        _yaw = -MathHelper.PiOver2;
    }

    public Matrix4 GetViewMatrix() {
        return Matrix4.LookAt(Position, Position + _front, _up);
    }

    public Matrix4 GetProjectionMatrix() {
        return Matrix4.CreatePerspectiveFieldOfView(_fov, AspectRatio, 0.01f, 100f);
    }

    private void UpdateVectors() {
        // First, the front matrix is calculated using some basic trigonometry.
        _front.X = MathF.Cos(_pitch) * MathF.Cos(_yaw);
        _front.Y = MathF.Sin(_pitch);
        _front.Z = MathF.Cos(_pitch) * MathF.Sin(_yaw);

        // We need to make sure the vectors are all normalized, as otherwise we would get some funky results.
        _front = Vector3.Normalize(_front);

        // Calculate both the right and the up vector using cross product.
        // Note that we are calculating the right from the global up; this behaviour might
        // not be what you need for all cameras so keep this in mind if you do not want a FPS camera.
        _right = Vector3.Normalize(Vector3.Cross(_front, Vector3.UnitY));
        _up = Vector3.Normalize(Vector3.Cross(_right, _front));
    }
}
