using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace TestCar.Core 
{
    public static class ZombieEx
    {
        [Serializable]
        public struct RandomRange
        {
            [SerializeField] private int _minValue;
            [SerializeField] private int _maxValue;

            public int Value => Random.Range(_minValue, _maxValue);
        }

        private const string STUMBLING_TAG = "Stumbling";
        public static bool IsStumbling(this Zombie zombie) =>
            !zombie.Anim.enabled || 
            zombie.Anim.GetCurrentAnimatorStateInfo(0).IsTag(STUMBLING_TAG) ||
            zombie.Anim.GetNextAnimatorStateInfo(0).IsTag(STUMBLING_TAG);
    }

    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(NavMeshAgent))]
    public class Zombie : MonoBehaviour
    {
        private static Dictionary<int, Zombie> _zombies = new ();
        public static Dictionary<int, Zombie> Zombies => _zombies;

        private const string STUMBLING_ANIM_PARAM = "Stumbling";
        private const string IDLE_ANIM_PARAM = "Idle";
        private const string STAND_UP_ANIM_PARAM = "StandUp";
        private const string WALK_ANIM_PARAM = "Walk";

        [SerializeField] private GameObject[] _skins;

        [SerializeField] private ZombieEx.RandomRange _healthRange;
        [SerializeField] private ZombieEx.RandomRange _damageRange;
        [SerializeField] private ZombieEx.RandomRange _idleAnimsRange;
        [SerializeField] private ZombieEx.RandomRange _standUpAnimsRange;
        [SerializeField] private ZombieEx.RandomRange _walkAnimsRange;

        private int _currentHealth;
        public int CurrentDamage { get; private set; }

        private Animator _anim;
        public Animator Anim => _anim ??= _anim = GetComponent<Animator>();

        private Collider _mainCollider;
        private Collider MainCollider => _mainCollider ??= _mainCollider = GetComponent<Collider>();

        private NavMeshAgent _navAgent;
        private NavMeshAgent NavAgent => _navAgent ??= _navAgent = GetComponent<NavMeshAgent>();

        private Rigidbody[] _rigidBodies;
        private Rigidbody[] RBs => _rigidBodies ??= _rigidBodies = GetComponentsInChildren<Rigidbody>(true);

        private void Awake()
        {
            _zombies.Add(gameObject.GetInstanceID(), this);

            Anim.SetFloat(IDLE_ANIM_PARAM, _idleAnimsRange.Value);
            Anim.SetFloat(STAND_UP_ANIM_PARAM, _standUpAnimsRange.Value);
            Anim.SetFloat(WALK_ANIM_PARAM, _walkAnimsRange.Value);

            var randomSkinIndex = Random.Range(0, _skins.Length);
            for (var i = 0; i <  _skins.Length; i++)
                _skins[i].SetActive(randomSkinIndex == i);
            
            NavAgent.isStopped = true;
        }

        private void OnEnable() => Init();

        public void Init()
        {
            SetDeadState(false);
            _currentHealth = _healthRange.Value;
            CurrentDamage = _damageRange.Value;
        }

        // Можно вынести в интерфейс, но сейчас не актуально, так как враг лишь 1
        public void Hit(int damage)
        {
            _currentHealth -= damage;
            if (_currentHealth > 0)
                Anim.SetTrigger(STUMBLING_ANIM_PARAM);
            else
                SetDeadState();
        }

        private void SetDeadState(bool state = true) 
        {
            foreach (var rb in RBs)
                rb.gameObject.SetActive(state);
           
            Anim.enabled = !state;
        }

        private void Update() 
        {
            MainCollider.enabled = !this.IsStumbling();

            if (!Anim.enabled)
                return;

            var selectedVehicle = VehiclesSelector.SelectedVehicle;
            if (selectedVehicle == null)
                return;

            NavAgent.SetDestination(selectedVehicle.transform.position);
            transform.LookAt(NavAgent.steeringTarget);
        }
    }
}