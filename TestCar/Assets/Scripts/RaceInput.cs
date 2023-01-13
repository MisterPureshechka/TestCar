using System;
using UnityEngine;

namespace TestCar
{
    public class RaceInput : MonoBehaviour
    {
        public static float VerticalInput { get; private set; }
        public static float HorizontalInput { get; private set; }

        public void SetVerticalInput(Single value) => VerticalInput = value;

        private void Update() => HorizontalInput = SimpleInput.GetAxis("Horizontal");
    }
}