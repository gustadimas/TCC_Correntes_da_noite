using System.Collections.Generic;
using UnityEngine;

namespace CorrentesDaNoite.Player
{
    public class StonePile : MonoBehaviour
    {
        public static readonly List<StonePile> All = new List<StonePile>();

        private void OnEnable() => All.Add(this);
        private void OnDisable() => All.Remove(this);
    }
}