using System.Collections;
using Components.GoBased;
using UnityEngine;

namespace Creatures.Mobs.Patrolling
{
    [RequireComponent(typeof(CreatureAttack), typeof(SpawnListComponent))]
    public class PointPatrol : Patrol
    {
        [SerializeField] private Transform[] _points;
        [SerializeField] private float _treshold = 0.6f;
        [SerializeField] private float _waitTime;

        private float _startSpeed;
        private int _zeroValue = 0;
        private int _destinationPointIndex;
        
        private int _nextPoint = 1;
        private CreatureMover _creatureMover;
        private SpawnListComponent _particles;
        private WaitForSeconds _waitTimeSeconds;

        private bool _isOnPoint => (_points[_destinationPointIndex].position - transform.position).magnitude < _treshold;

        private void Awake()
        {
            _creatureMover = GetComponent<CreatureMover>();
            _particles = GetComponent<SpawnListComponent>();
            _startSpeed = _creatureMover.Speed;
        }

        private void Start()
        {
            _waitTimeSeconds = new WaitForSeconds(_waitTime);
        }
        
        public override IEnumerator DoPatrol()
        {
            while (enabled)
            {
                if (_isOnPoint)
                {
                    _creatureMover.Speed = _zeroValue;
                    yield return _waitTimeSeconds;

                    _destinationPointIndex = (int) Mathf.Repeat(_destinationPointIndex + _nextPoint, _points.Length);
                    _creatureMover.Speed = _startSpeed;
                }
                
                var direction = _points[_destinationPointIndex].position - transform.position;
                direction.y = _zeroValue;
                _creatureMover.SetDirection(direction.normalized);

                yield return null;
            }
        }
    }
}