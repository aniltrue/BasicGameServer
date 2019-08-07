using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace OnlineRPGClient
{
    public class GameProxy
    {
        private String name;
        private int id, playerNumber;

        public GameProxy(String value)
        {
            dynamic obj = JObject.Parse(value);

            name = obj["name"].ToString();
            id = obj["id"];
            playerNumber = obj["players"];
        }

        public String Name => name;
        public int ID => id;
        public int PlayerNumber => playerNumber;

        public override string ToString()
        {
            return name + "\t" + playerNumber;
        }
    }
}
