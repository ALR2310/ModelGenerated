using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelGen.Model
{
    public class GetConnectString
    {
        private static GetConnectString _instance;
        public static GetConnectString Instance => _instance ?? (_instance = new GetConnectString());

        private string _ConnectString;

        public string ConnectString
        {
            get { return _ConnectString; }
            set
            {
                _ConnectString = value;
                OnDataChanged?.Invoke(_ConnectString);
            }
        }

        public event Action<string> OnDataChanged;
    }
}
