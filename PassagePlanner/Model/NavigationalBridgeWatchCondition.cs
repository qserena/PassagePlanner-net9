using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassagePlanner
{
    public class NavigationalBridgeWatchCondition
    {
        private string _abbreviation;
        private string _definitionText;

        public NavigationalBridgeWatchCondition()
        {
            Abbreviation = string.Empty;
            DefinitionText = string.Empty;
        }

        public NavigationalBridgeWatchCondition(string abbreviation, string definitionText)
        {
            Abbreviation = abbreviation;
            DefinitionText = definitionText;
        }

        // STATIC dirty flag takes care of the case where ANY NavigationalBridgeWatchCondition is dirty.
        public static bool IsDirty { get; set; }

        public string Abbreviation
        {
            get
            {
                return _abbreviation;
            }
            set
            {
                if (_abbreviation == value)
                {
                    return;
                }

                _abbreviation = value;
                IsDirty = true;
            }
        }

        public string DefinitionText
        {
            get
            {
                return _definitionText;
            }
            set
            {
                if (_definitionText == value)
                {
                    return;
                }

                _definitionText = value;
                IsDirty = true;
            }
        }
    }
}
