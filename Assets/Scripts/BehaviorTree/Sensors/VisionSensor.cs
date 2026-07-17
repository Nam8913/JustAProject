using UnityEngine;

namespace BehaviorTree
{
    public class VisionSensor : SensorBase
    {
        [SerializeField] private float _sightRange = 20f;
        public float SightRange => _sightRange;
        [SerializeField] private float _sightAngle = 90f;
        [SerializeField] private LayerMask _targetLayers;
        [SerializeField] private LayerMask _obstacleLayers;

        protected override void Sense()
        {
            var colliders = Physics.OverlapSphere(transform.position, _sightRange, _targetLayers);

            DefineThing closestThreat = null;
            float closestDist = float.MaxValue;

            for (int i = 0; i < colliders.Length; i++)
            {
                var dir = colliders[i].transform.position - transform.position;
                float angle = Vector3.Angle(transform.forward, dir);

                if (angle > _sightAngle * 0.5f)
                    continue;

                if (Physics.Raycast(transform.position, dir.normalized, dir.magnitude, _obstacleLayers))
                    continue;

                var creature = colliders[i].GetComponent<Creature>();
                if (creature == null)
                    continue;

                float dist = dir.sqrMagnitude;
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestThreat = creature; // Creature inherits from DefineThing
                }
            }

            if (closestThreat != null)
            {
                Blackboard.Set(BBKeys.CanSeeEnemy, true);
                Blackboard.Set(BBKeys.ThreatTarget, closestThreat);
                Blackboard.Set(BBKeys.LastKnownEnemyPosition, closestThreat.transform.position);
            }
            else
            {
                Blackboard.Set(BBKeys.CanSeeEnemy, false);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _sightRange);

            Vector3 leftDir = Quaternion.Euler(0, -_sightAngle * 0.5f, 0) * transform.forward;
            Vector3 rightDir = Quaternion.Euler(0, _sightAngle * 0.5f, 0) * transform.forward;

            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position, leftDir * _sightRange);
            Gizmos.DrawRay(transform.position, rightDir * _sightRange);
        }
    }
}
