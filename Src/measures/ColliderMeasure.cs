using UnityEngine;

namespace com.github.lhervier.ksp.rockprecisionfixdiag.measures
{
    /// <summary>
    /// Where the collider of one rock stands, as the physics engine holds it, against the rock as it is
    /// drawn. Stock rocks have no collider; a mod can give them one. Heights are distances from the
    /// centre of the body, in metres.
    /// </summary>
    internal class ColliderMeasure
    {
        /// <summary>Height of the centre of the collider, where the physics engine holds it.</summary>
        public double PhysicsHeightM;

        /// <summary>Height of the same centre, where the rock is drawn.</summary>
        public double DrawnHeightM;

        /// <summary>
        /// The centre where the physics engine holds it minus the centre where it is drawn, along the vertical
        /// of the quad, in millimetres. Positive: the collider stands above the rock one sees.
        /// </summary>
        public double UpMm;

        /// <summary>The same gap, what is left once the vertical part is removed, in millimetres.</summary>
        public double AcrossMm;

        /// <summary>
        /// Measures the collider carried by one rock of a holder, or returns null when that rock has no
        /// collider to measure. <paramref name="rockObject"/> is the object of the rock under its holder, and
        /// <paramref name="drawnToWorld"/> the matrix its holder is drawn from.
        /// </summary>
        public static ColliderMeasure Take(Transform rockObject, Matrix4x4 drawnToWorld, PQ quad, CelestialBody body)
        {
            MeshCollider collider = rockObject.GetComponent<MeshCollider>();
            if (collider == null || collider.sharedMesh == null || !collider.enabled
                || !rockObject.gameObject.activeInHierarchy)
            {
                return null;
            }

            // Both centres are the centre of the same box, the one the collider mesh spans, so they can be
            // compared even though the physics engine widens that box to hold it once turned. The drawn one
            // goes through the matrix the rock is drawn from and the pose of the rock under its holder, read
            // from the transform itself: whatever the engine was given, this is where the rock is seen.
            Vector3d physics = collider.bounds.center;
            Matrix4x4 rockToWorld = drawnToWorld * Matrix4x4.TRS(
                rockObject.localPosition, rockObject.localRotation, rockObject.localScale);
            Vector3d drawn = rockToWorld.MultiplyPoint3x4(collider.sharedMesh.bounds.center);

            Vector3d gap = physics - drawn;
            Vector3d up = ((Vector3d)quad.transform.position - body.position).normalized;
            double upM = Vector3d.Dot(gap, up);
            return new ColliderMeasure
            {
                PhysicsHeightM = HeightUtils.HeightOf(physics, body),
                DrawnHeightM = HeightUtils.HeightOf(drawn, body),
                UpMm = upM * 1000.0,
                AcrossMm = (gap - up * upM).magnitude * 1000.0
            };
        }

        /// <summary>Adds the collider to a record, on a line of its own that names the rock it belongs to.</summary>
        public void Log(RecordLog log, RockMeasure rock)
        {
            log.Line(1, "rock '{0}' '{1}' #{2} collider: physics {3}, drawn {4}, up {5} mm, across {6} mm",
                rock.QuadName, rock.ScatterName, rock.Index, FormatUtils.FormatHeight(PhysicsHeightM),
                FormatUtils.FormatHeight(DrawnHeightM), FormatUtils.FormatSigned(UpMm),
                FormatUtils.Format(AcrossMm));
        }
    }
}
