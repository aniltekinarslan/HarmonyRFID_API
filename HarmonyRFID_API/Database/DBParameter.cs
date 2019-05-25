using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HarmonyRFID_API.Database
{
    public enum DBTypes
    {
        BigInt, Int, Double, Char, NChar, NVarChar, VarChar, Text, NText, Image, Date, Time, DateTime, SmallDateTime, Bit, TinyInt
    }

    public class DBParameter
    {
        private string _name;
        public DBTypes Type { get; set; }
        public int size { get; set; }
        public object Value { get; set; }
        private ParameterDirection _direction = ParameterDirection.Input;

        public string Name
        {
            get { return _name; }
            set
            {
                if (value.Contains("@"))
                    _name = value;
                else
                    _name = "@" + value;
            }
        }

        public ParameterDirection Direction
        {
            get { return _direction; }
            set { _direction = value; }
        }

        public DBParameter(string Name, DBTypes Type, Object Value)
        {
            this.Name = Name;
            this.Type = Type;
            this.Value = Value;
        }

        public DBParameter(string Name, DBTypes Type, out Object Value, ParameterDirection direction)
        {
            this.Name = Name;
            this.Type = Type;
            Value = this.Value;
            this._direction = direction;
        }

        public DBParameter(string Name, DBTypes Type, int size, out Object Value, ParameterDirection direction)
        {
            this.Name = Name;
            this.size = size;
            Value = this.Value;
            this._direction = direction;
        }

        public DBParameter()
        {
        }
    }
}
