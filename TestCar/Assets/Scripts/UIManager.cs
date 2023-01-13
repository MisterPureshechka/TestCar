using TestCar.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TestCar 
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private GameObject _selectRoot;
        [SerializeField] private GameObject _raceRoot;
        [SerializeField] private GameObject _deadRoot;

        private void OnEnable()
        {
            Vehicle.OnRaceDone.AddListener(RaceDone);
            VehiclesSelector.OnSelectDone.AddListener(SelectDone);

            _selectRoot.SetActive(true);
            _raceRoot.SetActive(false);
            _deadRoot.SetActive(false);
        }

        private void OnDisable()
        {
            Vehicle.OnRaceDone.RemoveListener(RaceDone);
            VehiclesSelector.OnSelectDone.RemoveListener(SelectDone);
        }

        private void RaceDone()
        {
            _deadRoot.SetActive(true);
        }

        private void SelectDone()
        {
            _selectRoot.SetActive(false);
            _raceRoot.SetActive(true);
        }

        public void Restart() 
        {
            SceneManager.LoadScene(0);
        }
    }
}