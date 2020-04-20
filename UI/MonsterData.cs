using System.Windows;
using System.Windows.Interop;
using MhwOverlay.Core;

namespace MhwOverlay.UI
{
    public class MonsterData : Bindable
    {
        public ulong Address { get; set; }
        private string name;
        public string Name { get { return name; } set { SetProperty(ref name, value); } }
        private float maxHP;
        public float MaxHP { get { return maxHP; } set { SetProperty(ref maxHP, value); NotifyPropertyChanged("Fraction"); } }
        private float hP;
        public float HP { get { return hP; } set { SetProperty(ref hP, value); NotifyPropertyChanged("Fraction"); } }

        public double Fraction { get { return HP / MaxHP; } }

        public override bool Equals(object obj)
        {
            var other = obj as MonsterData;
            if (other == null) return false;
            return this.Address == other.Address && this.Name == other.Name;
        }
        public override int GetHashCode() => (Name, Address).GetHashCode();
    }
}