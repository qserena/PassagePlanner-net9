using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassagePlanner
{
    public class PassagePlannerSettings : ISettings
    {
        public object this[string propertyName]
        {
            get
            {
                return Properties.Settings.Default[propertyName];
            }
            set
            {
                Properties.Settings.Default[propertyName] = value;
            }
        }

        public void Save()
        {
            Properties.Settings.Default.Save();
        }
    }
}
