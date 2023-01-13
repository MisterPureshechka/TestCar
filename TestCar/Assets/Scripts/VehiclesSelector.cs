using TestCar.Core;
using UnityEngine;
using UnityEngine.Events;

namespace TestCar
{
    public class VehiclesSelector : MonoBehaviour
    {
        public static UnityEvent OnSelectDone = new();
        public static Vehicle SelectedVehicle { get; set; }

        [SerializeField] private float _camSpeed;

        private static Transform _cameraTrans;
        private static Transform CameraTrans => _cameraTrans ??= Camera.allCameras[0].transform;

        private Vehicle[] _vehicles;
        private Transform _selectedVehicleShowPoint;
        private int _selectedVehicleIndex;

        private void Start()
        {
            _vehicles = Vehicle.VehiclesArray;
            SelectedVehicle = null;
            _cameraTrans = null;
        } 

        private void Update()
        {
            if (SelectedVehicle != null)
                return;

            _selectedVehicleShowPoint = _vehicles[_selectedVehicleIndex].ShowPoint;
            var deltaTime = Time.deltaTime * _camSpeed;
            var pos = Vector3.Lerp(CameraTrans.position, _selectedVehicleShowPoint.position, deltaTime);
            var rot = Quaternion.Lerp(CameraTrans.rotation, _selectedVehicleShowPoint.rotation, deltaTime);
            CameraTrans.SetPositionAndRotation(pos, rot);
        }

        public void NextVehicle()
        {
            _selectedVehicleIndex++;
            if (_selectedVehicleIndex >= _vehicles.Length)
                _selectedVehicleIndex = 0;
        }

        public void Select() 
        {
            SelectedVehicle = _vehicles[_selectedVehicleIndex];
            OnSelectDone.Invoke();
        } 
    }
}