using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Threading.Tasks;

namespace OnlineRPGServerV1
{
    public class JSONBuilder
    {
        private static readonly CultureInfo cultureInfo = CultureInfo.InvariantCulture;
        
        private StringBuilder stringBuilder;
        private int counter = 0;

        public JSONBuilder()
        {
            stringBuilder = new StringBuilder();

            stringBuilder.Append("{");
            counter = 0;
        }

        public JSONBuilder Add(String name, object value)
        {
            String valueText = "";
            if (value == null)
                valueText = "null";
            else if ((value is double) || (value is float))
                valueText = ((double)value).ToString(cultureInfo);
            else if (value is String)
                valueText = "'" + value + "'";
            else if (value is bool)
            {
                if ((bool)value == true)
                    valueText = "true";
                else
                    valueText = "false";
            }
            else
                valueText = value.ToString();

            if (counter == 0)
                stringBuilder.AppendFormat("'{0}':{1}", name, valueText);
            else
                stringBuilder.AppendFormat(", '{0}':{1}", name, valueText);

            counter++;

            return this;
        }

        public override string ToString()
        {
            String result = stringBuilder.ToString();

            return result + " }";
        }

        public JSONBuilder Clone()
        {
            String text = stringBuilder.ToString();

            JSONBuilder clone = new JSONBuilder();
            clone.counter = counter;
            clone.stringBuilder = new StringBuilder(text);

            return clone;
        }
    }
}
