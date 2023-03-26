using UnityEngine;

public struct RotationChanges
{
    public Vector3 TurnAddition { get; }

    public float PitchAdditionInDegrees { get; }

    public RotationChanges(Vector3 turnAddition, float pitchAdditionInDegrees)
    {
        TurnAddition = turnAddition;
        PitchAdditionInDegrees = pitchAdditionInDegrees;
    }

    public static RotationChanges Empty => new RotationChanges();
}

public class Aimbot : MonoBehaviour
{
    private const string Tag = "Enemy";

    public float aimAssistRadius = 2f;
    public float timeToAim = 0.1f;

    public RotationChanges TrackTarget()
    {
        var target = VehicleCamera.ClosestEnemy;

        // if we didn't find a target, just return an empty result that does not change the rotations
        if (!target)
        {
            Debug.Log("No Target");
            return RotationChanges.Empty;
        }
        Debug.Log("We have a Target");
        var targetPos = target.position;
        var totalHorizontalRotationAngles = CalculateTotalRotationAngles(Vector3.up, targetPos);
        var totalVerticalRotationAngles = CalculateTotalRotationAngles(Camera.main.transform.right, targetPos);

        var dx = CalculateDeltaRotationDegrees(totalVerticalRotationAngles, timeToAim, Time.deltaTime, targetPos);
        var dy = CalculateDeltaRotationDegrees(totalHorizontalRotationAngles, timeToAim, Time.deltaTime, targetPos);

        // return the pitch addition in degrees and the turn addition in euler angles (rotated along Y axis)
        return new RotationChanges(pitchAdditionInDegrees: dx, turnAddition: dy * Vector3.up);
    }

    private Transform SelectTarget()
    {
        // find start point and direction for the spherecast
        var direction = Camera.main.transform.forward;
        var startPoint = Camera.main.transform.position;

        // shoot the spherecast
        if (!Physics.SphereCast(startPoint, aimAssistRadius, direction, out var hit, 1000f))
        {
            // if we didn't hit anything then we won't do any aimbotting
            return null;
        }

        // we found a target only if the collider we hit is tagged as 'enemy'
        return hit.collider.CompareTag(Tag) ? hit.collider.transform : null;
    }

    private float CalculateDeltaRotationDegrees(float totalRotation, float time, float deltaTime, Vector3 target)
    {
        // normalize for the aim assist radius or a short radius will have slower aim speed
        var adjustedTimeToAim = time * aimAssistRadius;

        // get the distance from player to target
        var distance = (target - Camera.main.transform.position).magnitude;

        // calculate the angle from its opposite and adjacent sides and divide by time to get angular velocity
        var angularVelocity = Mathf.Atan2(1f, distance) * Mathf.Rad2Deg / adjustedTimeToAim;

        // to avoid overshoot, if we are closer to aiming at the target than one step, we just snap to it
        return Mathf.Min(angularVelocity * deltaTime, Mathf.Abs(totalRotation)) * Mathf.Sign(totalRotation);
    }

    private float CalculateTotalRotationAngles(Vector3 planeNormal, Vector3 target)
    {
        // project to a plane defined by its normal vector, for both the actual and desired look directions
        var camForwardProjected = Vector3.ProjectOnPlane(Camera.main.transform.forward, planeNormal);
        var playerToTargetProjected = Vector3.ProjectOnPlane((target - Camera.main.transform.position).normalized, planeNormal);

        // calculate the signed angle between the actual and desired look directions
        return Vector3.SignedAngle(camForwardProjected, playerToTargetProjected, planeNormal);
    }
}