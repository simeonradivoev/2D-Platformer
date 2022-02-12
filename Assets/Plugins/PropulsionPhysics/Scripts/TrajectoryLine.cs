using UnityEngine;
using System.Collections;

namespace Polycrime
{
    public static class TrajectoryLine
    {
        public static void Render(Vector3 startPoint, Vector3 endPoint, float time, Color color)
        {
            Vector3 initialVelocity = TrajectoryMath.CalculateVelocity(startPoint, endPoint, time);
            float deltaTime = time / initialVelocity.magnitude;
            int drawSteps = (int)(initialVelocity.magnitude - 0.5f)+1;
            Vector3 currentPosition = startPoint;
            Vector3 previousPosition = currentPosition;
            Gizmos.color = color;

            if (IsParabolicVelocity(initialVelocity))
            {
                for (int i = 0; i < drawSteps; i++)
                {
                    currentPosition += (initialVelocity * deltaTime) + (0.5f * Physics.gravity * deltaTime * deltaTime);
                    initialVelocity += Physics.gravity * deltaTime;
                    Gizmos.DrawLine(previousPosition, currentPosition);

                    //////////////////////////////////////////////////////////////////////////////////
                    // If the next loop is the last iteration, then don't update the previous position
                    // vector so it can be used to draw the gizmos arrow.
                    if ((i + 1) < drawSteps)
                    {
                        previousPosition = currentPosition;
                    }
                }
                DrawArrow(previousPosition, (currentPosition - previousPosition));
            }
            else
            {
                Vector3 newUpDirection = new Vector3(currentPosition.x, endPoint.y, currentPosition.z);
                Gizmos.DrawLine(currentPosition, newUpDirection);
                DrawArrow(newUpDirection, new Vector3(0f, 0.01f, 0f));
            }
        }

        private static void DrawArrow(Vector3 position, Vector3 direction)
        {
            int[] arrowAngles = new int[] { 225, 135 };

            foreach (int angle in arrowAngles)
            {
                Vector3 endPoint = Quaternion.LookRotation(direction) * Quaternion.Euler(0, angle, 0) * Vector3.forward;
                Gizmos.DrawRay(position + direction, endPoint * 0.5f);
            }
        }

        private static bool IsParabolicVelocity(Vector3 velocity)
        {
            return !(velocity.x == 0 && velocity.z == 0);
        }
    }
}
