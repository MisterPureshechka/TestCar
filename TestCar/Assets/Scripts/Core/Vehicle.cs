using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TestCar.Core
{
    [RequireComponent(typeof(Rigidbody))]
    public class Vehicle : MonoBehaviour
    {
        [Serializable]
        private sealed class WheelData
        {
            private const float MIN_INPUUT_VALUE = 0.1f;

            [SerializeField] private bool _isActive;
            [SerializeField][Range(0f, 25f)] private float _steerAngle;
            [SerializeField] private WheelCollider _wheel;
            [SerializeField] private Transform _wheelBody;

            public void Set(float inputValue, float force, float rotate)
            {
                _wheel.motorTorque = inputValue * (_isActive ? force : 0);
                _wheel.steerAngle = Mathf.Lerp(0, _steerAngle, Mathf.Abs(rotate)) * (rotate >= 0f ? 1 : -1);
                _wheel.GetWorldPose(out var pos, out var rot);
                _wheelBody.position = pos;
                _wheelBody.rotation = rot;

                _wheel.brakeTorque = Mathf.Abs(inputValue) <= MIN_INPUUT_VALUE ? float.MaxValue : 0;
            }
        }

        [Serializable]
        private sealed class CameraData
        {
            [SerializeField] private Vector3 _posOffset = new(0f, 5f, -10f);
            [SerializeField] private Vector3 _rotOffset = new(-10f, 0f, 0f);
            [SerializeField] private float _camSpeed = 10f;

            private Transform _cameraTrans;
            private Transform CameraTrans => _cameraTrans ??= Camera.allCameras[0].transform;

            public void Set(float inputValue, Transform origin, float deltaTime)
            {
                var oldRot = CameraTrans.rotation;
                CameraTrans.LookAt(origin);
                deltaTime *= _camSpeed;
                var currentPosOffset = _posOffset;
                currentPosOffset.z *= inputValue < 0f ? -1f : 1f;
                CameraTrans.rotation = Quaternion.Lerp(oldRot, CameraTrans.rotation * Quaternion.Euler(_rotOffset), deltaTime);
                CameraTrans.position = Vector3.Lerp(CameraTrans.position, origin.position + origin.TransformDirection(currentPosOffset), deltaTime);
            }
        }

        public static UnityEvent OnRaceDone = new();

        //Можно перенести в ScriptableObject и при спавне применять параметры к машине.
        //Также можно и через скачанный конфиг параметры применять
        [SerializeField] private string _name;
        [SerializeField] private CameraData _camData;
        [SerializeField] private WheelData[] _wheels;
        [SerializeField] private float _force;
        [SerializeField] private float _maxSpeed;
        [SerializeField] private Vector3 _centerOfMass;
        [SerializeField] private Transform _showPoint;
        [SerializeField] private TextMeshPro _description;
        [SerializeField] private int _damage;
        [SerializeField] private int _health;
        [SerializeField] private Image _healthFiller;

        public static Vehicle[] VehiclesArray => _vehicles.ToArray();

        private static readonly HashSet<Vehicle> _vehicles = new();        

        private int _currentHealth;

        private Rigidbody _rigidBody;
        private Rigidbody RB => _rigidBody ??= _rigidBody = GetComponent<Rigidbody>();

        public Transform ShowPoint => _showPoint;
        private float Speed => transform.InverseTransformDirection(RB.velocity).z;

        private void OnEnable()
        {
            _vehicles.Add(this);
            VehiclesSelector.OnSelectDone.AddListener(OnRaceStart);
        } 

        private void OnDisable()
        {
            _vehicles.Remove(this);
            VehiclesSelector.OnSelectDone.RemoveListener(OnRaceStart);
        }

        private void OnRaceStart() => _description.gameObject.SetActive(false);

        private void Start()
        {
            _currentHealth = _health;
            RB.centerOfMass = _centerOfMass;
            _description.text = $"Name: {_name}\nForce: {_force}\nSpeed: {_maxSpeed}\nDamage: {_damage}\nHP: {_health}";
        }

        private void Update()
        {
            if (VehiclesSelector.SelectedVehicle != this)
                return;

            if (_currentHealth <= 0)
                return;

            foreach (var wheel in _wheels)
                wheel.Set(RaceInput.VerticalInput, Speed <= _maxSpeed ? _force : 0, RaceInput.HorizontalInput);

            _camData.Set(RaceInput.VerticalInput, transform, Time.deltaTime);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!Zombie.Zombies.TryGetValue(other.gameObject.GetInstanceID(), out var zombie))
                return;

            _currentHealth -= zombie.CurrentDamage;
            zombie.Hit(_damage);

            _healthFiller.fillAmount = (float)_currentHealth / (float)_health;

            if (_currentHealth <= 0)
                OnRaceDone.Invoke();
        }
    }
}