using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassagePlanner
{
    public class RescueCoordinatingCenter : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string Station { get; set; }
        public string Remarks { get; set; }
        public string ChannelOrPhone { get; set; }

        public RescueCoordinatingCenter(string station, string remarks, string channelOrPhone)
        {
            Station = station;
            Remarks = remarks;
            ChannelOrPhone = channelOrPhone;
        }

        protected virtual void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = System.Threading.Interlocked.CompareExchange(ref PropertyChanged, null, null);
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
