#include "JObject.h"

void JObject::read(const string& json)
{
	values = map<string, JType>{};

	auto keys = vector<string>();

	auto curvelyBreakCounter = size_t(0);

	string key = "";

	for (const auto& c : json)
	{
		if (c == '{')
			curvelyBreakCounter++;
		else if (c == '}')
			curvelyBreakCounter--;
		else if (curvelyBreakCounter == 1 && c == ',')
		{
			keys.push_back(key);
			key = "";
		}
		else if (key == "" && c == ' ')
			continue;
		else
			key += c;

		if (curvelyBreakCounter == 0)
			break;
	}

	if (key != "")
	{
		keys.push_back(key);
	}

	for (const auto& k : keys)
	{
		auto index = k.find(":");

		auto name = k.substr(0, index);
		name = name.substr(1, name.length() - 2);

		auto valueText = k.substr(index + 1);

		auto value = getValue(valueText);

		values.insert_or_assign(name, value);
	}
}

const JType JObject::getValue(const string& valueText) const
{
	if (valueText[0] == '\'')
		return JType(valueText.substr(1, valueText.length() - 2));
	if (valueText == "true" || valueText == "false")
		return JType(valueText == "true");
	if (valueText[0] == '{')
		return JType(false);
	if (valueText.find('.') != string::npos)
		return JType(stod(valueText));

	return JType(stoi(valueText));
}

JObject::operator string() const
{
	auto visitor = [](const auto& value) -> string
	{
		using type = decay_t<decltype(value)>;

		if constexpr (is_same_v<type, string>)
			return "'" + value + "'";

		else if constexpr (is_same_v<type, bool>)
			return value ? "true" : "false";

		else if constexpr (is_same_v<type, JObject>)
			return (string)(value);

		else if constexpr (is_same_v<type, int>)
			return to_string(value);

		else if constexpr (is_same_v<type, double>)
			return to_string(value);

		else
			static_assert(false, "unknown type!");
	};

	string json = "{ ";
	size_t counter = 0;

	for (const auto& [name, value] : values)
	{
		auto&& v = value;
		if (counter == 0)
			json += "'" + name + "':" + visit(visitor, value);
		else
			json += ", '" + name + "':" + visit(visitor, value);

		counter++;
	}

	return json + " }";
}
