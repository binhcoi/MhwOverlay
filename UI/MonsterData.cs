using System.Windows;
using System.Windows.Interop;
using MhwOverlay.Core;

namespace MhwOverlay.UI
{
    public class MonsterData
    {
        public string Name { get; set; }
        public float MaxHP { get; set; }
        public float HP { get; set; }

        public double Fraction { get { return HP / MaxHP; } }
    }
}