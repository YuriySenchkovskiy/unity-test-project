using System.Collections;
using Components.ColliderBased;
using Components.GoBased;
using Creatures.Mobs.Patrolling;
using UnityEngine;

namespace Creatures.Mobs
{
    [RequireComponent(typeof(Patrol), typeof(SpawnListComponent), typeof(Animator))]
    [RequireComponent(typeof(CreatureMover), typeof(CreatureAttack))]
    public class MobAI : MonoBehaviour
    {
        [SerializeField] private LayerCheck _isCanAttack;
        [SerializeField] private float _horizontalTrashold = 0.2f;
        [SerializeField] private float _alarmDelay = 0.5f;
        [SerializeField] private float _attackCooldown = 1f;
       
        [SerializeField] private float _missHeroCooldown = 0.5f;
        [SerializeField] private LayerCheck _vision;
        [SerializeField] private string _attack = "Attack";
        [SerializeField] private string _miss = "Miss";

        private static readonly int IsDeadKey = Animator.StringToHash("isDeadKey");
        
        private CreatureMover _creatureMover;
        private CreatureAttack _creatureAttack;
        private Animator _animator; 
        private GameObject _target; 
        
        private SpawnListComponent _particles;
        private bool _isDead;
        private Patrol _patrol;
        private Coroutine _currentCoroutine;
        
        private WaitForSeconds _attackWait;
        private WaitForSeconds _cooldownWait;
        private WaitForSeconds _alarmWait;
        
        private void Awake()
        {
            _creatureMover = GetComponent<CreatureMover>();
            _creatureAttack = GetComponent<CreatureAttack>();
            _animator = GetComponent<Animator>();
            _particles = GetComponent<SpawnListComponent>();
            _patrol = GetComponent<Patrol>();
        }

        private void Start()
        {
            StartState(_patrol.DoPatrol());
            _attackWait = new WaitForSeconds(_attackCooldown);
            _cooldownWait = new WaitForSeconds(_missHeroCooldown);
            _alarmWait = new WaitForSeconds(_alarmDelay); 
        }
        
        public void FindHeroInVision(GameObject go)
        {
            if (_isDead)
            {
                return;
            }
            _target = go; 
            
            StartState(AgroToHero()); 
        }
        
        public void Die()
        {
            _isDead = true;
            _animator.SetBool(IsDeadKey,true);
    
            if (_currentCoroutine != null)
            {
                StopCoroutine(_currentCoroutine);
            }
        }

        private void StartState(IEnumerator coroutine)
        {
            _creatureMover.SetDirection(Vector2.zero); 
            
            if (_currentCoroutine != null)
            {
                StopCoroutine(_currentCoroutine);
            }
            
            _currentCoroutine = StartCoroutine(coroutine);
        }

        private IEnumerator AgroToHero()
        {
            LookAtHero();
            yield return _alarmWait;
            StartState(GoToHero());
        }

        private void LookAtHero()
        {
            _creatureMover.SetDirection(Vector2.zero);
            var direction = GetDirectionToTarget();
            _creatureMover.UpdateSpriteDirection(direction);
        }

        private IEnumerator GoToHero()
        {
            while (_vision.IsTouchingLayer) 
            {
                if (_isCanAttack.IsTouchingLayer)
                {
                    StartState(Attack());
                }
                else
                {
                    var horizontalDelta = Mathf.Abs(_target.transform.position.x 
                                                    - transform.position.x);
                    if (horizontalDelta <= _horizontalTrashold)
                    {
                        _creatureMover.SetDirection(Vector2.zero);
                    }
                    else
                    {
                        SetDirectionToTarget();
                    }
                }
                
                yield return null;
            }
            
            _creatureMover.SetDirection(Vector2.zero);
            _particles.Spawn(_miss);
            yield return _cooldownWait;

            CheckHeroPosition();
        }
        
        private IEnumerator Attack()
        {
            while (_isCanAttack.IsTouchingLayer)
            {
                _creatureAttack.Attack();
                _particles.Spawn(_attack);
                yield return _attackWait;
            }

            StartState(GoToHero());
        }

        private void SetDirectionToTarget()
        {
            var direction = GetDirectionToTarget();
            _creatureMover.SetDirection(direction.normalized);
        }

        private void CheckHeroPosition()
        {
            StartState(_vision.IsTouchingLayer ? GoToHero() : _patrol.DoPatrol());
        }

        private Vector2 GetDirectionToTarget()
        {
            var direction = _target.transform.position - transform.position;
            direction.y = 0;
            return direction;
        }
    }
}