using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelGen.Model
{
    public class DataStore
    {
        private static DataStore _instance;
        public static DataStore Instance => _instance ?? (_instance = new DataStore());

        private string _sharedData;

        public string SharedData
        {
            get { return _sharedData; }
            set
            {
                _sharedData = value;
                OnDataChanged?.Invoke(_sharedData);
            }
        }

        public event Action<string> OnDataChanged;
    }
}
