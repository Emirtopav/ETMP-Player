using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETMPData.DataEntities
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public abstract class BaseEntity : INotifyPropertyChanged
    {
        public int Id { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
